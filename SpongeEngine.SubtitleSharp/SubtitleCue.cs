using System.Collections.Generic;

namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Represents a single subtitle cue with its timing and text.
    /// 
    /// A subtitle cue contains the start and end times (in milliseconds) for the subtitle,
    /// along with the lines of text that should be displayed during that time period.
    /// The text is stored in two different formats:
    /// 
    /// - <see cref="Lines"/>: Raw subtitle lines, which may include formatting tags (e.g., for italics, colors).
    /// - <see cref="PlaintextLines"/>: Plain-text subtitle lines with all formatting tags removed.
    /// 
    /// Both properties store a collection of strings, allowing for multiple lines of text to be handled
    /// per subtitle cue. This is important because subtitles can span multiple lines, and the lines may 
    /// have different formatting or be split due to formatting rules in the subtitle file.
    /// 
    /// The class encapsulates a single subtitle unit, often referred to as a "cue," which includes both
    /// the time period during which the subtitle is displayed and the text associated with that period.
    /// </summary>
    public class SubtitleCue
    {
        /// <summary>
        /// Gets or sets the start time in milliseconds.
        /// This indicates when the subtitle should begin displaying.
        /// </summary>
        public int StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time in milliseconds.
        /// This indicates when the subtitle should stop displaying.
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// Gets or sets the raw subtitle lines, which may include formatting.
        /// 
        /// Each element in this list represents a line of text that may contain formatting codes
        /// (such as {\\i1} for italics). The raw lines are kept to preserve the original format
        /// of the subtitle as it appears in the subtitle file.
        /// </summary>
        public List<string> Lines { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the plain-text subtitle lines without formatting.
        /// 
        /// Each element in this list represents a line of text with all formatting tags removed.
        /// This version is useful for displaying the text without any special formatting or for
        /// processing the subtitle content without considering its visual appearance.
        /// </summary>
        public List<string> PlaintextLines { get; set; } = new ();
        
        /// <summary>
        /// Returns a string representation of the subtitle cue.
        /// 
        /// The string representation includes the start and end times, as well as the text lines.
        /// This is helpful for debugging or displaying a textual version of the subtitle cue.
        /// </summary>
        /// <returns>A string showing the start and end times and the subtitle text, with each line separated by a new line.</returns>
        public override string ToString()
        {
            return $"StartTime={StartTime}, EndTime={EndTime}, Lines={string.Join("\n", Lines)}, PlaintextLines={string.Join("\n", PlaintextLines)}";
        }
    }
}
