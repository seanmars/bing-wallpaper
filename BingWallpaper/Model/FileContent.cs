using System.IO;

namespace BingWallpaper.Model
{
    public struct FileContent
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public string UrlForOrigin { get; set; }
        public string UrlFor4K { get; set; }
        public Stream Stream { get; set; }
        
    }
}