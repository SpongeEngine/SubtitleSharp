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
        /// Converts a <see cref="SubtitleItem"/> into the lines for an SRT subtitle entry.
        /// </summary>
        /// <param name="subtitleItem">The subtitle item to convert.</param>
        /// <param name="subtitleEntryNumber">The sequential subtitle number (starting at 1).</param>
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If <c>false</c> and <see cref="SubtitleItem.PlaintextLines"/> is set,
        /// those lines are used.
        /// </param>
        /// <returns>A list of strings representing the SRT subtitle entry.</returns>
        private IEnumerable<string> SubtitleItemToSubtitleEntry(SubtitleItem subtitleItem, int subtitleEntryNumber, bool includeFormatting)
        {
            // Format the timecode line.
            string formatTimecodeLine()
            {
                TimeSpan start = TimeSpan.FromMilliseconds(subtitleItem.StartTime);
                TimeSpan end = TimeSpan.FromMilliseconds(subtitleItem.EndTime);
                return $"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}";
            }

            List<string> lines = new List<string>();
            lines.Add(subtitleEntryNumber.ToString());
            lines.Add(formatTimecodeLine());
            // Use either formatted or plaintext lines based on the includeFormatting flag.
            if (includeFormatting == false && subtitleItem.PlaintextLines != null)
                lines.AddRange(subtitleItem.PlaintextLines);
            else
                lines.AddRange(subtitleItem.Lines);

            return lines;
        }

        /// <summary>
        /// Writes a collection of subtitle items to a stream in SubRip (SRT) format synchronously.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="includeFormatting">
        /// Indicates whether to include formatting codes. If <c>false</c>, <see cref="SubtitleItem.PlaintextLines"/> should be populated.
        /// </param>
        public void WriteStream(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true)
        {
            using TextWriter writer = new StreamWriter(stream);
            List<SubtitleItem> items = subtitleItems.ToList(); // Prevent multiple enumeration.
            for (int i = 0; i < items.Count; i++)
            {
                SubtitleItem subtitleItem = items[i];
                IEnumerable<string> lines = SubtitleItemToSubtitleEntry(subtitleItem, i + 1, includeFormatting);
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
        /// Indicates whether to include formatting codes. If <c>false</c>, <see cref="SubtitleItem.PlaintextLines"/> should be populated.
        /// </param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        public async Task WriteStreamAsync(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true)
        {
            await using TextWriter writer = new StreamWriter(stream);
            List<SubtitleItem> items = subtitleItems.ToList(); // Prevent multiple enumeration.
            for (int i = 0; i < items.Count; i++)
            {
                SubtitleItem subtitleItem = items[i];
                IEnumerable<string> lines = SubtitleItemToSubtitleEntry(subtitleItem, i + 1, includeFormatting);
                foreach (string line in lines)
                    await writer.WriteLineAsync(line);

                await writer.WriteLineAsync(); // Blank line between entries.
            }
        }
    }
}