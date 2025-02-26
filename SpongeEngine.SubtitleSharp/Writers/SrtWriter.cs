using System.Text;

namespace SpongeEngine.SubtitleSharp.Writers
{
    /// <summary>
    /// Implements writing of subtitle items to a SubRip (SRT) formatted file.
    /// 
    /// An SRT file consists of sequentially numbered subtitle blocks, each with a timecode line followed by text lines.
    /// </summary>
    public class SrtWriter : ISubtitleWriter
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
        /// <returns>A list of strings representing the SRT subtitle entry.</returns>
        private IEnumerable<string> SubtitleItemToSubtitleEntry(SubtitleCue subtitleCue, int subtitleEntryNumber, bool includeFormatting)
        {
            // Format the timecode line.
            string formatTimecodeLine()
            {
                TimeSpan start = TimeSpan.FromMilliseconds(subtitleCue.StartTime);
                TimeSpan end = TimeSpan.FromMilliseconds(subtitleCue.EndTime);
                return $"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}";
            }

            List<string> lines = new List<string>();
            lines.Add(subtitleEntryNumber.ToString());
            lines.Add(formatTimecodeLine());
            // Use either formatted or plaintext lines based on the includeFormatting flag.
            if (includeFormatting == false && subtitleCue.PlaintextLines != null)
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
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If <c>false</c>, <see cref="SubtitleCue.PlaintextLines"/> should be populated.
        /// </param>
        public void WriteStream(Stream stream, IEnumerable<SubtitleCue> subtitleItems, bool includeFormatting = true)
        {
            using TextWriter writer = new StreamWriter(stream);
            List<SubtitleCue> items = subtitleItems.ToList(); // Prevent multiple enumeration.
            for (int i = 0; i < items.Count; i++)
            {
                SubtitleCue subtitleCue = items[i];
                IEnumerable<string> lines = SubtitleItemToSubtitleEntry(subtitleCue, i + 1, includeFormatting);
                foreach (string line in lines)
                    writer.WriteLine(line);

                writer.WriteLine(); // Blank line between entries.
            }
        }

        /// <summary>
        /// Writes a collection of subtitle items to a stream in SubRip (SRT) format asynchronously.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If <c>false</c>, <see cref="SubtitleCue.PlaintextLines"/> should be populated.
        /// </param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        public async Task WriteStreamAsync(Stream stream, IEnumerable<SubtitleCue> subtitleItems, bool includeFormatting = true)
        {
            await using TextWriter writer = new StreamWriter(stream);
            List<SubtitleCue> items = subtitleItems.ToList(); // Prevent multiple enumeration.
            for (int i = 0; i < items.Count; i++)
            {
                SubtitleCue subtitleCue = items[i];
                IEnumerable<string> lines = SubtitleItemToSubtitleEntry(subtitleCue, i + 1, includeFormatting);
                foreach (string line in lines)
                    await writer.WriteLineAsync(line);

                await writer.WriteLineAsync(); // Blank line between entries.
            }
        }
        
        /// <summary>
        /// Converts a collection of subtitle items to a string in SubRip (SRT) format.
        /// </summary>
        /// <param name="subtitleItems">The subtitle items to convert.</param>
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If set to <c>false</c> and <see cref="SubtitleCue.PlaintextLines"/> is populated,
        /// those lines will be used instead.
        /// </param>
        /// <returns>A string containing the SRT formatted subtitles.</returns>
        public string WriteToText(IEnumerable<SubtitleCue> subtitleItems, bool includeFormatting = true)
        {
            StringBuilder sb = new StringBuilder();
            List<SubtitleCue> items = subtitleItems.ToList(); // Prevent multiple enumeration.
            for (int i = 0; i < items.Count; i++)
            {
                SubtitleCue subtitleCue = items[i];
                IEnumerable<string> lines = SubtitleItemToSubtitleEntry(subtitleCue, i + 1, includeFormatting);
                foreach (string line in lines)
                {
                    sb.AppendLine(line);
                }
                sb.AppendLine(); // Blank line between entries.
            }
            return sb.ToString();
        }

    }
}