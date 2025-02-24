using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    public class SubtitleParser
    {
        private readonly Dictionary<SubtitlesFormat, ISubtitleParser> _subFormatToParser = new Dictionary<SubtitlesFormat, ISubtitleParser>
        {
            { SubtitlesFormat.SubRipFormat, new SrtParser() },
            { SubtitlesFormat.SubStationAlphaFormat, new SsaParser() },
            { SubtitlesFormat.WebVttFormat, new VttParser() },
        };

        public SubtitleParser() { }

        /// <summary>
        /// Gets the most likely format based on the file’s extension.
        /// </summary>
        public SubtitlesFormat GetMostLikelyFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension))
            {
                foreach (SubtitlesFormat format in SubtitlesFormat.SupportedSubtitlesFormats)
                {
                    if (format.Extension != null && Regex.IsMatch(extension, format.Extension, RegexOptions.IgnoreCase))
                    {
                        return format;
                    }
                }
            }
            return null;
        }

        public List<SubtitleItem> ParseText(string subtitleContent, Encoding? encoding = null, SubtitlesFormat? preferredFormat = null)
        {
            if (string.IsNullOrWhiteSpace(subtitleContent))
                throw new ArgumentException("Subtitle text cannot be null or empty.", nameof(subtitleContent));

            encoding ??= Encoding.UTF8;
            using MemoryStream stream = new MemoryStream(encoding.GetBytes(subtitleContent));
            return ParseStream(stream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required }, preferredFormat);
        }

        public List<SubtitleItem> ParseStream(Stream stream)
        {
            return ParseStream(stream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        public List<SubtitleItem> ParseStream(Stream stream, SubtitleParserOptions options, SubtitlesFormat subFormat = null)
        {
            Dictionary<SubtitlesFormat, ISubtitleParser> dictionary = subFormat != null ?
                _subFormatToParser
                .OrderBy(dic => Math.Abs(string.Compare(dic.Key.Name, subFormat.Name, StringComparison.Ordinal)))
                .ToDictionary(entry => entry.Key, entry => entry.Value)
                : _subFormatToParser;

            return ParseStream(stream, options, dictionary);
        }

        public List<SubtitleItem> ParseStream(Stream stream, SubtitleParserOptions options, Dictionary<SubtitlesFormat, ISubtitleParser> subFormatDictionary)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Cannot parse a non-readable stream");

            Stream seekableStream = stream;
            if (!stream.CanSeek)
            {
                seekableStream = StreamHelpers.CopyStream(stream);
                seekableStream.Seek(0, SeekOrigin.Begin);
            }

            subFormatDictionary = subFormatDictionary ?? _subFormatToParser;
            foreach (KeyValuePair<SubtitlesFormat, ISubtitleParser> kvp in subFormatDictionary)
            {
                if (seekableStream.CanSeek)
                    seekableStream.Position = 0;
                try
                {
                    ISubtitleParser subtitleParser = kvp.Value;
                    List<SubtitleItem> items = subtitleParser.ParseStream(seekableStream, options);
                    if (items != null && items.Any())
                        return items;
                }
                catch (Exception)
                {
                    // Continue with next parser.
                    continue;
                }
            }

            if (seekableStream.CanSeek)
                seekableStream.Position = 0;
            string firstCharsOfFile = LogFirstCharactersOfStream(seekableStream, 500, options.Encoding);
            string message = string.Format("All the subtitles parsers failed to parse the following stream:{0}", firstCharsOfFile);
            throw new ArgumentException(message);
        }

        private string LogFirstCharactersOfStream(Stream stream, int nbOfCharactersToPrint, Encoding encoding)
        {
            string message = "";
            if (stream.CanRead)
            {
                if (stream.CanSeek)
                    stream.Position = 0;
                StreamReader reader = new StreamReader(stream, encoding, true);
                char[] buffer = new char[nbOfCharactersToPrint];
                reader.ReadBlock(buffer, 0, nbOfCharactersToPrint);
                message = string.Format("Parsing of subtitle stream failed. Beginning of sub stream:\n{0}", string.Join("", buffer));
            }
            else
            {
                message = string.Format("Tried to log the first {0} characters of a closed stream", nbOfCharactersToPrint);
            }
            return message;
        }
    }
}