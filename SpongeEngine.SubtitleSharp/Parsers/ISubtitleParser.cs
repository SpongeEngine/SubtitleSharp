using System.Text;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Defines methods required for a subtitle parser.
    /// </summary>
    public interface ISubtitleParser
    {
        /// <summary>
        /// Parses a subtitle stream using the specified encoding.
        /// </summary>
        /// <param name="stream">The stream containing subtitle data.</param>
        /// <param name="encoding">The character encoding used to read the stream.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects parsed from the stream.</returns>
        List<SubtitleItem> ParseStream(Stream stream, Encoding encoding);

        /// <summary>
        /// Parses a subtitle stream using the provided parser options.
        /// </summary>
        /// <param name="stream">The stream containing subtitle data.</param>
        /// <param name="options">Options to control parsing (encoding and timecode mode).</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects parsed from the stream.</returns>
        List<SubtitleItem> ParseStream(Stream stream, SubtitleParserOptions options);
    }
}