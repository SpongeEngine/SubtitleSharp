# SubtitleSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![Run Tests](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml/badge.svg)](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml)
[![License](https://img.shields.io/github/license/SpongeEngine/SubtitleSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%2B-512BD4)](https://dotnet.microsoft.com/download)

SubtitleSharp is a robust C# library for parsing and writing subtitle files. It supports multiple subtitle formats—including SubRip (SRT), WebVTT (VTT), and SubStation Alpha (SSA)—and provides a unified API for both synchronous and asynchronous subtitle processing.


## Features
- **Multi-Format Support:** Parse and write subtitles in SRT, VTT, and SSA formats.
- **Unified Parsing API:** Automatically detect and parse subtitles via file extension or by specifying a preferred format using the `SubtitleParser` class.
- **Customizable Options:** Configure parsing behavior using `SubtitleParserOptions` and control timecode requirements with `SubtitleTimecodeMode`.
- **Asynchronous Processing:** Fully supports async/await for non-blocking I/O operations.
- **Cross-Platform Compatibility:** Works with .NET 6.0, .NET 7.0, and .NET 8.0+.

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.SubtitleSharp
```
## Examples
### Automatic Format Detection
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

// Open the subtitle file as a stream.
using var fileStream = new FileStream("path_to_subtitle.srt", FileMode.Open, FileAccess.Read);

// Initialize the parser with default options.
var parser = new SubtitleParser();

// Parse the stream into a list of SubtitleCue objects.
var SubtitleCues = parser.ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8 });

// Output subtitle start and end times.
foreach (var item in SubtitleCues)
{
    Console.WriteLine($"Start: {item.StartTime} ms, End: {item.EndTime} ms");
    foreach (var line in item.Lines)
    {
        Console.WriteLine(line);
    }
}
```

### Parsing from Text
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

string subtitleContent = File.ReadAllText("path_to_subtitle.vtt", Encoding.UTF8);
var SubtitleCues = new SubtitleParser().ParseText(subtitleContent, new SubtitleParserOptions {});
```

### Specifying a Preferred Format
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

// Specify the preferred format (e.g., SubRip for SRT files).
var preferredFormat = SubtitlesFormat.SubRipFormat;

using var fileStream = new FileStream("path_to_subtitle.srt", FileMode.Open, FileAccess.Read);
var SubtitleCues = new SubtitleParser().ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8, PrioritizedSubtitleFormat = SubtitlesFormat.SubRipFormat });
```

### Asynchronous Parsing
```csharp
using System.Text;
using SpongeEngine.SubtitleSharp;

using var fileStream = new FileStream("path_to_subtitle.ssa", FileMode.Open, FileAccess.Read);
var parser = new SubtitleParser();
var SubtitleCues = await parser.ParseStreamAsync(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8 });
```
### Writing SRT Files
```csharp
using SpongeEngine.SubtitleSharp;
using SpongeEngine.SubtitleSharp.Writers;

// Assume SubtitleCues is a List<SubtitleCue> obtained from parsing.
using var outputStream = new FileStream("output.srt", FileMode.Create, FileAccess.Write);
var srtWriter = new SrtWriter();
srtWriter.WriteStream(outputStream, SubtitleCues);
```

### Writing SSA Files
```
using SpongeEngine.SubtitleSharp;
using SpongeEngine.SubtitleSharp.Writers;

using var outputStream = new FileStream("output.ssa", FileMode.Create, FileAccess.Write);
var ssaWriter = new SsaWriter();
await ssaWriter.WriteStreamAsync(outputStream, SubtitleCues);
```

### Logging and Error Handling
```csharp
using Microsoft.Extensions.Logging;
using SpongeEngine.SubtitleSharp;

ILogger logger = LoggerFactory
    .Create(builder => builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<SubtitleParser>();

try
{
    var parser = new SubtitleParser(logger);
    using var fileStream = new FileStream("path_to_subtitle.srt", FileMode.Open, FileAccess.Read);
    var SubtitleCues = parser.ParseStream(fileStream, new SubtitleParserOptions { Encoding = Encoding.UTF8 });
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error parsing subtitle: {ex.Message}");
}

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
