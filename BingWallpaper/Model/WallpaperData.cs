using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BingWallpaper.Model
{
    public class WallpaperData
    {
        public DateTime LastFetchTime { get; set; }
        public BingImage Today { get; set; }
        public List<BingImage> Images { get; set; }

        [JsonIgnore]
        public List<BingImage> SortedImages
        {
            get { return Images.OrderByDescending(i => i.Date).ToList(); }
        }

        public WallpaperData()
        {
            Images = new List<BingImage>();
        }
    }
}