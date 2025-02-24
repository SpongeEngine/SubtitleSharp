using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Provides functionality for parsing subtitles from various formats.
    /// 
    /// This class selects the appropriate subtitle parser based on file extension or preferred format
    /// and delegates parsing to the underlying format-specific parser.
    /// </summary>
    public class SubtitleParser
    {
        private readonly Dictionary<SubtitlesFormat, ISubtitleParser> _subFormatToParser = new Dictionary<SubtitlesFormat, ISubtitleParser>
        {
            { SubtitlesFormat.SubRipFormat, new SrtParser() },
            { SubtitlesFormat.SubStationAlphaFormat, new SsaParser() },
            { SubtitlesFormat.WebVttFormat, new VttParser() },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SubtitleParser"/> class.
        /// </summary>
        public SubtitleParser() { }

        /// <summary>
        /// Determines the most likely subtitle format based on the file extension.
        /// </summary>
        /// <param name="fileName">The subtitle file name.</param>
        /// <returns>
        /// A <see cref="SubtitlesFormat"/> that best matches the file extension, or <c>null</c> if no match is found.
        /// </returns>
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

        /// <summary>
        /// Parses subtitle content provided as a string.
        /// </summary>
        /// <param name="subtitleContent">The subtitle content.</param>
        /// <param name="encoding">The text encoding (defaults to UTF-8 if null).</param>
        /// <param name="preferredFormat">An optional preferred subtitle format.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects extracted from the content.</returns>
        /// <exception cref="ArgumentException">Thrown if the subtitle content is null or empty.</exception>
        public List<SubtitleItem> ParseText(string subtitleContent, Encoding? encoding = null, SubtitlesFormat? preferredFormat = null)
        {
            if (string.IsNullOrWhiteSpace(subtitleContent))
                throw new ArgumentException("Subtitle text cannot be null or empty.", nameof(subtitleContent));

            encoding ??= Encoding.UTF8;
            using MemoryStream stream = new MemoryStream(encoding.GetBytes(subtitleContent));
            return ParseStream(stream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required }, preferredFormat);
        }

        /// <summary>
        /// Parses subtitles from a stream using default options (UTF-8 and required timecodes).
        /// </summary>
        /// <param name="stream">The input stream containing subtitle data.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects parsed from the stream.</returns>
        public List<SubtitleItem> ParseStream(Stream stream)
        {
            return ParseStream(stream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        /// <summary>
        /// Parses subtitles from a stream using the specified options and an optional preferred format.
        /// </summary>
        /// <param name="stream">The input stream containing subtitle data.</param>
        /// <param name="options">Parser options including encoding and timecode mode.</param>
        /// <param name="subFormat">An optional preferred subtitle format to prioritize during parsing.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects extracted from the stream.</returns>
        public List<SubtitleItem> ParseStream(Stream stream, SubtitleParserOptions options, SubtitlesFormat subFormat = null)
        {
            Dictionary<SubtitlesFormat, ISubtitleParser> dictionary = subFormat != null ?
                _subFormatToParser
                .OrderBy(dic => Math.Abs(string.Compare(dic.Key.Name, subFormat.Name, StringComparison.Ordinal)))
                .ToDictionary(entry => entry.Key, entry => entry.Value)
                : _subFormatToParser;

            return ParseStream(stream, options, dictionary);
        }

        /// <summary>
        /// Iterates through available subtitle parsers to extract subtitle items from the stream.
        /// </summary>
        /// <param name="stream">The input stream containing subtitle data.</param>
        /// <param name="options">Parser options including encoding and timecode mode.</param>
        /// <param name="subFormatDictionary">A dictionary mapping subtitle formats to their respective parsers.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects parsed from the stream.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the stream is not readable or if no parser can successfully extract subtitle items.
        /// </exception>
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
                    // Continue with next parser if current fails.
                    continue;
                }
            }

            if (seekableStream.CanSeek)
                seekableStream.Position = 0;
            string firstCharsOfFile = LogFirstCharactersOfStream(seekableStream, 500, options.Encoding);
            string message = string.Format("All the subtitles parsers failed to parse the following stream:{0}", firstCharsOfFile);
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Logs the first few characters of the stream for diagnostic purposes.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="nbOfCharactersToPrint">The number of characters to log.</param>
        /// <param name="encoding">The encoding used to read the stream.</param>
        /// <returns>A string containing the initial part of the stream.</returns>
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