using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Plugin.AppleMusic;

namespace NzbDrone.Core.Download.Clients.AppleMusic
{
    public class AppleMusic : DownloadClientBase<AppleMusicSettings>
    {
        private readonly IAppleMusicProxy _proxy;

        public AppleMusic(IAppleMusicProxy proxy,
                          IConfigService configService,
                          IDiskProvider diskProvider,
                          IRemotePathMappingService remotePathMappingService,
                          ILocalizationService localizationService,
                          Logger logger)
            : base(configService, diskProvider, remotePathMappingService, localizationService, logger)
        {
            _proxy = proxy;
        }

        public override string Protocol => nameof(AppleMusicDownloadProtocol);

        public override string Name => "Apple Music";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var queue = _proxy.GetQueue(Settings);

            foreach (var item in queue)
            {
                item.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false);
            }

            return queue;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(item);
            }

            _proxy.RemoveFromQueue(item.DownloadId, Settings);
        }

        public override Task<string> Download(RemoteAlbum remoteAlbum, IIndexer indexer)
        {
            var downloadId = _proxy.Download(remoteAlbum, Settings);
            return Task.FromResult(downloadId);
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = true,
                OutputRootFolders = new() { new OsPath(Settings.DownloadPath) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            if (!AppleMusicApi.CheckHealth(Settings.ApiBaseUrl))
            {
                failures.Add(new ValidationFailure("ApiBaseUrl", "Unable to connect to the Apple Music API service. Check the URL and ensure the service is running."));
            }
        }
    }
}
