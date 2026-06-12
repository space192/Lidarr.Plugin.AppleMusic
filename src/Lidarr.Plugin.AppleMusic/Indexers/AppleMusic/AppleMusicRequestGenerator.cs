using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.AppleMusic
{
    public class AppleMusicRequestGenerator : IIndexerRequestGenerator
    {
        public AppleMusicIndexerSettings Settings { get; set; }
        public Logger Logger { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            // No RSS support; return a dummy request so Lidarr has something to test when saving settings
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetAlbumSearchRequests("test", "test"));
            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var chain = new IndexerPageableRequestChain();
            chain.AddTier(GetAlbumSearchRequests(searchCriteria.AlbumQuery, searchCriteria.ArtistQuery));
            return chain;
        }

        public IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var chain = new IndexerPageableRequestChain();
            chain.AddTier(GetArtistSearchRequests(searchCriteria.ArtistQuery));
            return chain;
        }

        private IEnumerable<IndexerRequest> GetAlbumSearchRequests(string albumQuery, string artistQuery)
        {
            var baseUrl = Settings.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/search/albums?query={Uri.EscapeDataString(albumQuery)}&artist={Uri.EscapeDataString(artistQuery)}&limit=25";

            var req = new IndexerRequest(url, HttpAccept.Json);
            req.HttpRequest.Method = System.Net.Http.HttpMethod.Get;
            yield return req;
        }

        private IEnumerable<IndexerRequest> GetArtistSearchRequests(string artistQuery)
        {
            var baseUrl = Settings.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/search/artists?query={Uri.EscapeDataString(artistQuery)}&limit=25";

            var req = new IndexerRequest(url, HttpAccept.Json);
            req.HttpRequest.Method = System.Net.Http.HttpMethod.Get;
            yield return req;
        }
    }
}
