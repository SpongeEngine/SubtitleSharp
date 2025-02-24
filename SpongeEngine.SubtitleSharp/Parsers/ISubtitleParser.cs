using System.Text;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Interface specifying the required methods for a subtitle parser.
    /// </summary>
    public interface ISubtitleParser
    {
        /// <summary>
        /// Parses a subtitles file stream using the given encoding.
        /// </summary>
        List<SubtitleItem> ParseStream(Stream stream, Encoding encoding);

        /// <summary>
        /// Parses a subtitles file stream using the specified options.
        /// </summary>
        List<SubtitleItem> ParseStream(Stream stream, SubtitleParserOptions options);
    }
}