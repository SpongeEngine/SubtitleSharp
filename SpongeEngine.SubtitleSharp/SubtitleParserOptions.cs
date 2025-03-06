using System.Text;

namespace SpongeEngine.SubtitleSharp
{
	/// <summary>
	/// Specifies options for parsing subtitle streams.
	/// </summary>
	public class SubtitleParserOptions
	{
		/// <summary>
		/// Gets or sets the text encoding used to read subtitle streams.
		/// Defaults to UTF-8.
		/// </summary>
		public Encoding Encoding { get; set; } = Encoding.UTF8;

		/// <summary>
		/// Gets or sets the mode for handling subtitle timecodes.
		/// When set to <see cref="SubtitleTimecodeMode.Required"/>, timecodes must be present;
		/// when set to <see cref="SubtitleTimecodeMode.Optional"/>, dummy timecodes will be assigned if missing;
		/// when set to <see cref="SubtitleTimecodeMode.None"/>, no timecodes are expected and all lines are treated as text.
		/// </summary>
		public SubtitleTimecodeMode TimecodeMode { get; set; } = SubtitleTimecodeMode.Required;
	}
}
