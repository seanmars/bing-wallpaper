using System.Collections.Generic;

namespace BingWallpaper.Model
{
    public class WallpaperData
    {
        public BingImage Today { get; set; }
        public List<BingImage> Images { get; set; }

        public WallpaperData()
        {
            Images = new List<BingImage>();
        }
    }
}