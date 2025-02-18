#nullable enable
using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    /// <summary>
    /// Parser for the .vtt subtitles files. Does not handle formatting tags within the text; that has to be parsed separately.
    ///
    /// A .vtt file looks like:
    /// WEBVTT
    ///
    /// CUE - 1
    /// 00:00:10.500 --> 00:00:13.000
    /// Elephant's Dream
    ///
    /// CUE - 2
    /// 00:00:15.000 --> 00:00:18.000
    /// At the left we can see...
    /// </summary>
    public class VttParser : ISubtitleParser
    {
        private static readonly Regex _longTimestampRegex = new Regex("(?<H>[0-9]+):(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex _shortTimestampRegex = new Regex("(?<M>[0-9]+):(?<S>[0-9]+)[,\\.](?<m>[0-9]+)", RegexOptions.Compiled);

        private readonly string[] _timecodeDelimiters = new string[] { "-->", "- >", "->" };

        public VttParser() { }

        public List<SubtitleItem> ParseStream(Stream vttStream, Encoding encoding)
        {
            // Test if stream if readable and seekable (just a check, should be good).
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. " +
                                   "Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanSeek);
                throw new ArgumentException(message);
            }

            // Seek the beginning of the stream.
            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, encoding, detectEncodingFromByteOrderMarks: true);

            List<SubtitleItem> items = new List<SubtitleItem>();
            IEnumerator<string> vttSubParts = GetVttSubTitleParts(reader).GetEnumerator();
            if (false == vttSubParts.MoveNext())
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
                foreach (string line in lines)
                {
                    if (subtitleItem.StartTime == 0 && subtitleItem.EndTime == 0)
                    {
                        // We look for the timecodes first.
                        bool success = TryParseTimecodeLine(line, out int startTc, out int endTc);
                        if (success)
                        {
                            subtitleItem.StartTime = startTc;
                            subtitleItem.EndTime = endTc;
                        }
                    }
                    else
                    {
                        // We found the timecode, now we get the text.
                        subtitleItem.Lines.Add(line);
                    }
                }

                if ((subtitleItem.StartTime != 0 || subtitleItem.EndTime != 0) && subtitleItem.Lines.Any())
                {
                    // Parsing succeeded.
                    items.Add(subtitleItem);
                }
            }
            while (vttSubParts.MoveNext());

            // Option 1: If no valid cues were found, throw an exception.
            if (!items.Any())
            {
                throw new FormatException("Parsing as VTT returned no valid cues.");
            }

            return items;
        }

        public async Task<List<SubtitleItem>> ParseStreamAsync(Stream vttStream, Encoding encoding)
        {
            // Test if stream if readable and seekable (just a check, should be good).
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                string message = string.Format("Stream must be seekable and readable in a subtitles parser. " +
                                   "Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   vttStream.CanSeek, vttStream.CanSeek);
                throw new ArgumentException(message);
            }

            // Seek the beginning of the stream.
            vttStream.Position = 0;
            StreamReader reader = new StreamReader(vttStream, encoding, detectEncodingFromByteOrderMarks: true);

            List<SubtitleItem> items = new List<SubtitleItem>();
            IAsyncEnumerator<string> vttBlockEnumerator = GetVttSubTitlePartsAsync(reader).GetAsyncEnumerator();
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
                foreach (string line in lines)
                {
                    if (subtitleItem.StartTime == 0 && subtitleItem.EndTime == 0)
                    {
                        // We look for the timecodes first.
                        bool success = TryParseTimecodeLine(line, out int startTc, out int endTc);
                        if (success)
                        {
                            subtitleItem.StartTime = startTc;
                            subtitleItem.EndTime = endTc;
                        }
                    }
                    else
                    {
                        // We found the timecode, now we get the text.
                        subtitleItem.Lines.Add(line);
                    }
                }

                if ((subtitleItem.StartTime != 0 || subtitleItem.EndTime != 0) && subtitleItem.Lines.Any())
                {
                    // Parsing succeeded.
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
        /// Enumerates the subtitle parts in a VTT file based on the standard line break observed between them.
        /// A VTT subtitle part is in the form:
        ///
        /// CUE - 1
        /// 00:00:20.000 --> 00:00:24.400
        /// Altocumulus clouds occur between six thousand
        ///
        /// The first line is optional, as well as the hours in the time codes.
        /// </summary>
        /// <param name="reader">The textreader associated with the vtt file</param>
        /// <returns>An IEnumerable(string) object containing all the subtitle parts</returns>
        private IEnumerable<string> GetVttSubTitleParts(TextReader reader)
        {
            string line;
            StringBuilder stringBuilder = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    // return only if not empty
                    string res = stringBuilder.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }
                    stringBuilder = new StringBuilder();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }

            if (stringBuilder.Length > 0)
            {
                yield return stringBuilder.ToString();
            }
        }

        private async IAsyncEnumerable<string> GetVttSubTitlePartsAsync(TextReader reader)
        {
            string line;
            StringBuilder sb = new StringBuilder();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    // Return only if not empty.
                    string res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }
                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            string[] parts = line.Split(_timecodeDelimiters, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                // This is not a timecode line.
                startTc = -1;
                endTc = -1;
                return false;
            }
            else
            {
                startTc = ParseVttTimecode(parts[0]);
                endTc = ParseVttTimecode(parts[1]);
                return true;
            }
        }

        /// <summary>
        /// Takes an VTT timecode as a string and parses it into a double (in seconds). A VTT timecode reads as follows:
        /// 00:00:20.000
        /// or
        /// 00:20.000
        /// </summary>
        /// <param name="s">The timecode to parse</param>
        /// <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        private int ParseVttTimecode(string s)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int milliseconds = -1;
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
                int nbOfMs = (int)result.TotalMilliseconds;
                return nbOfMs;
            }

            return -1;
        }
    }
}