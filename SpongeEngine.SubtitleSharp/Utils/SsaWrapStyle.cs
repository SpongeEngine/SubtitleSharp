namespace SpongeEngine.SubtitleSharp.Utils
{
    /// <summary>
    /// Represents the wrap style used by Advanced SSA subtitles.
    /// </summary>
    public enum SsaWrapStyle
    {
        /// <summary>
        /// Smart wrapping, where lines are evenly broken.
        /// </summary>
        Smart = 0,
        /// <summary>
        /// End-of-line word wrapping; only "\N" breaks are recognized.
        /// </summary>
        EndOfLine = 1,
        /// <summary>
        /// No word wrapping; both "\n" and "\N" are treated as line breaks.
        /// </summary>
        None = 2,
        /// <summary>
        /// Similar to Smart, but the lower line is given more width.
        /// </summary>
        SmartWideLowerLine = 3
    }

    /// <summary>
    /// Provides extension methods for converting strings or integers to <see cref="SsaWrapStyle"/>.
    /// </summary>
    public static class SsaWrapStyleExtensions
    {
        /// <summary>
        /// Parses a string into a corresponding <see cref="SsaWrapStyle"/> value.
        /// 
        /// Invalid inputs default to <see cref="SsaWrapStyle.None"/>.
        /// </summary>
        /// <param name="rawString">A string representation of a wrap style value.</param>
        /// <returns>The corresponding <see cref="SsaWrapStyle"/> value.</returns>
        public static SsaWrapStyle FromString(this string rawString) =>
            int.TryParse(rawString, out int rawInt) == false ?
                SsaWrapStyle.None :
                FromInt(rawInt);

        /// <summary>
        /// Converts an integer into the corresponding <see cref="SsaWrapStyle"/> value.
        /// 
        /// Integers outside the valid range default to <see cref="SsaWrapStyle.None"/>.
        /// </summary>
        /// <param name="rawInt">An integer representing a wrap style.</param>
        /// <returns>The corresponding <see cref="SsaWrapStyle"/> value.</returns>
        public static SsaWrapStyle FromInt(this int rawInt) =>
            rawInt switch
            {
                0 => SsaWrapStyle.Smart,
                1 => SsaWrapStyle.EndOfLine,
                2 => SsaWrapStyle.None,
                3 => SsaWrapStyle.SmartWideLowerLine,
                _ => SsaWrapStyle.None
            };
    }
}