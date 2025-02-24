namespace SpongeEngine.SubtitleSharp.Writers
{
    /// <summary>
    /// Defines methods required for writing subtitle items to a stream.
    /// </summary>
    public interface ISubtitleWriter
    {
        /// <summary>
        /// Writes a collection of subtitle items to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="includeFormatting">
        /// Indicates whether formatting codes should be included. If set to <c>false</c>, it is expected that
        /// <see cref="SubtitleItem.PlaintextLines"/> is populated.
        /// </param>
        void WriteStream(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true);

        /// <summary>
        /// Asynchronously writes a collection of subtitle items to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="subtitleItems">The subtitle items to write.</param>
        /// <param name="includeFormatting">
        /// Indicates whether formatting codes should be included. If set to <c>false</c>, it is expected that
        /// <see cref="SubtitleItem.PlaintextLines"/> is populated.
        /// </param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        Task WriteStreamAsync(Stream stream, IEnumerable<SubtitleItem> subtitleItems, bool includeFormatting = true);
    }
}