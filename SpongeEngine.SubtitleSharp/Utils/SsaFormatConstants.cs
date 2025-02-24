namespace SpongeEngine.SubtitleSharp.Utils
{
    /// <summary>
    /// Defines constant values used in parsing and writing SSA subtitle files.
    /// </summary>
    public static class SsaFormatConstants
    {
        /// <summary>
        /// Represents the [Script Info] section header.
        /// </summary>
        public const string SCRIPT_INFO_LINE = "[Script Info]";

        /// <summary>
        /// Represents the [Events] section header.
        /// </summary>
        public const string EVENT_LINE = "[Events]";

        /// <summary>
        /// The separator character used in SSA files.
        /// </summary>
        public const char SEPARATOR = ',';

        /// <summary>
        /// The character used to denote comments.
        /// </summary>
        public const char COMMENT = ';';

        /// <summary>
        /// The prefix used for specifying wrap style.
        /// </summary>
        public const string WRAP_STYLE_PREFIX = "WrapStyle: ";

        /// <summary>
        /// The prefix used to denote dialogue lines.
        /// </summary>
        public const string DIALOGUE_PREFIX = "Dialogue: ";

        /// <summary>
        /// The header name for the start time column.
        /// </summary>
        public const string START_COLUMN = "Start";

        /// <summary>
        /// The header name for the end time column.
        /// </summary>
        public const string END_COLUMN = "End";

        /// <summary>
        /// The header name for the text column.
        /// </summary>
        public const string TEXT_COLUMN = "Text";
    }
}