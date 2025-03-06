using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            _parser = new SubtitleParser(); // Initialize once for all tests.
            _filesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            _output = output;
        }

        // Helper to get file paths from the Files folder.
        private string GetFilePath(string fileName) => Path.Combine(_filesFolder, fileName);

        // Helper to parse a file and perform assertions.
        private void ParseFileAndAssert(string fileName, SubtitleTimecodeMode mode, Action<List<SubtitleCue>> assertAction)
        {
            string filePath = GetFilePath(fileName);
            using FileStream fileStream = File.OpenRead(filePath);
            List<SubtitleCue> items = _parser.ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = mode });
            foreach (var cue in items)
            {
                _output.WriteLine(cue.ToString());
            }
            assertAction(items);
        }

        #region File Parsing Tests

        [Fact]
        public void ShouldParseValidSrtFileSuccessfully()
        {
            // Test a file with valid timecodes.
            ParseFileAndAssert("downfall_steiners_attack.srt", SubtitleTimecodeMode.Required, items =>
            {
                Assert.NotEmpty(items);
                Assert.All(items, item =>
                {
                    Assert.True(item.StartTime > 0);
                    Assert.True(item.EndTime > 0);
                });
            });
        }

        [Fact]
        public void ShouldParseNoTimecodesSubtitlesSuccessfully()
        {
	        // Test a file that uses Optional mode to assign dummy timecodes.
	        ParseFileAndAssert("downfall_steiners_attack_no_timecodes.srt", SubtitleTimecodeMode.Optional, items =>
	        {
		        Assert.NotEmpty(items);
	        });
        }
        
        [Fact]
        public void ShouldParseNoTimecodesNoNewlineSeparationSubtitlesSuccessfully()
        {
	        // Test a file that uses Optional mode to assign dummy timecodes.
	        ParseFileAndAssert("downfall_steiners_attack_no_timecodes_no_newline_separation.srt", SubtitleTimecodeMode.Optional, items =>
	        {
		        Assert.NotEmpty(items);
	        });
        }

        [Fact]
        public void ShouldFailForInvalidSrt_NoText()
        {
            string invalidSrtFilePath = GetFilePath("downfall_steiners_attack_invalid_notext.srt");

            using FileStream fileStream = File.OpenRead(invalidSrtFilePath);
            ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                _parser.ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required })
            );
            Assert.Contains("No valid subtitle items found", ex.Message);
        }

        [Fact]
        public void ShouldFailForInvalidSrt_BadTimecode()
        {
            // Create an in-memory stream with an invalid timecode.
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
invalid_timecode --> 00:00:04,000
Some text"));

            SubtitleParser subtitleParser = new SubtitleParser();
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                subtitleParser.ParseStream(invalidSrtStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required })
            );
            Assert.Contains("Subtitle block with missing or invalid", exception.Message);
        }

        [Fact]
        public void TestInvalidSrtStreamWithBadTimecode()
        {
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
invalid_timecode --> 00:00:04,000
Some text"));

            SubtitleParser subtitleParser = new SubtitleParser();
            Assert.Throws<ArgumentException>(() =>
                subtitleParser.ParseStream(invalidSrtStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required })
            );
        }

        [Fact]
        public void TestInvalidSrtStreamWithNoText()
        {
            MemoryStream invalidSrtStream = new MemoryStream(Encoding.UTF8.GetBytes(@"1
00:00:01,000 --> 00:00:04,000"));

            SubtitleParser subtitleParser = new SubtitleParser();
            Assert.Throws<ArgumentException>(() =>
                subtitleParser.ParseStream(invalidSrtStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Required })
            );
        }

        #endregion

        #region Timecode Parsing Tests

        [Theory]
        [InlineData("00:00:01,000", 1000)]
        [InlineData("01:30:15,250", 5415250)]
        [InlineData("00:00:00,000", 0)]
        [InlineData("invalid_timecode", -1)]
        [InlineData("00:00:00,abc", -1)]
        public void TestParseSrtTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SubtitleParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }

        [Theory]
        [InlineData("00:00:01,000 --> 00:00:04,000", 1000, 4000, true)]
        [InlineData("invalid_timecode --> 00:00:04,000", -1, -1, false)]
        [InlineData("00:00:01,000 --> invalid_timecode", 1000, -1, false)]
        [InlineData("00:00:01,000 --> 00:00:01,000", 1000, 1000, true)]
        public void TestTryParseTimecodeLine(string line, int expectedStart, int expectedEnd, bool expectedSuccess)
        {
            bool success = SubtitleParser.TryParseTimecodeLine(line, out int start, out int end);
            _output.WriteLine($"Testing: {line}");
            _output.WriteLine($"Parsed result: Start: {start}, End: {end}, Success: {success}");

            Assert.Equal(expectedSuccess, success);
            if (success)
            {
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
            }
        }

        [Theory]
        [InlineData("00:00:01,000", 1000)]
        [InlineData("01:30:15,250", 5415250)]
        [InlineData("00:00:00,000", 0)]
        [InlineData("00:00:00,500", 500)]
        public void TestParseValidTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SubtitleParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }

        [Theory]
        [InlineData("invalid_timecode", -1)]
        [InlineData("00:00:00,abc", -1)]
        public void TestParseInvalidTimecode(string timecode, int expectedMilliseconds)
        {
            int result = SubtitleParser.ParseSrtTimecode(timecode);
            Assert.Equal(expectedMilliseconds, result);
        }

        [Theory]
        [InlineData("00:00:01,000 --> 00:00:01,000", 1000, 1000, true)]
        public void TestSameStartAndEndTime(string line, int expectedStart, int expectedEnd, bool expectedSuccess)
        {
            bool success = SubtitleParser.TryParseTimecodeLine(line, out int start, out int end);
            Assert.Equal(expectedSuccess, success);
            if (success)
            {
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
            }
        }

        #endregion
    }
}
