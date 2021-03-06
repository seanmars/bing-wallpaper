using System.IO;

namespace BingWallpaper.Model
{
    public struct FileContent
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public Stream Stream { get; set; }
    }
}