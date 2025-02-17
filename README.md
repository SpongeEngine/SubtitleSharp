# SubtitleSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.SubtitleSharp.svg)](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)
[![Run Tests](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml/badge.svg)](https://github.com/SpongeEngine/SubtitleSharp/actions/workflows/run-tests.yml)
[![License](https://img.shields.io/github/license/SpongeEngine/SubtitleSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%2B-512BD4)](https://dotnet.microsoft.com/download)

C# library for parsing and handling subtitle files (e.g., SRT, VTT, ASS).

## Features
- Supports parsing subtitle formats including **SubRip (SRT)**, **WebVTT (VTT)**, and **SubStation Alpha (ASS)**.
- Converts subtitle files to structured data (`SubtitleItem`) for easy processing.
- Handles timecode validation and parsing.
- Cross-platform compatibility with .NET 6.0+.
- Async/await support for non-blocking subtitle processing.

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.SubtitleSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.SubtitleSharp
```

## Quick Start

### Basic Usage
```csharp
using SpongeEngine.SubtitleSharp;

// Parse an SRT file into structured subtitle items
var srtFilePath = "path_to_subtitle.srt";
using var fileStream = new FileStream(srtFilePath, FileMode.Open, FileAccess.Read);
var parser = new SrtParser();
var subtitleItems = parser.ParseStream(fileStream, Encoding.UTF8);

// Output subtitle start and end times
foreach (var item in subtitleItems)
{
    Console.WriteLine($"Start: {item.StartTime}, End: {item.EndTime}");
}
```

## Configuration
### Parsing Options
```csharp
// Manually specify format for parsing
SubtitlesFormat format = SubtitlesFormat.SubRipFormat;
using var fileStream = new FileStream("path_to_subtitle.srt", FileMode.Open, FileAccess.Read);
var parser = new SubParser();
var subtitleItems = parser.ParseStream(fileStream, Encoding.UTF8, format);
```
### Error Handling
```csharp
try
{
    var subtitleItems = parser.ParseStream(fileStream, Encoding.UTF8, format);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error parsing subtitle: {ex.Message}");
}
```
### Logging
```csharp
ILogger logger = LoggerFactory
    .Create(builder => builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<SubtitleParser>();

// Example usage of logger in subtitle parsing
var parser = new SubParser(logger);
```

### Testing
To run the tests:
```bash
dotnet test
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## Support
For issues and feature requests, please use the [GitHub issues page](https://github.com/SpongeEngine/SubtitleSharp/issues).
