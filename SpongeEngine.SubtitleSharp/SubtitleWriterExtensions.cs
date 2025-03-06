using System.Collections.Generic;

namespace SpongeEngine.SubtitleSharp
{
	/// <summary>
	/// Extension methods for writing subtitle cues.
	/// </summary>
	public static class SubtitleWriterExtensions
	{
		/// <summary>
		/// Converts a collection of <see cref="SubtitleCue"/> objects to an SRT formatted string using the specified options.
		/// </summary>
		/// <param name="cues">The collection of subtitle cues.</param>
		/// <param name="options">Options controlling the output format.</param>
		/// <returns>An SRT formatted string.</returns>
		public static string ToSrtText(this IEnumerable<SubtitleCue> cues, SubtitleWriterOptions options)
		{
			var writer = new SubtitleWriter();
			return writer.WriteToText(cues, options);
		}
	}
}
