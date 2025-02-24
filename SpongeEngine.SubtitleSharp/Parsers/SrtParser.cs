using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Implements parsing for SubRip (SRT) subtitle files.
    /// 
    /// An SRT file typically has the following structure:
    /// 
    ///     1
    ///     00:18:03,875 --> 00:18:04,231
    ///     Oh?
    ///     
    ///     2
    ///     00:18:05,194 --> 00:18:05,905
    ///     What was that?
    /// </summary>
    public class SrtParser : ISubtitleParser
    {
        private static readonly string[] _timecodeDelimiters = { "-->", "- >", "->" };

        /// <summary>
        /// Initializes a new instance of the <see cref="SrtParser"/> class.
        /// </summary>
        public SrtParser() { }

        /// <summary>
        /// Parses an SRT stream using the provided parser options.
        /// </summary>
        /// <param name="srtStream">A seekable and readable stream containing SRT data.</param>
        /// <param name="options">Options specifying encoding and timecode mode.</param>
        /// <returns>A list of <see cref="SubtitleItem"/> objects extracted from the stream.</returns>
        /// <exception cref="ArgumentException">Thrown if the stream is not readable/seekable or if a subtitle block is invalid.</exception>
        public List<SubtitleItem> ParseStream(Stream srtStream, SubtitleParserOptions options)
        {
            if (!srtStream.CanRead || !srtStream.CanSeek)
                throw new ArgumentException("Stream must be seekable and readable.");

            srtStream.Position = 0;
            StreamReader reader = new StreamReader(srtStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> items = new List<SubtitleItem>();
            List<string> srtSubParts = GetSrtSubTitleParts(reader).ToList();
            if (!srtSubParts.Any())
                throw new FormatException("Parsing as SRT returned no subtitle parts.");

            int dummyTime = 0, defaultDuration = 1000;
            foreach (string srtSubPart in srtSubParts)
            {
                List<string> lines = srtSubPart
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                SubtitleItem item = new SubtitleItem();
                bool timecodeFound = false;
                foreach (string line in lines)
                {
                    // Debug logging can be enabled here if needed.
                    if (!timecodeFound)
                    {
                        int startTc, endTc;
                        bool success = TryParseTimecodeLine(line, out startTc, out endTc);
                        if (!success)
                        {
                            if (options.TimecodeMode == SubtitleTimecodeMode.Optional)
                            {
                                startTc = dummyTime;
                                endTc = dummyTime + defaultDuration;
                                dummyTime += defaultDuration;
                                timecodeFound = true;
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid timecode in line: {line}");
                            }
                        }
                        else
                        {
                            item.StartTime = startTc;
                            item.EndTime = endTc;
                            timecodeFound = true;
                            continue;
                        }
                    }
                    else
                    {
                        item.Lines.Add(line);
                        item.PlaintextLines.Add(Regex.Replace(line, @"\{.*?\}|<.*?>", string.Empty));
                    }
                }

                if (timecodeFound && item.Lines.Any())
                    items.Add(item);
                else if (options.TimecodeMode != SubtitleTimecodeMode.Optional)
                    throw new ArgumentException($"Subtitle block with missing or invalid timecode or text: {string.Join(", ", lines)}");
            }

            if (!items.Any())
                throw new ArgumentException("No valid subtitle items found in the stream.");

            return items;
        }

        /// <summary>
        /// Splits the SRT file into individual subtitle blocks using blank lines as delimiters.
        /// </summary>
        /// <param name="reader">A <see cref="TextReader"/> for the SRT file.</param>
        /// <returns>An enumerable sequence of subtitle block strings.</returns>
        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
            string line;
            StringBuilder stringBuilder = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    string res = stringBuilder.ToString().TrimEnd();
                    if (!string.IsNullOrWhiteSpace(res))
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
        /// Attempts to parse a timecode line in an SRT block into start and end timecodes.
        /// </summary>
        /// <param name="line">A line expected to contain two timecodes separated by a delimiter.</param>
        /// <param name="startTc">Output start timecode (in milliseconds) if parsing succeeds; otherwise -1.</param>
        /// <param name="endTc">Output end timecode (in milliseconds) if parsing succeeds; otherwise -1.</param>
        /// <returns><c>true</c> if parsing is successful; otherwise, <c>false</c>.</returns>
        public static bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            string[] parts = line.Split(_timecodeDelimiters, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                startTc = -1;
                endTc = -1;
                return false;
            }
            startTc = ParseSrtTimecode(parts[0].Trim());
            endTc = ParseSrtTimecode(parts[1].Trim());
            return startTc != -1 && endTc != -1;
        }

        /// <summary>
        /// Parses an SRT timecode string into its equivalent value in milliseconds.
        /// </summary>
        /// <param name="s">An SRT timecode string in the format hh:mm:ss,fff.</param>
        /// <returns>The timecode in milliseconds, or -1 if parsing fails.</returns>
        public static int ParseSrtTimecode(string s)
        {
            Match match = Regex.Match(s, @"^(\d{2}):(\d{2}):(\d{2}),(\d{3})$");
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);
                return (int)(new TimeSpan(hours, minutes, seconds).TotalMilliseconds + milliseconds);
            }
            else
            {
                return -1;
            }
        }
    }
}