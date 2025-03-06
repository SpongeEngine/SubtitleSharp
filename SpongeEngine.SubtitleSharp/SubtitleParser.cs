using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpongeEngine.SubtitleSharp
{
    /// <summary>
    /// Provides functionality for parsing subtitles from various formats.
    /// 
    /// This class selects the appropriate subtitle parser based on file extension or preferred format
    /// and delegates parsing to the underlying format-specific parser.
    /// </summary>
    public class SubtitleParser
    {
        private static readonly string[] _timecodeDelimiters = { "-->", "- >", "->" };
        
        /// <summary>
        /// Parses an SRT stream using the provided parser options.
        /// </summary>
        /// <param name="srtStream">A seekable and readable stream containing SRT data.</param>
        /// <param name="options">Options specifying encoding and timecode mode.</param>
        /// <returns>A list of <see cref="SubtitleCue"/> objects extracted from the stream.</returns>
        /// <exception cref="ArgumentException">Thrown if the stream is not readable/seekable or if a subtitle block is invalid.</exception>
        public List<SubtitleCue> ParseStream(Stream srtStream, SubtitleParserOptions options)
        {
            if (!srtStream.CanRead || !srtStream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable and readable.");
            }

            srtStream.Position = 0;
            using var reader = new StreamReader(srtStream, options.Encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleCue> items = new();
            List<string> srtSubParts = GetSrtSubTitleParts(reader).ToList();

            if (!srtSubParts.Any())
            {
                throw new FormatException("Parsing as SRT returned no subtitle parts.");
            }

            int dummyTime = 0, defaultDuration = 1000;

            foreach (string srtSubPart in srtSubParts)
            {
                var allLines = srtSubPart
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                // Remove numeric sequence lines.
                var lines = allLines.Where(l => !Regex.IsMatch(l, @"^\d+$")).ToList();
                var cue = new SubtitleCue();

                int startTc = -1, endTc = -1;
                string timecodeLine = null;
				// Find the first valid timecode line and assign its values.
                foreach (string line in lines)
                {
	                if (TryParseTimecodeLine(line, out int tmpStart, out int tmpEnd))
	                {
		                timecodeLine = line;
		                startTc = tmpStart;
		                endTc = tmpEnd;
		                break;
	                }
                }

                if (timecodeLine != null)
                {
	                cue.StartTime = startTc;
	                cue.EndTime = endTc;
	                foreach (string line in lines)
	                {
		                if (line == timecodeLine)
			                continue;
		                cue.Lines.Add(line);
		                cue.PlaintextLines.Add(Regex.Replace(line, @"\{.*?\}|<.*?>", string.Empty));
	                }
                }
                else
                {
	                if (options.TimecodeMode == SubtitleTimecodeMode.Optional)
	                {
		                // Assign dummy timecodes.
		                cue.StartTime = dummyTime;
		                cue.EndTime = dummyTime + defaultDuration;
		                dummyTime += defaultDuration;

		                foreach (string line in lines)
		                {
			                cue.Lines.Add(line);
			                cue.PlaintextLines.Add(Regex.Replace(line, @"\{.*?\}|<.*?>", string.Empty));
		                }
	                }
	                else if (options.TimecodeMode == SubtitleTimecodeMode.None)
	                {
		                // Timecodes are not expected; set both to 0.
		                cue.StartTime = 0;
		                cue.EndTime = 0;
		                foreach (string line in lines)
		                {
			                cue.Lines.Add(line);
			                cue.PlaintextLines.Add(Regex.Replace(line, @"\{.*?\}|<.*?>", string.Empty));
		                }
	                }
	                else
	                {
		                throw new ArgumentException($"Subtitle block with missing or invalid timecode or text: {string.Join(", ", lines)}");
	                }
                }

                if (cue.Lines.Any())
                {
	                items.Add(cue);
                }
            }

            if (!items.Any())
            {
	            throw new ArgumentException("No valid subtitle items found in the stream.");
            }

            return items;
        }

        /// <summary>
        /// Splits the SRT file into individual subtitle blocks using blank lines as delimiters.
        /// </summary>
        /// <param name="reader">A <see cref="TextReader"/> for the SRT file.</param>
        /// <returns>An enumerable sequence of subtitle block strings.</returns>
        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
	        string content = reader.ReadToEnd();

	        // Try splitting using a blank line (supporting both Windows and Unix newlines).
	        var parts = content.Split(new string[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
	        if (parts.Length > 1)
	        {
		        foreach (var part in parts)
		        {
			        string trimmed = part.Trim();
			        if (!string.IsNullOrEmpty(trimmed))
				        yield return trimmed;
		        }
	        }
	        else
	        {
		        // Fallback: group lines by detecting a line with only digits as a new block.
		        var lines = content.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
		        var block = new List<string>();
		        foreach (var line in lines)
		        {
			        string trimmedLine = line.Trim();
			        // If the line is numeric, use it as a block separator but do not add it to the block.
			        if (Regex.IsMatch(trimmedLine, @"^\d+$"))
			        {
				        if (block.Count > 0)
				        {
					        yield return string.Join("\n", block).Trim();
					        block.Clear();
				        }
				        // Skip adding the numeric line.
				        continue;
			        }
			        block.Add(line);
		        }

		        if (block.Count > 0)
		        {
			        yield return string.Join("\n", block).Trim();
		        }
	        }
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
            return -1;
        }

        /// <summary>
        /// Parses subtitle content provided as a string.
        /// </summary>
        /// <param name="subtitleContent">The subtitle content.</param>
        /// <param name="subtitleParserOptions">Subtitle parser options.</param>
        /// <returns>A list of <see cref="SubtitleCue"/> objects extracted from the content.</returns>
        /// <exception cref="ArgumentException">Thrown if the subtitle content is null or empty.</exception>
        public List<SubtitleCue> ParseText(string subtitleContent, SubtitleParserOptions subtitleParserOptions)
        {
            if (string.IsNullOrWhiteSpace(subtitleContent))
            {
                throw new ArgumentException("Subtitle text cannot be null or empty.", nameof(subtitleContent));
            }

            using MemoryStream stream = new MemoryStream(subtitleParserOptions.Encoding.GetBytes(subtitleContent));
            return ParseStream(stream, subtitleParserOptions);
        }
    }
}
