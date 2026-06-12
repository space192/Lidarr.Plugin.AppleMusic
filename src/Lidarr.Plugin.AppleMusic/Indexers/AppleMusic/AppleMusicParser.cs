using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Http;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Plugin.AppleMusic;

namespace NzbDrone.Core.Indexers.AppleMusic
{
    public class AppleMusicParser : IParseIndexerResponse
    {
        public AppleMusicIndexerSettings Settings { get; set; }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse response)
        {
            var releases = new List<ReleaseInfo>();
            var jsonResponse = JsonConvert.DeserializeObject<AppleMusicSearchResponse>(response.Content);

            if (jsonResponse?.Results == null)
            {
                return releases;
            }

            foreach (var album in jsonResponse.Results)
            {
                releases.AddRange(ProcessAlbumResult(album));
            }

            return releases
                .OrderByDescending(o => o.Size)
                .ToList();
        }

        private IEnumerable<ReleaseInfo> ProcessAlbumResult(AppleMusicAlbumResult album)
        {
            var qualityTiers = GetQualityTiers(album.AvailableCodecs);

            foreach (var tier in qualityTiers)
            {
                yield return ToReleaseInfo(album, tier);
            }
        }

        private static List<QualityTier> GetQualityTiers(List<string> availableCodecs)
        {
            var tiers = new List<QualityTier>();

            if (availableCodecs == null || !availableCodecs.Any())
            {
                // Default to AAC if no codec info available
                tiers.Add(new QualityTier("aac", "AAC", "256", "AAC (M4A) 256kbps", 32000));
                return tiers;
            }

            if (availableCodecs.Contains("aac", StringComparer.OrdinalIgnoreCase))
            {
                tiers.Add(new QualityTier("aac", "AAC", "256", "AAC (M4A) 256kbps", 32000));
            }

            if (availableCodecs.Contains("alac", StringComparer.OrdinalIgnoreCase))
            {
                tiers.Add(new QualityTier("alac", "ALAC", "Lossless", "ALAC (M4A) Lossless", 176400));
            }

            if (availableCodecs.Contains("atmos", StringComparer.OrdinalIgnoreCase))
            {
                tiers.Add(new QualityTier("atmos", "EAC3", "Atmos", "Dolby Atmos (M4A)", 96000));
            }

            return tiers;
        }

        private static ReleaseInfo ToReleaseInfo(AppleMusicAlbumResult album, QualityTier tier)
        {
            var publishDate = DateTime.UtcNow;
            if (album.Year > 0)
            {
                publishDate = new DateTime(album.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            // Estimate size: bytesPerSecond * durationInSeconds
            var durationSeconds = album.TotalDurationMs / 1000.0;
            var estimatedSize = (long)(tier.BytesPerSecond * durationSeconds);

            var title = $"{album.Artist} - {album.Title}";

            if (album.Year > 0)
            {
                title += $" ({album.Year})";
            }

            title += $" [{tier.FormatLabel}] [WEB]";

            return new ReleaseInfo
            {
                Guid = $"AppleMusic-{album.Id}-{tier.CodecId}",
                Artist = album.Artist,
                Album = album.Title,
                Title = title,
                DownloadUrl = album.Url,
                InfoUrl = album.Url,
                PublishDate = publishDate,
                Size = estimatedSize,
                Codec = tier.Codec,
                Container = tier.Container,
                DownloadProtocol = nameof(AppleMusicDownloadProtocol)
            };
        }

        private class QualityTier
        {
            public string CodecId { get; }
            public string Codec { get; }
            public string Container { get; }
            public string FormatLabel { get; }
            public int BytesPerSecond { get; }

            public QualityTier(string codecId, string codec, string container, string formatLabel, int bytesPerSecond)
            {
                CodecId = codecId;
                Codec = codec;
                Container = container;
                FormatLabel = formatLabel;
                BytesPerSecond = bytesPerSecond;
            }
        }
    }
}
