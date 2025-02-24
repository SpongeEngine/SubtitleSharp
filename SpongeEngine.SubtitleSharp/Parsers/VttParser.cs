#nullable enable
using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Implements parsing for WebVTT subtitle files.
    /// 
    /// This parser extracts cues and timecodes from a VTT file. Note that it does not process formatting tags;
    /// if needed, formatting must be handled separately.
    /// 
    /// A typical WebVTT file has the following structure:
    /// 
    ///     WEBVTT
    ///     
    ///     CUE - 1
    ///     00:00:10.500 --> 00:00:13.000
    ///     Elephant's Dream
    ///     
    ///     CUE - 2
    ///     00:00:15.000 --> 00:00:18.000
    ///     At the left we can see...
    /// </summary>
    public class VttParser : ISubtitleParser
    {
        private static readonly Regex _longTimestampRegex = new Regex("(?<H>[0-9]+):(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex _shortTimestampRegex = new Regex("(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);
        private readonly string[] _timecodeDelimiters = new string[] { "-->", "- >", "->" };

        /// <summary>
        /// Initializes a new instance of the <see cref="VttParser"/> class.
        /// </summary>
        public VttParser() { }

        // For backward compatibility:
        /// <summary>
        /// Parses a WebVTT stream using the specified encoding.
        /// </summary>
        /// <param name="vttStream">A seekable and readable stream containing WebVTT content.</param>
        /// <param name="encoding">The character encoding used to read the stream.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects parsed from the stream.</returns>
        public List<SubtitleItem> ParseStream(Stream vttStream, Encoding encoding)
        {
            return ParseStream(vttStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        /// <summary>
        /// Parses a WebVTT stream using the provided parser options.
        /// </summary>
        /// <param name="vttStream">A seekable and readable stream containing WebVTT content.</param>
        /// <param name="options">Parser options including encoding and timecode handling.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects extracted from the stream.</returns>
        /// <exception cref="ArgumentException">Thrown if the stream is not readable or seekable.</exception>
        /// <exception cref="FormatException">Thrown if no valid subtitle cues are found.</exception>
        public List<SubtitleItem> ParseStream(Stream vttStream, SubtitleParserOptions options)
        {
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanRead);
                throw new ArgumentException(message);
            }

            // Reset stream position and create a reader.
            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> items = new List<SubtitleItem>();
            IEnumerator<string> vttSubParts = GetVttSubTitleParts(reader).GetEnumerator();
            int dummyTime = 0, defaultDuration = 1000;

            if (!vttSubParts.MoveNext())
            {
                throw new FormatException("Parsing as VTT returned no VTT parts.");
            }

            do
            {
                // Split the current subtitle block into individual non-empty, trimmed lines.
                IEnumerable<string> lines = vttSubParts.Current
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(l => !string.IsNullOrEmpty(l));

                SubtitleItem subtitleItem = new SubtitleItem();
                bool timecodeFound = false;
                foreach (string line in lines)
                {
                    if (!timecodeFound)
                    {
                        // Attempt to parse a timecode line.
                        bool success = TryParseTimecodeLine(line, out int startTc, out int endTc);
                        if (success)
                        {
                            subtitleItem.StartTime = startTc;
                            subtitleItem.EndTime = endTc;
                            timecodeFound = true;
                        }
                        else if (options.TimecodeMode == SubtitleTimecodeMode.Optional)
                        {
                            // When timecodes are optional, assign dummy timecodes.
                            subtitleItem.StartTime = dummyTime;
                            subtitleItem.EndTime = dummyTime + defaultDuration;
                            dummyTime += defaultDuration;
                            timecodeFound = true;
                        }
                    }
                    else
                    {
                        // Subsequent lines are considered subtitle text.
                        subtitleItem.Lines.Add(line);
                    }
                }

                // Add the cue if valid timecodes and at least one text line are present.
                if ((subtitleItem.StartTime != 0 || subtitleItem.EndTime != 0) && subtitleItem.Lines.Any())
                {
                    items.Add(subtitleItem);
                }
            }
            while (vttSubParts.MoveNext());

            if (!items.Any())
            {
                throw new FormatException("Parsing as VTT returned no valid cues.");
            }

            return items;
        }

        /// <summary>
        /// Asynchronously parses a WebVTT stream using the specified encoding.
        /// </summary>
        /// <param name="vttStream">A seekable and readable stream containing WebVTT content.</param>
        /// <param name="encoding">The character encoding used to read the stream.</param>
        /// <returns>
        /// A task representing the asynchronous operation, with a list of <see cref="SubtitleItem"/> objects as its result.
        /// </returns>
        public async Task<List<SubtitleItem>> ParseStreamAsync(Stream vttStream, Encoding encoding)
        {
            return await ParseStreamAsync(vttStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        /// <summary>
        /// Asynchronously parses a WebVTT stream using the provided parser options.
        /// </summary>
        /// <param name="vttStream">A seekable and readable stream containing WebVTT content.</param>
        /// <param name="options">Parser options including encoding and timecode handling.</param>
        /// <returns>
        /// A task representing the asynchronous operation, with a list of <see cref="SubtitleItem"/> objects extracted from the stream.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the stream is not readable or seekable.</exception>
        /// <exception cref="FormatException">Thrown if no valid subtitle cues are found.</exception>
        public async Task<List<SubtitleItem>> ParseStreamAsync(Stream vttStream, SubtitleParserOptions options)
        {
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanRead);
                throw new ArgumentException(message);
            }

            // Reset stream position and create a reader.
            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> items = new List<SubtitleItem>();
            IAsyncEnumerator<string> vttBlockEnumerator = GetVttSubTitlePartsAsync(reader).GetAsyncEnumerator();
            int dummyTime = 0, defaultDuration = 1000;

            if (await vttBlockEnumerator.MoveNextAsync() == false)
            {
                throw new FormatException("Parsing as VTT returned no VTT parts.");
            }

            do
            {
                IEnumerable<string> lines = vttBlockEnumerator.Current
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(l => !string.IsNullOrEmpty(l));

                SubtitleItem subtitleItem = new SubtitleItem();
                bool timecodeFound = false;
                foreach (string line in lines)
                {
                    if (!timecodeFound)
                    {
                        bool success = TryParseTimecodeLine(line, out int startTc, out int endTc);
                        if (success)
                        {
                            subtitleItem.StartTime = startTc;
                            subtitleItem.EndTime = endTc;
                            timecodeFound = true;
                        }
                        else if (options.TimecodeMode == SubtitleTimecodeMode.Optional)
                        {
                            subtitleItem.StartTime = dummyTime;
                            subtitleItem.EndTime = dummyTime + defaultDuration;
                            dummyTime += defaultDuration;
                            timecodeFound = true;
                        }
                    }
                    else
                    {
                        subtitleItem.Lines.Add(line);
                    }
                }

                if ((subtitleItem.StartTime != 0 || subtitleItem.EndTime != 0) && subtitleItem.Lines.Any())
                {
                    items.Add(subtitleItem);
                }
            }
            while (await vttBlockEnumerator.MoveNextAsync());

            if (!items.Any())
            {
                throw new FormatException("Parsing as VTT returned no valid cues.");
            }

            return items;
        }

        /// <summary>
        /// Splits the WebVTT file into individual subtitle blocks based on blank lines.
        /// 
        /// Each block may contain an optional cue identifier, a timecode line, and one or more text lines.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> associated with the VTT file.</param>
        /// <returns>An enumerable sequence of subtitle block strings.</returns>
        private IEnumerable<string> GetVttSubTitleParts(TextReader reader)
        {
            string line;
            StringBuilder stringBuilder = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    string res = stringBuilder.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                        yield return res;
                    stringBuilder = new StringBuilder();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }
            if (stringBuilder.Length > 0)
                yield return stringBuilder.ToString();
        }

        /// <summary>
        /// Asynchronously splits the WebVTT file into subtitle blocks using blank lines as delimiters.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> for the VTT file.</param>
        /// <returns>An asynchronous enumerable sequence of subtitle block strings.</returns>
        private async IAsyncEnumerable<string> GetVttSubTitlePartsAsync(TextReader reader)
        {
            string line;
            StringBuilder sb = new StringBuilder();
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    string res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                        yield return res;
                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        /// <summary>
        /// Attempts to extract start and end timecodes from a timecode line.
        /// </summary>
        /// <param name="line">A line expected to contain two timecodes separated by a delimiter.</param>
        /// <param name="startTc">Output start timecode (in milliseconds) if parsing succeeds; otherwise -1.</param>
        /// <param name="endTc">Output end timecode (in milliseconds) if parsing succeeds; otherwise -1.</param>
        /// <returns><c>true</c> if both timecodes were successfully parsed; otherwise, <c>false</c>.</returns>
        private bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            string[] parts = line.Split(_timecodeDelimiters, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                startTc = -1;
                endTc = -1;
                return false;
            }
            else
            {
                startTc = ParseVttTimecode(parts[0]);
                endTc = ParseVttTimecode(parts[1]);
                return startTc != -1 && endTc != -1;
            }
        }

        /// <summary>
        /// Parses a WebVTT timecode string into its equivalent value in milliseconds.
        /// 
        /// Acceptable formats include either <c>hh:mm:ss.mmm</c> or <c>mm:ss.mmm</c>.
        /// </summary>
        /// <param name="s">The timecode string to parse.</param>
        /// <returns>The timecode in milliseconds, or -1 if parsing fails.</returns>
        private int ParseVttTimecode(string s)
        {
            int hours = 0, minutes = 0, seconds = 0, milliseconds = -1;
            Match match = _longTimestampRegex.Match(s);
            if (match.Success)
            {
                hours = int.Parse(match.Groups["H"].Value);
                minutes = int.Parse(match.Groups["M"].Value);
                seconds = int.Parse(match.Groups["S"].Value);
                milliseconds = int.Parse(match.Groups["m"].Value);
            }
            else
            {
                match = _shortTimestampRegex.Match(s);
                if (match.Success)
                {
                    minutes = int.Parse(match.Groups["M"].Value);
                    seconds = int.Parse(match.Groups["S"].Value);
                    milliseconds = int.Parse(match.Groups["m"].Value);
                }
            }

            if (milliseconds >= 0)
            {
                TimeSpan result = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                return (int)result.TotalMilliseconds;
            }
            return -1;
        }
    }
}