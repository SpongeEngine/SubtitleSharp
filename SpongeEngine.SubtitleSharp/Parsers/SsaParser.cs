using System.Text;
using System.Text.RegularExpressions;
using SpongeEngine.SubtitleSharp.Utils;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Implements parsing for the SubStation Alpha (SSA) subtitles format.
    /// 
    /// The SSA format is a structured subtitle format that includes sections such as [Script Info] and [Events].
    /// This parser focuses on extracting dialogue entries from the [Events] section.
    /// </summary>
    public class SsaParser : ISubtitleParser
    {
        // For backward compatibility:
        /// <summary>
        /// Parses an SSA stream using the specified encoding.
        /// </summary>
        /// <param name="ssaStream">A seekable and readable stream containing SSA subtitle data.</param>
        /// <param name="encoding">The character encoding used to read the stream.</param>
        /// <returns>A list of <see cref="SubtitleCue"/> objects parsed from the stream.</returns>
        public List<SubtitleCue> ParseStream(Stream ssaStream, Encoding encoding)
        {
            return ParseStream(ssaStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        /// <summary>
        /// Parses an SSA stream using the provided parser options.
        /// </summary>
        /// <param name="ssaStream">A seekable and readable stream containing SSA subtitle data.</param>
        /// <param name="options">Parser options including encoding and timecode mode.</param>
        /// <returns>A list of <see cref="SubtitleCue"/> objects extracted from the stream.</returns>
        /// <exception cref="ArgumentException">Thrown if the stream is not readable/seekable or if the format is invalid.</exception>
        public List<SubtitleCue> ParseStream(Stream ssaStream, SubtitleParserOptions options)
        {
            if (!ssaStream.CanRead || !ssaStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                            ssaStream.CanSeek, ssaStream.CanRead);
                throw new ArgumentException(message);
            }

            ssaStream.Position = 0;
            StreamReader reader = new StreamReader(ssaStream, options.Encoding, true);
            SsaWrapStyle wrapStyle = SsaWrapStyle.None;
            string? line = reader.ReadLine();
            int lineNumber = 1;
            while (line != null && line != SsaFormatConstants.EVENT_LINE)
            {
                if (line.StartsWith(SsaFormatConstants.WRAP_STYLE_PREFIX))
                {
                    wrapStyle = line.Split(':')[1].TrimStart().FromString();
                }
                line = reader.ReadLine();
                lineNumber++;
            }

            if (line != null)
            {
                string? headerLine = reader.ReadLine();
                if (!string.IsNullOrEmpty(headerLine))
                {
                    List<string> columnHeaders = headerLine.Split(SsaFormatConstants.SEPARATOR).Select(head => head.Trim()).ToList();
                    int startIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.START_COLUMN);
                    int endIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.END_COLUMN);
                    int textIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.TEXT_COLUMN);
                    if (startIndexColumn > 0 && endIndexColumn > 0 && textIndexColumn > 0)
                    {
                        List<SubtitleCue> items = new List<SubtitleCue>();
                        int dummyTime = 0, defaultDuration = 1000;
                        line = reader.ReadLine();
                        while (line != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                string[] columns = line.Split(SsaFormatConstants.SEPARATOR);
                                string startText = columns[startIndexColumn];
                                string endText = columns[endIndexColumn];
                                string textLine = string.Join(",", columns.Skip(textIndexColumn));
                                int start = ParseSsaTimecode(startText);
                                int end = ParseSsaTimecode(endText);
                                if ((start > 0 && end > 0) || options.TimecodeMode == SubtitleTimecodeMode.Optional)
                                {
                                    if (start <= 0 || end <= 0)
                                    {
                                        start = dummyTime;
                                        end = dummyTime + defaultDuration;
                                        dummyTime += defaultDuration;
                                    }
                                    if (!string.IsNullOrEmpty(textLine))
                                    {
                                        List<string> lines;
                                        // Choose splitting strategy based on wrap style.
                                        switch (wrapStyle)
                                        {
                                            case SsaWrapStyle.Smart:
                                            case SsaWrapStyle.SmartWideLowerLine:
                                            case SsaWrapStyle.EndOfLine:
                                                lines = textLine.Split(@"\N").ToList();
                                                break;
                                            case SsaWrapStyle.None:
                                                lines = Regex.Split(textLine, @"(?:\\n)|(?:\\N)").ToList();
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }
                                        lines = lines.Select(l => l.TrimStart()).ToList();
                                        SubtitleCue subtitleCue = new SubtitleCue()
                                        {
                                            StartTime = start,
                                            EndTime = end,
                                            Lines = lines,
                                            PlaintextLines = lines.Select(subtitleLine => Regex.Replace(subtitleLine, @"\{.*?\}", string.Empty)).ToList()
                                        };
                                        items.Add(subtitleCue);
                                    }
                                }
                            }
                            line = reader.ReadLine();
                        }
                        if (items.Any())
                            return items;
                        else
                            throw new ArgumentException("Stream is not in a valid Ssa format");
                    }
                    else
                    {
                        string message = string.Format("Couldn't find all the necessary columns headers ({0}, {1}, {2}) in header line {3}",
                                                        SsaFormatConstants.START_COLUMN, SsaFormatConstants.END_COLUMN,
                                                        SsaFormatConstants.TEXT_COLUMN, headerLine);
                        throw new ArgumentException(message);
                    }
                }
                else
                {
                    string message = string.Format("The header line after the line '{0}' was null -> no need to continue parsing", line);
                    throw new ArgumentException(message);
                }
            }
            else
            {
                string message = string.Format("Reached end of header at line '{0}' (line #{1}) without finding the Event section ({2})", line, lineNumber, SsaFormatConstants.EVENT_LINE);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Parses an SSA timecode string into its equivalent value in milliseconds.
        /// </summary>
        /// <param name="s">The SSA timecode string to parse.</param>
        /// <returns>The timecode in milliseconds, or -1 if parsing fails.</returns>
        private int ParseSsaTimecode(string s)
        {
            if (TimeSpan.TryParse(s, out TimeSpan result))
            {
                return (int)result.TotalMilliseconds;
            }
            else
            {
                return -1;
            }
        }
    }
}