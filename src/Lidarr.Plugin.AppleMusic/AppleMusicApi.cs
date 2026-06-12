using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Plugin.AppleMusic
{
    public class AppleMusicApi
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static AppleMusicSearchResponse SearchAlbums(string baseUrl, string query, string artist, int limit = 25)
        {
            var url = $"{baseUrl.TrimEnd('/')}/search/albums?query={Uri.EscapeDataString(query)}&artist={Uri.EscapeDataString(artist)}&limit={limit}";
            var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<AppleMusicSearchResponse>(json);
        }

        public static AppleMusicSearchResponse SearchArtists(string baseUrl, string query, int limit = 25)
        {
            var url = $"{baseUrl.TrimEnd('/')}/search/artists?query={Uri.EscapeDataString(query)}&limit={limit}";
            var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<AppleMusicSearchResponse>(json);
        }

        public static AppleMusicDownloadResponse StartDownload(string baseUrl, string albumUrl, string codec, string outputPath, string outputFormat = "flac")
        {
            var url = $"{baseUrl.TrimEnd('/')}/download";
            var payload = new
            {
                url = albumUrl,
                codec = codec,
                output_format = outputFormat,
                output_path = outputPath
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<AppleMusicDownloadResponse>(json);
        }

        public static AppleMusicDownloadStatus GetDownloadStatus(string baseUrl, string downloadId)
        {
            var url = $"{baseUrl.TrimEnd('/')}/download/{downloadId}";
            var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<AppleMusicDownloadStatus>(json);
        }

        public static void CancelDownload(string baseUrl, string downloadId)
        {
            var url = $"{baseUrl.TrimEnd('/')}/download/{downloadId}";
            var response = _httpClient.DeleteAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public static bool CheckHealth(string baseUrl)
        {
            try
            {
                var url = $"{baseUrl.TrimEnd('/')}/health";
                var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class AppleMusicSearchResponse
    {
        [JsonProperty("results")]
        public List<AppleMusicAlbumResult> Results { get; set; } = new List<AppleMusicAlbumResult>();
    }

    public class AppleMusicAlbumResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public int TrackCount { get; set; }

        [JsonProperty("total_duration_ms")]
        public long TotalDurationMs { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("cover_url")]
        public string CoverUrl { get; set; }

        [JsonProperty("available_codecs")]
        public List<string> AvailableCodecs { get; set; } = new List<string>();
    }

    public class AppleMusicDownloadResponse
    {
        [JsonProperty("download_id")]
        public string DownloadId { get; set; }
    }

    public class AppleMusicDownloadStatus
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("progress")]
        public double Progress { get; set; }

        [JsonProperty("output_path")]
        public string OutputPath { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
