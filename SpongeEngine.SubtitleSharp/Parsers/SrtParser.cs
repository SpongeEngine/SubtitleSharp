using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp.Parsers
{
    public class SrtParser : ISubtitlesParser
    {
        private static readonly string[] _delimiters = { "-->", "- >", "->" };

        public SrtParser() { }

        public List<SubtitleItem> ParseStream(Stream srtStream, Encoding encoding)
        {
            if (!srtStream.CanRead || !srtStream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable and readable.");
            }

            srtStream.Position = 0;
            var reader = new StreamReader(srtStream, encoding, detectEncodingFromByteOrderMarks: true);
            var items = new List<SubtitleItem>();
            var srtSubParts = GetSrtSubTitleParts(reader).ToList();

            if (!srtSubParts.Any())
            {
                throw new FormatException("Parsing as SRT returned no subtitle parts.");
            }

            foreach (var srtSubPart in srtSubParts)
            {
                var lines = srtSubPart
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                var item = new SubtitleItem();
                bool timecodeFound = false;

                foreach (var line in lines)
                {
                    Console.WriteLine($"[DEBUG] Parsing line: {line}");

                    if (!timecodeFound)
                    {
                        int startTc, endTc;
                        bool success = TryParseTimecodeLine(line, out startTc, out endTc);

                        if (!success)
                        {
                            Console.WriteLine($"[DEBUG] Timecode parsing failed for line: {line}");
                            throw new ArgumentException($"Invalid timecode in line: {line}");
                        }

                        item.StartTime = startTc;
                        item.EndTime = endTc;
                        timecodeFound = true;
                        continue; // Move to next part (text)
                    }
                    else
                    {
                        item.Lines.Add(line);
                        item.PlaintextLines.Add(Regex.Replace(line, @"\{.*?\}|<.*?>", string.Empty)); // Remove any formatting tags
                    }
                }

                if (timecodeFound && item.Lines.Any())
                {
                    items.Add(item);
                }
                else
                {
                    throw new ArgumentException($"Subtitle block with missing or invalid timecode or text: {string.Join(", ", lines)}");
                }
            }

            if (!items.Any())
            {
                throw new ArgumentException("No valid subtitle items found in the stream.");
            }

            return items;
        }

        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
            string line;
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrWhiteSpace(res))
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
        
        public static bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            // Split the line using the delimiter (-->)
            var parts = line.Split(_delimiters, StringSplitOptions.None);
    
            // Debugging: log the parts after splitting
            Console.WriteLine($"Line: {line}");
            Console.WriteLine($"Parts: {string.Join(" | ", parts)}");  // Show the parts split by --> 
    
            if (parts.Length != 2)
            {
                startTc = -1;
                endTc = -1;
                return false;
            }

            // Parse the start and end timecodes using ParseSrtTimecode
            startTc = ParseSrtTimecode(parts[0].Trim());  // Trim whitespace to be safe
            endTc = ParseSrtTimecode(parts[1].Trim());    // Trim whitespace to be safe
    
            // Debugging: log the parsed timecodes
            Console.WriteLine($"Start Timecode: {startTc}, End Timecode: {endTc}");
    
            // Return true if both timecodes are valid
            if (startTc != -1 && endTc != -1)
            {
                return true;
            }

            startTc = -1;
            endTc = -1;
            return false;
        }

        public static int ParseSrtTimecode(string s)
        {
            var match = Regex.Match(s, @"^(\d{2}):(\d{2}):(\d{2}),(\d{3})$");

            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);

                // Log parsed values
                Console.WriteLine($"Parsed timecode: {hours}:{minutes}:{seconds},{milliseconds}");

                // Calculate and return total milliseconds
                return (int)(new TimeSpan(hours, minutes, seconds).TotalMilliseconds + milliseconds);
            }
            else
            {
                // Log failure to match
                Console.WriteLine($"Failed to parse timecode: {s}");
                return -1; // Invalid timecode
            }
        }
    }
}