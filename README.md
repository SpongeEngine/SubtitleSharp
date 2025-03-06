# SubtitleSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![Run Tests](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml/badge.svg)](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml)
[![License](https://img.shields.io/github/license/SpongeEngine/SubtitleSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-7.0%20%7C%208.0%20%7C%209.0%2B-512BD4)](https://dotnet.microsoft.com/download)

SubtitleSharp is a C# library for parsing and writing subtitles in the SubRip (SRT) format.

## Features
- **Only SRT Support:** Parse and write subtitles in SRT.
- **Customizable Options:** Configure parsing behavior using `SubtitleParserOptions` and control timecode requirements with `SubtitleTimecodeMode`.
- **Asynchronous Processing:** Fully supports async/await for non-blocking I/O operations.
- **Cross-Platform Compatibility:** Works with .NET 7.0, .NET 8.0, and .NET 9.0+.

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.SubtitleSharp
```
## Examples
### Parsing SRT Files
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

// Open the SRT file as a stream.
using FileStream fileStream = new FileStream("path_to_subtitle.srt", FileMode.Open, FileAccess.Read);

// Initialize the parser.
SubtitleParser parser = new SubtitleParser();

// Parse the stream into a list of SubtitleCue objects.
List<SubtitleCue> subtitleCues = parser.ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8 });

// Output subtitle start and end times.
foreach (SubtitleCue subtitleCue in subtitleCues)
{
    Console.WriteLine($"Start: {subtitleCue.StartTime} ms, End: {subtitleCue.EndTime} ms");
    foreach (string line in subtitleCue.Lines)
    {
        Console.WriteLine(line);
    }

```

### Parsing from Text
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

string subtitleContent = File.ReadAllText("path_to_subtitle.srt", Encoding.UTF8);
List<SubtitleCue> subtitleCues = new SubtitleParser().ParseText(subtitleContent, new SubtitleParserOptions { Encoding = Encoding.UTF8 });
```

### Optional Timecode Mode
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

using FileStream fileStream = new FileStream("path_to_subtitle_no_timecodes.srt", FileMode.Open, FileAccess.Read);
List<SubtitleCue> subtitleCues = new SubtitleParser().ParseStream(
    fileStream, 
    new SubtitleParserOptions { Encoding = Encoding.UTF8, TimecodeMode = SubtitleTimecodeMode.Optional }
);

// Each cue will have dummy start and end times assigned.
```

### Writing SRT Files
```csharp
using SpongeEngine.SubtitleSharp;
using System.IO;

// Assume subtitleCues is a List<SubtitleCue> obtained from parsing.
using FileStream outputStream = new FileStream("output.srt", FileMode.Create, FileAccess.Write);
SubtitleWriter writer = new SubtitleWriter();
writer.WriteStream(outputStream, subtitleCues);
```
### Writing SRT Files
```csharp
using SpongeEngine.SubtitleSharp;
using SpongeEngine.SubtitleSharp.Writers;

// Assume subtitleCues is a List<SubtitleCue> obtained from parsing.
using FileStream outputStream = new FileStream("output.srt", FileMode.Create, FileAccess.Write);
SrtWriter srtWriter = new SrtWriter();
srtWriter.WriteStream(outputStream, subtitleCues);
```

### Asynchronous Writing
```csharp
using SpongeEngine.SubtitleSharp;
using System.IO;
using System.Threading.Tasks;

// Assume subtitleCues is a List<SubtitleCue> obtained from parsing.
using FileStream outputStream = new FileStream("output.srt", FileMode.Create, FileAccess.Write);
SubtitleWriter writer = new SubtitleWriter();
await writer.WriteStreamAsync(outputStream, subtitleCues);
```

### Testing
To run the unit tests, execute:
```bash
dotnet test
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## Support
For issues and feature requests, please use the [GitHub issues page](https://github.com/SpongeEngine/SubtitleSharp/issues).
