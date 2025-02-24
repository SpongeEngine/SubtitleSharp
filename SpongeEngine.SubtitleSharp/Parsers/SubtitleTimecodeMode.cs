namespace SpongeEngine.SubtitleSharp.Parsers
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
        /// Timecodes may be missing; in such cases, dummy timecodes will be assigned.
        /// </summary>
        Optional
    }
}