using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.AppleMusic
{
    public class AppleMusic : HttpIndexerBase<AppleMusicIndexerSettings>
    {
        public override string Name => "Apple Music";
        public override string Protocol => nameof(AppleMusicDownloadProtocol);
        public override bool SupportsRss => false;
        public override bool SupportsSearch => true;
        public override int PageSize => 25;
        public override TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public AppleMusic(IHttpClient httpClient,
            IIndexerStatusService indexerStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new AppleMusicRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new AppleMusicParser()
            {
                Settings = Settings
            };
        }
    }
}
