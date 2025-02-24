namespace SpongeEngine.SubtitleSharp.Parsers
{
    public enum SubtitleTimecodeMode
    {
        Required,   // Timecodes must be present.
        Optional    // Timecodes may be missing; dummy timecodes will be assigned.
    }
}