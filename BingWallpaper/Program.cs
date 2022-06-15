using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BingWallpaper.Data;
using BingWallpaper.Model;
using BingWallpaper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BingWallpaper
{
    class Program
    {
        private static LocalDb _db;
        private static string _outputPath;

        public static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            var resFolder = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("ResourcePath"));
            var dbPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("DataSource"));
            _outputPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("OutputPath"));

            Directory.CreateDirectory(resFolder);
            _db = new LocalDb(dbPath);

            await using var provider = RegisterServices(configuration);
            var logger = provider.GetRequiredService<ILogger<Program>>();
            try
            {
                await Run(provider, cts.Token);
            }
            catch (Exception e)
            {
                logger.LogCritical("{Exception}", e);
            }
            finally
            {
                logger.LogInformation("Done");
            }
        }

        private static async Task Run(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var fetcher = serviceProvider.GetRequiredService<BingWallpaperFetcher>();
            var fileContent = await fetcher.FetchOnlyUrlAsync(cancellationToken);
            _db.AddTodayWallpaper(fileContent);
            var md = MarkdownGenerator.Create(_db.Data, "Wallpaper");
            await File.WriteAllTextAsync(_outputPath, md.ToMarkdown(), cancellationToken);

            await _db.SaveData();
        }

        private static ServiceProvider RegisterServices(IConfiguration configuration)
        {
            var services = new ServiceCollection()
                .AddLogging(opt =>
                {
                    opt.ClearProviders()
                        .AddConfiguration(configuration.GetSection("Logging"))
                        .AddConsole();
                });
            services.AddHttpClient();

            services.AddSingleton<BingWallpaperFetcher>();

            return services.BuildServiceProvider(true);
        }

        private static async Task SaveImageAsync(string resourceRootPath, FileContent? fileContent,
            CancellationToken cancellationToken = default)
        {
            if (!fileContent.HasValue)
            {
                return;
            }

            var content = fileContent.Value;
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var rex = new Regex($"[{Regex.Escape(regexSearch)}]");
            var illegalName = rex.Replace(content.FileName, "");
            var fileName = illegalName;

            var path = Path.Combine(resourceRootPath, fileName);
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.Stream.CopyToAsync(fs, cancellationToken);
        }
    }
}