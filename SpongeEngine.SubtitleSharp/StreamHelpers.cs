namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Provides helper methods for working with streams.
    /// </summary>
    static class StreamHelpers
    {
        /// <summary>
        /// Copies the contents of the input stream into a new seekable <see cref="MemoryStream"/>.
        /// This is particularly useful when the input stream is not seekable.
        /// </summary>
        /// <param name="inputStream">The input stream to copy.</param>
        /// <returns>A <see cref="MemoryStream"/> containing the copied data.</returns>
        public static Stream CopyStream(Stream inputStream)
        {
            MemoryStream outputStream = new MemoryStream();
            int count;
            do
            {
                byte[] buf = new byte[1024];
                count = inputStream.Read(buf, 0, 1024);
                outputStream.Write(buf, 0, count);
            } while (inputStream.CanRead && count > 0);
            outputStream.ToArray();

            return outputStream;
        }
    }
}