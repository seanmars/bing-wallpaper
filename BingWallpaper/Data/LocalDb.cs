using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BingWallpaper.Helper;
using BingWallpaper.Model;
using Newtonsoft.Json;

namespace BingWallpaper.Data
{
    public class LocalDb
    {
        private readonly string _dataSource;
        public WallpaperData Data { get; private set; }

        public LocalDb(string path)
        {
            _dataSource = path;
            if (!File.Exists(path))
            {
                Data = new WallpaperData();
                return;
            }

            InitDatabase();
        }

        private void InitDatabase()
        {
            using var file = File.OpenText(_dataSource);
            using var reader = new JsonTextReader(file);
            var serializer = new JsonSerializer();
            Data = serializer.Deserialize<WallpaperData>(reader);
        }

        private bool IsExists(BingImage bingImage)
        {
            return Data.Images.Any(i =>
                i.Date.Date == bingImage.Date.Date ||
                i.UrlFor4K == bingImage.UrlFor4K);
        }

        public async Task SaveData()
        {
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            await using var sw = new StreamWriter(_dataSource);
            using var writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, Data);
        }

        public void AddWallpaper(BingImage image)
        {
            Data.Images ??= new List<BingImage>();
            Data.Images.Add(image);
        }

        public BingImage? AddWallpaper(FileContent? fileContent)
        {
            if (!fileContent.HasValue)
            {
                return null;
            }

            var image = new BingImage
            {
                Date = DateHelper.Today,
                UrlForOrigin = fileContent.Value.UrlForOrigin,
                UrlFor4K = fileContent.Value.UrlFor4K
            };

            if (IsExists(image))
            {
                return null;
            }

            AddWallpaper(image);

            return image;
        }

        public void AddTodayWallpaper(FileContent? fileContent)
        {
            var image = AddWallpaper(fileContent);
            if (image.HasValue)
            {
                SetToday(image.Value);
            }
        }

        public void SetToday(BingImage image)
        {
            Data.Today = image;
        }
    }
}