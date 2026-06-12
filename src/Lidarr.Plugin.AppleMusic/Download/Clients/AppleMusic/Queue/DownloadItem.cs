using NzbDrone.Core.Download;

namespace NzbDrone.Core.Download.Clients.AppleMusic.Queue
{
    public class DownloadItem
    {
        public string ID { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public string AlbumUrl { get; set; }
        public string Codec { get; set; }
        public string FormatLabel { get; set; }
        public string OutputPath { get; set; }
        public long TotalSize { get; set; }
        public long DownloadedSize { get; set; }
        public DownloadItemStatus Status { get; set; }
        public string Error { get; set; }

        public double Progress
        {
            get
            {
                if (TotalSize <= 0)
                {
                    return 0;
                }

                return (double)DownloadedSize / TotalSize;
            }
        }

        public DownloadItem()
        {
            Status = DownloadItemStatus.Queued;
        }
    }
}
