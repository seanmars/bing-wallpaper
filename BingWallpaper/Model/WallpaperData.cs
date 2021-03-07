using System.Collections.Generic;
using System.Linq;

namespace BingWallpaper.Model
{
    public class WallpaperData
    {
        public BingImage Today { get; set; }
        public List<BingImage> Images { get; set; }

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