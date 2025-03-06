using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Implements writing of subtitle items to a SubRip (SRT) formatted file.
    /// </summary>
    public class SubtitleWriter
    {
        /// <summary>
        /// Converts a <see cref="SubtitleCue"/> into the lines for an SRT subtitle entry.
        /// </summary>
        /// <param name="subtitleCue">The subtitle item to convert.</param>
        /// <param name="subtitleEntryNumber">The sequential subtitle number (starting at 1).</param>
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If <c>false</c> and <see cref="SubtitleCue.PlaintextLines"/> is set,
        /// those lines are used.
        /// </param>
        /// <param name="includeTimecode">If <c>true</c>, the timecode line is included; otherwise, it is omitted.</param>
        /// <returns>A list of strings representing the SRT subtitle entry.</returns>
        private IEnumerable<string> SubtitleItemToSubtitleEntry(SubtitleCue subtitleCue, int subtitleEntryNumber, bool includeFormatting, bool includeTimecode)
        {
            // Always start with the sequence number.
            List<string> lines = new() { subtitleEntryNumber.ToString() };

            // Add timecode line only if required.
            if (includeTimecode)
            {
                string FormatTimecodeLine()
                {
                    TimeSpan start = TimeSpan.FromMilliseconds(subtitleCue.StartTime);
                    TimeSpan end = TimeSpan.FromMilliseconds(subtitleCue.EndTime);
                    return $"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}";
                }
                lines.Add(FormatTimecodeLine());
            }

            // Append text lines.
            if (!includeFormatting && subtitleCue.PlaintextLines is not null)
                lines.AddRange(subtitleCue.PlaintextLines);
            else
                lines.AddRange(subtitleCue.Lines);

            return lines;
        }

        /// <summary>
        /// Writes a collection of subtitle items to a stream in SubRip (SRT) format synchronously.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="options">Options controlling the output format.</param>
        public void WriteStream(Stream stream, IEnumerable<SubtitleCue> subtitleItems, SubtitleWriterOptions options)
        {
            // Use leaveOpen: true so the stream remains open after disposing the writer.
            using TextWriter writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
            // Set the newline sequence from the options.
            writer.NewLine = options.NewLine;
            WriteEntriesAsync(writer, subtitleItems, options.IncludeFormatting, options.IncludeTimecode).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Writes a collection of subtitle items to a stream in SubRip (SRT) format asynchronously.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="options">Options controlling the output format.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        public async Task WriteStreamAsync(Stream stream, IEnumerable<SubtitleCue> subtitleItems, SubtitleWriterOptions options)
        {
            // Use leaveOpen: true so the stream remains open after disposing the writer.
            await using TextWriter writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
            // Set the newline sequence from the options.
            writer.NewLine = options.NewLine;
            await WriteEntriesAsync(writer, subtitleItems, options.IncludeFormatting, options.IncludeTimecode).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a collection of subtitle items to a string in SubRip (SRT) format.
        /// </summary>
        /// <param name="subtitleItems">The subtitle items to convert.</param>
        /// <param name="options">Options controlling the output format.</param>
        /// <returns>A string containing the SRT formatted subtitles.</returns>
        public string WriteToText(IEnumerable<SubtitleCue> subtitleItems, SubtitleWriterOptions options)
        {
            StringBuilder sb = new();
            using TextWriter writer = new StringWriter(sb);
            // Set the newline sequence from the options.
            writer.NewLine = options.NewLine;
            WriteEntriesAsync(writer, subtitleItems, options.IncludeFormatting, options.IncludeTimecode).GetAwaiter().GetResult();
            return sb.ToString();
        }

        /// <summary>
        /// Writes subtitle entries using the provided TextWriter.
        /// </summary>
        /// <param name="writer">The text writer to write to.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="includeFormatting">Whether to include formatting codes.</param>
        /// <param name="includeTimecode">Whether to include timecode lines.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        private async Task WriteEntriesAsync(TextWriter writer, IEnumerable<SubtitleCue> subtitleItems, bool includeFormatting, bool includeTimecode, CancellationToken cancellationToken = default)
        {
	        List<SubtitleCue> items = subtitleItems.ToList();
	        for (int i = 0; i < items.Count; i++)
	        {
		        IEnumerable<string> lines = SubtitleItemToSubtitleEntry(items[i], i + 1, includeFormatting, includeTimecode);
		        foreach (var line in lines)
		        {
			        await writer.WriteLineAsync(line).ConfigureAwait(false);
		        }
	        }
        }
    }
}
