namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Represents a subtitle file format, including its name and associated file extension.
    /// </summary>
    public class SubtitlesFormat
    {
        /// <summary>
        /// Gets or sets the name of the subtitle format.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern for the file extension of the subtitle format.
        /// </summary>
        public string Extension { get; set; }
        
        // Private constructor to enforce use of predefined formats.
        private SubtitlesFormat() {}

        /// <summary>
        /// The SubRip (SRT) subtitle format.
        /// </summary>
        public static SubtitlesFormat SubRipFormat = new SubtitlesFormat()
        {
            Name = "SubRip",
            Extension = @"\.srt"
        };

        /// <summary>
        /// The MicroDVD subtitle format.
        /// </summary>
        public static SubtitlesFormat MicroDvdFormat = new SubtitlesFormat()
        {
            Name = "MicroDvd",
            Extension = @"\.sub"
        };

        /// <summary>
        /// The SubViewer subtitle format.
        /// </summary>
        public static SubtitlesFormat SubViewerFormat = new SubtitlesFormat()
        {
            Name = "SubViewer",
            Extension = @"\.sub"
        };

        /// <summary>
        /// The SubStation Alpha (SSA) subtitle format.
        /// </summary>
        public static SubtitlesFormat SubStationAlphaFormat = new SubtitlesFormat()
        {
            Name = "SubStationAlpha",
            Extension = @"\.ssa"
        };

        /// <summary>
        /// The TTML subtitle format.
        /// </summary>
        public static SubtitlesFormat TtmlFormat = new SubtitlesFormat()
        {
            Name = "TTML",
            Extension = @"\.ttml"
        };

        /// <summary>
        /// The WebVTT subtitle format.
        /// </summary>
        public static SubtitlesFormat WebVttFormat = new SubtitlesFormat()
        {
            Name = "WebVTT",
            Extension = @"\.vtt"
        };

        /// <summary>
        /// The YouTube XML subtitle format.
        /// </summary>
        public static SubtitlesFormat YoutubeXmlFormat = new SubtitlesFormat()
        {
            Name = "YoutubeXml",
            //Extension can be defined if needed.
        };

        /// <summary>
        /// A list of all supported subtitle formats.
        /// </summary>
        public static List<SubtitlesFormat> SupportedSubtitlesFormats = new List<SubtitlesFormat>()
        {
            SubRipFormat,
            MicroDvdFormat,
            SubViewerFormat,
            SubStationAlphaFormat,
            TtmlFormat,
            WebVttFormat,
            YoutubeXmlFormat
        };
    }
}