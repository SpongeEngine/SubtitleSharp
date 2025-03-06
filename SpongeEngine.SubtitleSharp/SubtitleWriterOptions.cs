namespace SpongeEngine.SubtitleSharp
{
	/// <summary>
	/// Options for configuring the behavior of the subtitle writer.
	/// </summary>
	public class SubtitleWriterOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether formatting should be included.
		/// Defaults to true.
		/// </summary>
		public bool IncludeFormatting { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether timecode lines should be included.
		/// Defaults to true.
		/// </summary>
		public bool IncludeTimecode { get; set; } = true;

		/// <summary>
		/// Gets or sets the newline sequence used when writing subtitles.
		/// Defaults to "\n" (Linux-style).
		/// </summary>
		public string NewLine { get; set; } = "\n";
	}
}
