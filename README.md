# ModelingEvolution.FileSystem

[![NuGet](https://img.shields.io/nuget/v/ModelingEvolution.FileSystem.svg)](https://www.nuget.org/packages/ModelingEvolution.FileSystem/)

Strongly-typed file system path handling for .NET with intuitive path arithmetic operators.

## Features

- **Type-safe paths**: `AbsolutePath` and `RelativePath` value types prevent mixing path types
- **Path arithmetic**: Intuitive operators for combining and computing paths
- **Cross-platform**: Automatically normalizes separators to current platform (`/` on Linux, `\` on Windows)
- **IParsable support**: Works with `Parse`, `TryParse`, and generic parsing APIs
- **JSON serialization**: Built-in `System.Text.Json` support via `JsonParsableConverter`

## Installation

```bash
dotnet add package ModelingEvolution.FileSystem
```

## Quick Start

```csharp
using ModelingEvolution.FileSystem;

// Create paths
AbsolutePath projectRoot = "/home/user/projects/myapp";  // Linux
AbsolutePath projectRoot = @"C:\Projects\MyApp";         // Windows

RelativePath srcFolder = "src/components";

// Combine paths with + operator
AbsolutePath fullPath = projectRoot + srcFolder;
// Linux:   /home/user/projects/myapp/src/components
// Windows: C:\Projects\MyApp\src\components

// Get relative path between absolutes with - operator
RelativePath relative = fullPath - projectRoot;
// Result: src/components (or src\components on Windows)
```

## Path Arithmetic Operators

The library provides three intuitive operators:

| Expression | Result | Description |
|------------|--------|-------------|
| `Relative + Relative` | `RelativePath` | Combine two relative paths |
| `Absolute + Relative` | `AbsolutePath` | Append relative path to absolute |
| `Absolute - Absolute` | `RelativePath` | Get relative path between two absolutes |

### Examples

```csharp
// Relative + Relative = Relative
RelativePath src = "src";
RelativePath components = "components";
RelativePath combined = src + components;  // src/components

// Absolute + Relative = Absolute
AbsolutePath root = "/projects";
AbsolutePath full = root + combined;  // /projects/src/components

// Absolute - Absolute = Relative
AbsolutePath from = "/projects/app";
AbsolutePath to = "/projects/app/src/utils";
RelativePath diff = to - from;  // src/utils

// Works with parent traversal
AbsolutePath sibling = "/projects/other";
RelativePath toSibling = sibling - from;  // ../other
```

## Cross-Platform Path Normalization

Paths are automatically normalized to the current platform's separator:

```csharp
// On Linux - all separators become /
var path = new RelativePath("foo\\bar/baz");
Console.WriteLine(path.Value);  // foo/bar/baz

// On Windows - all separators become \
var path = new RelativePath("foo\\bar/baz");
Console.WriteLine(path.Value);  // foo\bar\baz
```

Mixed and duplicate separators are handled:
```csharp
var path = new RelativePath("foo//bar\\\\baz");
// Normalized to: foo/bar/baz (Linux) or foo\bar\baz (Windows)
```

## API Reference

### RelativePath

```csharp
public readonly record struct RelativePath : IParsable<RelativePath>, IComparable<RelativePath>
{
    // Construction
    public RelativePath(string path);
    public static readonly RelativePath Empty;

    // Properties
    public string Value { get; }
    public bool IsEmpty { get; }
    public string FileName { get; }
    public string Extension { get; }
    public RelativePath Parent { get; }
    public string[] Segments { get; }

    // Operators
    public static RelativePath operator +(RelativePath left, RelativePath right);
    public static RelativePath operator +(RelativePath left, string right);

    // Conversions
    public static implicit operator RelativePath(string path);
    public static implicit operator string(RelativePath path);

    // Parsing
    public static RelativePath Parse(string s, IFormatProvider? provider = null);
    public static bool TryParse(string? s, IFormatProvider? provider, out RelativePath result);
}
```

### AbsolutePath

```csharp
public readonly record struct AbsolutePath : IParsable<AbsolutePath>, IComparable<AbsolutePath>
{
    // Construction
    public AbsolutePath(string path);
    public static AbsolutePath CurrentDirectory { get; }

    // Properties
    public string Value { get; }
    public string FileName { get; }
    public string Extension { get; }
    public string Root { get; }
    public AbsolutePath? Parent { get; }
    public bool Exists { get; }
    public bool IsFile { get; }
    public bool IsDirectory { get; }

    // Operators
    public static AbsolutePath operator +(AbsolutePath left, RelativePath right);
    public static AbsolutePath operator +(AbsolutePath left, string right);
    public static RelativePath operator -(AbsolutePath right, AbsolutePath left);

    // Conversions
    public static implicit operator AbsolutePath(string path);
    public static implicit operator string(AbsolutePath path);

    // Methods
    public bool StartsWith(AbsolutePath basePath);

    // Parsing
    public static AbsolutePath Parse(string s, IFormatProvider? provider = null);
    public static bool TryParse(string? s, IFormatProvider? provider, out AbsolutePath result);
}
```

## JSON Serialization

Both types serialize as strings using `JsonParsableConverter`:

```csharp
using System.Text.Json;

var config = new AppConfig
{
    ProjectRoot = new AbsolutePath("/projects/myapp"),
    SourceFolder = new RelativePath("src/main")
};

string json = JsonSerializer.Serialize(config);
// {"ProjectRoot":"/projects/myapp","SourceFolder":"src/main"}

var restored = JsonSerializer.Deserialize<AppConfig>(json);
```

## Use Cases

### Building file paths safely

```csharp
AbsolutePath outputDir = config.BuildOutput;
RelativePath artifactPath = "bin" + config.Configuration + config.TargetFramework;
AbsolutePath fullOutput = outputDir + artifactPath;
```

### Computing relative paths for links

```csharp
AbsolutePath docFile = "/docs/api/classes/MyClass.md";
AbsolutePath imageFile = "/docs/images/diagram.png";
RelativePath relativeLink = imageFile - docFile.Parent!.Value;
// Result: ../../images/diagram.png
```

### Working with project structures

```csharp
AbsolutePath solution = AbsolutePath.CurrentDirectory;
RelativePath testProject = "tests/MyProject.Tests";
AbsolutePath testDir = solution + testProject;

if (testDir.IsDirectory)
{
    // Run tests...
}
```

## License

MIT
