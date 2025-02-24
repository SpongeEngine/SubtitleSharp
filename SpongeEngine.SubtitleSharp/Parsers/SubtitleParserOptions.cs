using System.Text;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    public class SubtitleParserOptions
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public SubtitleTimecodeMode TimecodeMode { get; set; } = SubtitleTimecodeMode.Required;
    }
}