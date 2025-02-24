using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    public class SrtParser : ISubtitleParser
    {
        private static readonly string[] _timecodeDelimiters = { "-->", "- >", "->" };

        public SrtParser() { }

        // For backward compatibility:
        public List<SubtitleItem> ParseStream(Stream srtStream, Encoding encoding)
        {
            return ParseStream(srtStream, new SubtitleParserOptions { Encoding = encoding, TimecodeMode = SubtitleTimecodeMode.Required });
        }

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
                    Console.WriteLine($"[DEBUG] Parsing line: {line}");
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
                                Console.WriteLine($"[DEBUG] Timecode parsing failed for line: {line}");
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

        public static bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            string[] parts = line.Split(_timecodeDelimiters, StringSplitOptions.None);
            Console.WriteLine($"Line: {line}");
            Console.WriteLine($"Parts: {string.Join(" | ", parts)}");
            if (parts.Length != 2)
            {
                startTc = -1;
                endTc = -1;
                return false;
            }
            startTc = ParseSrtTimecode(parts[0].Trim());
            endTc = ParseSrtTimecode(parts[1].Trim());
            Console.WriteLine($"Start Timecode: {startTc}, End Timecode: {endTc}");
            return startTc != -1 && endTc != -1;
        }

        public static int ParseSrtTimecode(string s)
        {
            Match match = Regex.Match(s, @"^(\d{2}):(\d{2}):(\d{2}),(\d{3})$");
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);
                Console.WriteLine($"Parsed timecode: {hours}:{minutes}:{seconds},{milliseconds}");
                return (int)(new TimeSpan(hours, minutes, seconds).TotalMilliseconds + milliseconds);
            }
            else
            {
                Console.WriteLine($"Failed to parse timecode: {s}");
                return -1;
            }
        }
    }
}