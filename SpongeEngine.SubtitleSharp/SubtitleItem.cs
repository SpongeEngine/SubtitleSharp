namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Represents a single subtitle cue with its timing and text.
    /// </summary>
    public class SubtitleItem
    {
        /// <summary>
        /// Gets or sets the start time in milliseconds.
        /// </summary>
        public int StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time in milliseconds.
        /// </summary>
        public int EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the raw subtitle lines, which may include formatting.
        /// </summary>
        public List<string> Lines { get; set; }
        
        /// <summary>
        /// Gets or sets the plain-text subtitle lines without formatting.
        /// </summary>
        public List<string> PlaintextLines { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SubtitleItem"/> class.
        /// </summary>
        public SubtitleItem()
        {
            this.Lines = new List<string>();
            this.PlaintextLines = new List<string>();
        }
        
        /// <summary>
        /// Returns a string representation of the subtitle cue.
        /// </summary>
        /// <returns>A string showing the start and end times and the subtitle text.</returns>
        public override string ToString()
        {
            var startTs = new TimeSpan(0, 0, 0, 0, StartTime);
            var endTs = new TimeSpan(0, 0, 0, 0, EndTime);
            return string.Format("{0} --> {1}: {2}", startTs.ToString("G"), endTs.ToString("G"), string.Join(Environment.NewLine, Lines));
        }
    }
}