using System.Collections.Generic;
using System.Text;
using BingWallpaper.Helper;
using BingWallpaper.Model;

namespace BingWallpaper.Services
{
    public class MarkdownGenerator
    {
        private readonly StringBuilder _stringBuilder;

        private static string RepeatString(string s, int n)
        {
            return new StringBuilder(s.Length * n)
                .AppendJoin(s, new string[n + 1])
                .ToString();
        }

        public MarkdownGenerator()
        {
            _stringBuilder = new StringBuilder();
        }

        private MarkdownGenerator AddHead(string text, byte level)
        {
            var head = RepeatString("#", level);
            _stringBuilder.AppendFormat("{0} {1}", head, text)
                .AppendLine();
            return this;
        }

        public MarkdownGenerator AddH1(string text)
        {
            AddHead(text, 1);
            return this;
        }

        public MarkdownGenerator AddH2(string text)
        {
            AddHead(text, 2);
            return this;
        }

        public MarkdownGenerator AddH3(string text)
        {
            AddHead(text, 3);
            return this;
        }

        public MarkdownGenerator AddH4(string text)
        {
            AddHead(text, 4);
            return this;
        }

        public MarkdownGenerator AddH5(string text)
        {
            AddHead(text, 5);
            return this;
        }

        public MarkdownGenerator AddParagraph(string content)
        {
            _stringBuilder.AppendLine(content)
                .AppendLine();
            return this;
        }

        public MarkdownGenerator AddTable(int column, List<BingImage> items)
        {
            const string headerColumn = "      ";
            const string headerSplitColumn = " :----: ";

            if (column == 0 || items == null || items.Count == 0)
            {
                return this;
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append('|');
            for (var i = 0; i < column; i++)
            {
                sb.Append(headerColumn)
                    .Append('|');
            }

            sb.AppendLine().Append('|');
            for (var i = 0; i < column; i++)
            {
                sb.Append(headerSplitColumn)
                    .Append('|');
            }

            sb.AppendLine();
            for (var idx = 1; idx <= items.Count; idx++)
            {
                var item = items[idx - 1];
                sb.Append('|')
                    .AppendFormat("![]({0})<br />[Download {1}]({2})", item.UrlForOrigin, DateHelper.Format(item.Date), item.UrlFor4K);

                if (idx % column == 0)
                {
                    sb.AppendLine("|");
                }
            }

            if (items.Count % column != 0)
            {
                sb.AppendLine("|");
            }

            _stringBuilder.Append(sb);

            return this;
        }

        public static MarkdownGenerator Create(WallpaperData data, string title = "")
        {
            var generator = new MarkdownGenerator();
            return generator.AddH1(title)
                .AddParagraph($"![]({data.Today.UrlForOrigin})")
                .AddParagraph($"[Download Latest Wallpaper]({data.Today.UrlFor4K})")
                .AddTable(7, data.SortedImages);
        }

        public string ToMarkdown()
        {
            return _stringBuilder.ToString();
        }
    }
}