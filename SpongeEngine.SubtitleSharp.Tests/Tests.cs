using System.Text;
using SpongeEngine.SubtitleSharp.Parsers;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.SubtitleSharp.Tests
{
    public class SubtitleParserTests
    {
        private readonly SubtitleParser _parser;
        private readonly string _filesFolder;
        private readonly ITestOutputHelper _output;

        public SubtitleParserTests(ITestOutputHelper output)
        {
            _parser = new SubtitleParser(); // Initialize the parser once for all tests
            _filesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            _output = output; // Store the output helper
        }

        // Helper method to get file paths from the Files folder
        private string GetFilePath(string fileName)
        {
            return Path.Combine(_filesFolder, fileName);
        }

        [Fact]
        public void ShouldParseValidVttFileSuccessfully()
        {
            string validVttFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original).vtt");
        
            using (FileStream fileStream = File.OpenRead(validVttFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(validVttFilePath);
                List<SubtitleItem> items = _parser.ParseStream(fileStream, Encoding.UTF8, format);
                Assert.NotEmpty(items);
                Assert.All(items, item =>
                {
                    Assert.True(item.StartTime > 0);
                    Assert.True(item.EndTime > 0);
                });
            }
        }

        [Fact]
        public void ShouldParseValidSrtFileSuccessfully()
        {
            string validSrtFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original).srt");
        
            using (FileStream fileStream = File.OpenRead(validSrtFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(validSrtFilePath);
                List<SubtitleItem> items = _parser.ParseStream(fileStream, Encoding.UTF8, format);
                Assert.NotEmpty(items);
                Assert.All(items, item =>
                {
                    Assert.True(item.StartTime > 0);
                    Assert.True(item.EndTime > 0);
                });
            }
        }

        [Fact]
        public void ShouldParseValidAssFileSuccessfully()
        {
            string validAssFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original).ass");
        
            using (FileStream fileStream = File.OpenRead(validAssFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(validAssFilePath);
                List<SubtitleItem> items = _parser.ParseStream(fileStream, Encoding.UTF8, format);
                Assert.NotEmpty(items);
                Assert.All(items, item =>
                {
                    Assert.True(item.StartTime > 0);
                    Assert.True(item.EndTime > 0);
                });
            }
        }

        [Fact]
        public void ShouldFailForInvalidVtt_MissingCues()
        {
            string invalidVttFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original)_invalid_missingcues.vtt");

            using (FileStream fileStream = File.OpenRead(invalidVttFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(invalidVttFilePath);
                ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                    _parser.ParseStream(fileStream, Encoding.UTF8, format));
                Assert.Contains("All the subtitles parsers failed to parse", ex.Message);
            }
        }

        [Fact]
        public void ShouldFailForInvalidSrt_NoText()
        {
            string invalidSrtFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original)_invalid_notext.srt");

            using (FileStream fileStream = File.OpenRead(invalidSrtFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(invalidSrtFilePath);
                ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                    _parser.ParseStream(fileStream, Encoding.UTF8, format));
                Assert.Contains("All the subtitles parsers failed to parse", ex.Message);
            }
        }
        
        [Fact]
        public void ShouldFailForInvalidSrt_BadTimecode()
        {
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
    invalid_timecode --> 00:00:04,000
    Some text"));

            SrtParser parser = new SrtParser();

            // We expect an ArgumentException to be thrown due to the invalid timecode
            ArgumentException exception = Assert.Throws<ArgumentException>(() => parser.ParseStream(invalidSrtStream, Encoding.UTF8));
    
            // Ensure that the exception message contains the expected error message
            Assert.Contains("Invalid timecode in line", exception.Message);
        }

        [Fact]
        public void ShouldFailForInvalidAss_NoEvents()
        {
            string invalidAssFilePath = GetFilePath("Risitas - Las Paelleras (Extended Original)_invalid_noevents.ass");

            using (FileStream fileStream = File.OpenRead(invalidAssFilePath))
            {
                SubtitlesFormat format = _parser.GetMostLikelyFormat(invalidAssFilePath);
                ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                    _parser.ParseStream(fileStream, Encoding.UTF8, format));
                Assert.Contains("All the subtitles parsers failed to parse", ex.Message);
            }
        }
        
        [Theory]
        [InlineData("00:00:01,000", 1000)]  // valid timecode
        [InlineData("01:30:15,250", 5415250)] // valid timecode
        [InlineData("00:00:00,000", 0)]  // boundary case
        [InlineData("invalid_timecode", -1)] // invalid timecode
        [InlineData("00:00:00,abc", -1)] // invalid timecode with letters
        public void TestParseSrtTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SrtParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }
        
        [Theory]
        [InlineData("00:00:01,000 --> 00:00:04,000", 1000, 4000, true)] // valid timecodes
        [InlineData("invalid_timecode --> 00:00:04,000", -1, -1, false)] // invalid start time
        [InlineData("00:00:01,000 --> invalid_timecode", 1000, -1, false)] // invalid end time
        [InlineData("00:00:01,000 --> 00:00:01,000", 1000, 1000, true)] // valid timecodes, same start and end
        public void TestTryParseTimecodeLine(string line, int expectedStart, int expectedEnd, bool expectedSuccess)
        {
            bool success = SrtParser.TryParseTimecodeLine(line, out int start, out int end);
    
            // Log the test input and result
            _output.WriteLine($"Testing: {line}");
            _output.WriteLine($"Parsed result: Start: {start}, End: {end}, Success: {success}");
    
            Assert.Equal(expectedSuccess, success);
            if (success)
            {
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
            }
        }
        
        // Test parsing of individual timecode strings (unit test for the core functionality)
        [Theory]
        [InlineData("00:00:01,000", 1000)]
        [InlineData("01:30:15,250", 5415250)]
        [InlineData("00:00:00,000", 0)]
        [InlineData("00:00:00,500", 500)]
        public void TestParseValidTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SrtParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }

        // Test invalid timecode string (testing error handling)
        [Theory]
        [InlineData("invalid_timecode", -1)]
        [InlineData("00:00:00,abc", -1)]
        public void TestParseInvalidTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SrtParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }
        
        // Test edge case: same start and end time
        [Theory]
        [InlineData("00:00:01,000 --> 00:00:01,000", 1000, 1000, true)]
        public void TestSameStartAndEndTime(string line, int expectedStart, int expectedEnd, bool expectedSuccess)
        {
            bool success = SrtParser.TryParseTimecodeLine(line, out int start, out int end);
            Assert.Equal(expectedSuccess, success);
            if (success)
            {
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
            }
        }

        [Fact]
        public void TestInvalidSrtStreamWithBadTimecode()
        {
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
            invalid_timecode --> 00:00:04,000
            Some text"));

            SrtParser parser = new SrtParser();
            Assert.Throws<ArgumentException>(() => parser.ParseStream(invalidSrtStream, Encoding.UTF8));
        }

        [Fact]
        public void TestInvalidSrtStreamWithNoText()
        {
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
            00:00:01,000 --> 00:00:04,000"));

            SrtParser parser = new SrtParser();
            Assert.Throws<ArgumentException>(() => parser.ParseStream(invalidSrtStream, Encoding.UTF8));
        }
        
        [Fact]
        public void ShouldParseVttTextSuccessfully()
        {
            // A minimal VTT content sample
            string vttContent = @"WEBVTT

00:00:10.500 --> 00:00:13.000
Subtitle line one

00:00:15.000 --> 00:00:18.000
Subtitle line two";

            // Use the new ParseText overload
            List<SubtitleItem> items = _parser.ParseText(vttContent, Encoding.UTF8);
    
            Assert.NotEmpty(items);
            Assert.Equal(2, items.Count);
            Assert.All(items, item =>
            {
                Assert.True(item.StartTime > 0);
                Assert.True(item.EndTime > 0);
                Assert.NotEmpty(item.Lines);
            });
        }
    }
}