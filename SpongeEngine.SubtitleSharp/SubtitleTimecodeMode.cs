namespace SpongeEngine.SubtitleSharp
{
	/// <summary>
	/// Specifies how subtitle timecodes should be handled during parsing.
	/// </summary>
	public enum SubtitleTimecodeMode
	{
		/// <summary>
		/// Timecodes must be present in the subtitle data.
		/// </summary>
		Required,
		/// <summary>
		/// Timecodes may be missing; dummy timecodes will be assigned if missing.
		/// </summary>
		Optional,
		/// <summary>
		/// No timecodes are expected; the parser will ignore timecode information and only parse the text.
		/// </summary>
		None
	}
}
