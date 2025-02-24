using System.Text;
using System.Text.RegularExpressions;
using SpongeEngine.SubtitleSharp.Utils;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Parser for SubStation Alpha (SSA) subtitle files.
    /// </summary>
    public class SsaParser : ISubtitleParser
    {
        // For backward compatibility:
        public List<SubtitleItem> ParseStream(Stream ssaStream, Encoding encoding)
        {
            return ParseStream(ssaStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        public List<SubtitleItem> ParseStream(Stream ssaStream, SubtitleParserOptions options)
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
                        List<SubtitleItem> items = new List<SubtitleItem>();
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
                                        SubtitleItem subtitleItem = new SubtitleItem()
                                        {
                                            StartTime = start,
                                            EndTime = end,
                                            Lines = lines,
                                            PlaintextLines = lines.Select(subtitleLine => Regex.Replace(subtitleLine, @"\{.*?\}", string.Empty)).ToList()
                                        };
                                        items.Add(subtitleItem);
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
                string message = string.Format("We reached line '{0}' with line number #{1} without finding the Event section ({2})", line, lineNumber, SsaFormatConstants.EVENT_LINE);
                throw new ArgumentException(message);
            }
        }

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
