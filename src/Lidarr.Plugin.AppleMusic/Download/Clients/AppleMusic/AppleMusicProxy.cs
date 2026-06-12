using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients.AppleMusic.Queue;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Plugin.AppleMusic;

namespace NzbDrone.Core.Download.Clients.AppleMusic
{
    public interface IAppleMusicProxy
    {
        List<DownloadClientItem> GetQueue(AppleMusicSettings settings);
        string Download(RemoteAlbum remoteAlbum, AppleMusicSettings settings);
        void RemoveFromQueue(string downloadId, AppleMusicSettings settings);
    }

    public class AppleMusicProxy : IAppleMusicProxy
    {
        private readonly ICached<DateTime?> _startTimeCache;
        private readonly ConcurrentDictionary<string, DownloadItem> _activeDownloads;
        private readonly Logger _logger;

        public AppleMusicProxy(ICacheManager cacheManager, Logger logger)
        {
            _startTimeCache = cacheManager.GetCache<DateTime?>(GetType(), "startTimes");
            _activeDownloads = new ConcurrentDictionary<string, DownloadItem>();
            _logger = logger;
        }

        public List<DownloadClientItem> GetQueue(AppleMusicSettings settings)
        {
            var items = new List<DownloadClientItem>();

            foreach (var kvp in _activeDownloads.ToArray())
            {
                var downloadItem = kvp.Value;
                var downloadId = kvp.Key;

                try
                {
                    var status = AppleMusicApi.GetDownloadStatus(settings.ApiBaseUrl, downloadId);

                    downloadItem.Status = MapStatus(status.Status);
                    downloadItem.OutputPath = status.OutputPath ?? downloadItem.OutputPath;
                    downloadItem.Error = status.Error;

                    if (downloadItem.TotalSize > 0 && status.Progress > 0)
                    {
                        downloadItem.DownloadedSize = (long)(downloadItem.TotalSize * status.Progress);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to get status for download {0}", downloadId);
                }

                items.Add(ToDownloadClientItem(downloadItem));
            }

            // Remove completed items that have been picked up
            var completedIds = _activeDownloads
                .Where(x => x.Value.Status == DownloadItemStatus.Completed)
                .Select(x => x.Key)
                .ToList();

            return items;
        }

        public string Download(RemoteAlbum remoteAlbum, AppleMusicSettings settings)
        {
            var release = remoteAlbum.Release;
            var codec = ParseCodecFromRelease(release, settings);
            var albumUrl = release.DownloadUrl;

            var artist = release.Artist ?? "Unknown Artist";
            var album = release.Album ?? "Unknown Album";
            var outputPath = settings.DownloadPath.TrimEnd('/');

            var outputFormat = ((AppleMusicOutputFormat)settings.OutputFormat) switch
            {
                AppleMusicOutputFormat.FLAC => "flac",
                AppleMusicOutputFormat.M4A => "m4a",
                AppleMusicOutputFormat.WAV => "wav",
                AppleMusicOutputFormat.MP3 => "mp3",
                _ => "flac"
            };

            _logger.Info("Starting Apple Music download: {0} - {1} [{2}] -> {3}", artist, album, codec, outputFormat);

            var response = AppleMusicApi.StartDownload(settings.ApiBaseUrl, albumUrl, codec, outputPath, outputFormat);
            var downloadId = response.DownloadId;

            var downloadItem = new DownloadItem
            {
                ID = downloadId,
                Artist = artist,
                Album = album,
                Title = $"{artist} - {album}",
                AlbumUrl = albumUrl,
                Codec = codec,
                FormatLabel = GetFormatLabel(codec),
                OutputPath = outputPath,
                TotalSize = release.Size,
                Status = DownloadItemStatus.Queued
            };

            _activeDownloads.TryAdd(downloadId, downloadItem);

            return downloadId;
        }

        public void RemoveFromQueue(string downloadId, AppleMusicSettings settings)
        {
            try
            {
                AppleMusicApi.CancelDownload(settings.ApiBaseUrl, downloadId);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to cancel download {0} on the API", downloadId);
            }

            _activeDownloads.TryRemove(downloadId, out _);
            _startTimeCache.Remove(downloadId);
        }

        private string ParseCodecFromRelease(ReleaseInfo release, AppleMusicSettings settings)
        {
            // Try to extract codec from the Guid (e.g., "AppleMusic-1234567890-alac")
            if (release.Guid != null && release.Guid.Contains("-"))
            {
                var parts = release.Guid.Split('-');
                if (parts.Length >= 3)
                {
                    var codec = parts[parts.Length - 1].ToLowerInvariant();
                    if (codec == "aac" || codec == "alac" || codec == "atmos")
                    {
                        return codec;
                    }
                }
            }

            // Fall back to preferred codec from settings
            return ((AppleMusicCodec)settings.PreferredCodec) switch
            {
                AppleMusicCodec.AAC => "aac",
                AppleMusicCodec.ALAC => "alac",
                AppleMusicCodec.Atmos => "atmos",
                _ => "alac"
            };
        }

        private static string GetFormatLabel(string codec)
        {
            return codec.ToLowerInvariant() switch
            {
                "aac" => "AAC (M4A) 256kbps",
                "alac" => "ALAC (M4A) Lossless",
                "atmos" => "Dolby Atmos (M4A)",
                _ => codec
            };
        }

        private static DownloadItemStatus MapStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "queued" => DownloadItemStatus.Queued,
                "downloading" => DownloadItemStatus.Downloading,
                "completed" => DownloadItemStatus.Completed,
                "failed" => DownloadItemStatus.Failed,
                _ => DownloadItemStatus.Queued
            };
        }

        private DownloadClientItem ToDownloadClientItem(DownloadItem x)
        {
            var title = $"{x.Artist} - {x.Album} [WEB] [{x.FormatLabel}]";

            var item = new DownloadClientItem
            {
                DownloadId = x.ID,
                Title = title,
                TotalSize = x.TotalSize,
                RemainingSize = x.TotalSize - x.DownloadedSize,
                RemainingTime = GetRemainingTime(x),
                Status = x.Status,
                CanMoveFiles = true,
                CanBeRemoved = true,
            };

            if (x.OutputPath.IsNotNullOrWhiteSpace())
            {
                item.OutputPath = new OsPath(x.OutputPath);
            }

            if (x.Error.IsNotNullOrWhiteSpace())
            {
                item.Message = x.Error;
            }

            return item;
        }

        private TimeSpan? GetRemainingTime(DownloadItem x)
        {
            if (x.Status == DownloadItemStatus.Completed)
            {
                _startTimeCache.Remove(x.ID);
                return null;
            }

            if (x.Progress <= 0)
            {
                return null;
            }

            var started = _startTimeCache.Find(x.ID);
            if (started == null)
            {
                started = DateTime.UtcNow;
                _startTimeCache.Set(x.ID, started);
                return null;
            }

            var elapsed = DateTime.UtcNow - started;
            var progress = Math.Min(x.Progress, 1.0);

            return TimeSpan.FromTicks((long)(elapsed.Value.Ticks * (1 - progress) / progress));
        }
    }
}
