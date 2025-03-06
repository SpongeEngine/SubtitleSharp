# Contributing to LocalAI.NET

## Publishing to NuGet

### Prerequisites
1. Create a NuGet account at https://www.nuget.org
2. Generate an API key:
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create"
   - Name: "SpongeEngine.SubtitleSharp" (or your preferred name)
   - Expiration: 365 days
   - Select "Push new packages and package versions"
   - Glob Pattern: "SpongeEngine.SubtitleSharp*"
   - Save the generated key securely

### Publishing Process
1. Update version in `SpongeEngine.SubtitleSharp/SpongeEngine.SubtitleSharp.csproj`:
   ```xml
   <Version>1.1.1</Version>   <!-- Change this to new version -->
   ```

2. Clean and pack:
   ```bash
   dotnet clean
   dotnet pack -c Release
   ```

3. Push to NuGet:
   ```bash
   dotnet nuget push .\SpongeEngine.SubtitleSharp\bin\Release\SpongeEngine.SubtitleSharp.1.1.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```
   Replace:
   - `1.1.1` with your new version number
   - `YOUR_API_KEY` with your NuGet API key

4. Wait 15-30 minutes for the package to appear on NuGet.org

### Version Guidelines
- Use [Semantic Versioning](https://semver.org/):
  - MAJOR version for incompatible API changes
  - MINOR version for backwards-compatible functionality
  - PATCH version for backwards-compatible bug fixes

## Development Guidelines

### Code Style
- Use C# latest features and best practices
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Add XML documentation comments for public APIs

### Testing
1. Write unit tests for new features
2. Ensure all tests pass before submitting PR:
   ```bash
   dotnet test
   ```

### Pull Request Process
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Update documentation and tests
5. Submit a pull request

## Questions?
Open an issue on GitHub if you have questions or need help.
