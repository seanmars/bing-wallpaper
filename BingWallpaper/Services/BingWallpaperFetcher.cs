using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io;
using BingWallpaper.Model;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BingWallpaper.Services
{
    public class BingWallpaperFetcher
    {
        private readonly ILogger<BingWallpaperFetcher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly Flurl.Url _bingUrl = new("https://www.bing.com");
        private readonly string LocationCode = "zh-tw";

        public BingWallpaperFetcher(ILogger<BingWallpaperFetcher> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private async ValueTask<(string url, string description)> FetchImageInformation(CancellationToken cancellationToken = default)
        {
            var config = Configuration.Default
                .WithDefaultLoader(new LoaderOptions
                {
                    IsResourceLoadingEnabled = false
                })
                .WithDefaultCookies();

            var context = BrowsingContext.New(config);
            context.SetCookie(new Url(_bingUrl), $"_EDGE_S=mkt={LocationCode};");
            var document = await context.OpenAsync(_bingUrl.ToString(), cancellation: cancellationToken);

            var bgElement = document.Head.Children
                .FirstOrDefault(element => element.Id == "preloadBg");
            var infoElement = document.Body.QuerySelector("div#vs_cont > div.mc_caro > div > div.musCardCont > a.title");

            var imageUrl = bgElement == null ? string.Empty : bgElement.GetAttribute("href");
            var description = infoElement?.TextContent ?? string.Empty;

            return (url: imageUrl, description: description);
        }

        public async Task<FileContent?> FetchAsync(bool downloadImage, CancellationToken cancellationToken = default)
        {
            try
            {
                var (url, description) = await FetchImageInformation(cancellationToken);
                var uri = new Uri(_bingUrl.ToUri(), url);
                var queries = QueryHelpers.ParseQuery(uri.Query);
                if (!queries.TryGetValue("id", out var id))
                {
                    return null;
                }

                if (!queries.TryGetValue("rf", out var rf))
                {
                    return null;
                }

                var queryId = id.First();
                var ext = queryId.Split('.').Last();
                var fileName = $"{queryId.Split('_').First()}.{ext}";
                var queryRf = rf.First();
                var targetId = queryId.Replace(queryRf.Split('_')[1], "UHD");

                var uriForOrigin = _bingUrl.Reset().AppendPathSegment(uri.AbsolutePath)
                    .SetQueryParams(new
                    {
                        id = queryId
                    })
                    .ToUri();
                var uriFor4K = _bingUrl.Reset().AppendPathSegment(uri.AbsolutePath)
                    .SetQueryParams(new
                    {
                        id = $"{targetId}.{ext}"
                    })
                    .ToUri();

                Stream stream = default;
                if (downloadImage)
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(uriFor4K, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                }

                return new FileContent
                {
                    FileName = fileName,
                    Description = description,
                    UrlForOrigin = uriForOrigin.ToString(),
                    UrlFor4K = uriFor4K.ToString(),
                    Stream = stream
                };
            }
            catch (Exception e)
            {
                _logger.LogCritical("{Exception}", e);
                return null;
            }
        }

        public Task<FileContent?> FetchAndDownloadAsync(CancellationToken cancellationToken = default)
        {
            return FetchAsync(true, cancellationToken);
        }

        public Task<FileContent?> FetchOnlyUrlAsync(CancellationToken cancellationToken = default)
        {
            return FetchAsync(false, cancellationToken);
        }
    }
}
