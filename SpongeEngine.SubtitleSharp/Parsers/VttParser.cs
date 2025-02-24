#nullable enable
using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Parser for .vtt subtitle files. (Note that formatting tags are not processed here.)
    /// </summary>
    public class VttParser : ISubtitleParser
    {
        private static readonly Regex _longTimestampRegex = new Regex("(?<H>[0-9]+):(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex _shortTimestampRegex = new Regex("(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);
        private readonly string[] _timecodeDelimiters = new string[] { "-->", "- >", "->" };

        public VttParser() { }

        // For backward compatibility:
        public List<SubtitleItem> ParseStream(Stream vttStream, Encoding encoding)
        {
            return ParseStream(vttStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        public List<SubtitleItem> ParseStream(Stream vttStream, SubtitleParserOptions options)
        {
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanRead);
                throw new ArgumentException(message);
            }

            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> items = new List<SubtitleItem>();
            IEnumerator<string> vttSubParts = GetVttSubTitleParts(reader).GetEnumerator();
            int dummyTime = 0, defaultDuration = 1000;

            if (!vttSubParts.MoveNext())
            {
                throw new FormatException("Parsing as VTT returned no VTT part.");
            }

            do
            {
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
                        bool success = TryParseTimecodeLine(line, out int startTc, out int endTc);
                        if (success)
                        {
                            subtitleItem.StartTime = startTc;
                            subtitleItem.EndTime = endTc;
                            timecodeFound = true;
                        }
                        else if (options.TimecodeMode == SubtitleTimecodeMode.Optional)
                        {
                            // Assign dummy timecodes if timecodes are missing.
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
            while (vttSubParts.MoveNext());

            if (!items.Any())
            {
                throw new FormatException("Parsing as VTT returned no valid cues.");
            }

            return items;
        }

        public async Task<List<SubtitleItem>> ParseStreamAsync(Stream vttStream, Encoding encoding)
        {
            return await ParseStreamAsync(vttStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

        public async Task<List<SubtitleItem>> ParseStreamAsync(Stream vttStream, SubtitleParserOptions options)
        {
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanRead);
                throw new ArgumentException(message);
            }

            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> items = new List<SubtitleItem>();
            IAsyncEnumerator<string> vttBlockEnumerator = GetVttSubTitlePartsAsync(reader).GetAsyncEnumerator();
            int dummyTime = 0, defaultDuration = 1000;

            if (await vttBlockEnumerator.MoveNextAsync() == false)
            {
                throw new FormatException("Parsing as VTT returned no VTT part.");
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
