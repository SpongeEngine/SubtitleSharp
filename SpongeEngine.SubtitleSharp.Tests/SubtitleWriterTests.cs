using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace SpongeEngine.SubtitleSharp.Tests 
{
    public class SubtitleWriterTests
    {
        private readonly ITestOutputHelper _output;

        public SubtitleWriterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // Helper method to generate sample subtitle cues.
        private List<SubtitleCue> GetSampleCues()
        {
            return new List<SubtitleCue>
            {
                new SubtitleCue
                {
                    StartTime = 1000,
                    EndTime = 2000,
                    Lines = new List<string> { "How are you?" },
                    PlaintextLines = new List<string> { "How are you?" }
                },
                new SubtitleCue
                {
                    StartTime = 3000,
                    EndTime = 4000,
                    Lines = new List<string> { "Fine, thanks" },
                    PlaintextLines = new List<string> { "Fine, thanks" }
                },
                new SubtitleCue
                {
                    StartTime = 5000,
                    EndTime = 6000,
                    Lines = new List<string> { "You're welcome" },
                    PlaintextLines = new List<string> { "You're welcome" }
                }
            };
        }

        // Helper method that uses System.Text.Json to serialize the string so that newline characters appear as escaped sequences.
        private string GetUglyOutput(string input)
        {
            // JsonSerializer.Serialize will escape newline characters.
            string serialized = JsonSerializer.Serialize(input);
            // Remove the surrounding quotes.
            if (serialized.Length >= 2 && serialized[0] == '"' && serialized[^1] == '"')
            {
                return serialized.Substring(1, serialized.Length - 2);
            }
            return serialized;
        }

        [Fact]
        public void WriteToText_DefaultOptions_ShouldIncludeTimecodeAndFormatting()
        {
            var cues = GetSampleCues();
            // Use default options: IncludeTimecode and IncludeFormatting are true.
            var options = new SubtitleWriterOptions();
            var writer = new SubtitleWriter();
            string srtText = writer.WriteToText(cues, options);

            // Use the serializer helper to show escaped newline characters.
            _output.WriteLine(GetUglyOutput(srtText));
            // Verify sequence numbers, timecode delimiter, and cue text are present.
            Assert.Contains("1", srtText);
            Assert.Contains("-->", srtText);
            Assert.Contains("How are you?", srtText);
        }

        [Fact]
        public void WriteToText_NoTimecode_ShouldOmitTimecodeLine()
        {
            var cues = GetSampleCues();
            // Set options to not include timecodes.
            var options = new SubtitleWriterOptions
            {
                IncludeTimecode = false,
                IncludeFormatting = true
            };
            var writer = new SubtitleWriter();
            string srtText = writer.WriteToText(cues, options);

            _output.WriteLine(GetUglyOutput(srtText));
            // Verify that the timecode delimiter is missing while sequence numbers and text are present.
            Assert.DoesNotContain("-->", srtText);
            Assert.Contains("1", srtText);
            Assert.Contains("How are you?", srtText);
        }

        [Fact]
        public void ToSrtText_ExtensionMethod_DefaultOptions()
        {
            var cues = GetSampleCues();
            var options = new SubtitleWriterOptions(); // Default options.
            // Using the extension method to get SRT text.
            string srtText = cues.ToSrtText(options);

            _output.WriteLine(GetUglyOutput(srtText));
            Assert.Contains("-->", srtText);
            Assert.Contains("Fine, thanks", srtText);
        }

        [Fact]
        public async Task WriteStreamAsync_ShouldProduceValidOutput()
        {
            var cues = GetSampleCues();
            var options = new SubtitleWriterOptions { IncludeTimecode = true, IncludeFormatting = true };
            var writer = new SubtitleWriter();

            using MemoryStream ms = new MemoryStream();
            await writer.WriteStreamAsync(ms, cues, options);
            ms.Position = 0;
            string output = new StreamReader(ms).ReadToEnd();
            _output.WriteLine(GetUglyOutput(output));
            Assert.Contains("-->", output);
            Assert.Contains("You're welcome", output);
        }
    }
}
