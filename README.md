# ModelingEvolution.FileSystem

[![NuGet](https://img.shields.io/nuget/v/ModelingEvolution.FileSystem.svg)](https://www.nuget.org/packages/ModelingEvolution.FileSystem/)

Strongly-typed file system path handling for .NET with intuitive path arithmetic operators.

## Features

- **Type-safe paths**: `AbsolutePath`, `RelativePath`, and `FileExtension` value types
- **Path arithmetic**: Intuitive operators for combining and computing paths
- **Cross-platform**: Automatically normalizes separators to current platform (`/` on Linux, `\` on Windows)
- **Pure value types**: No filesystem access - paths are just data
- **IParsable support**: Works with `Parse`, `TryParse`, and generic parsing APIs
- **JSON serialization**: Built-in `System.Text.Json` support via `JsonParsableConverter`

## Installation

```bash
dotnet add package ModelingEvolution.FileSystem
```

## Quick Start

```csharp
using ModelingEvolution.FileSystem;

// Create paths (implicit conversion from string)
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
RelativePath path = "foo\\bar/baz";
Console.WriteLine((string)path);  // foo/bar/baz

// On Windows - all separators become \
RelativePath path = "foo\\bar/baz";
Console.WriteLine((string)path);  // foo\bar\baz
```

Mixed and duplicate separators are handled:
```csharp
RelativePath path = "foo//bar\\\\baz";
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
    public bool IsEmpty { get; }
    public RelativePath FileName { get; }
    public RelativePath FileNameWithoutExtension { get; }
    public FileExtension Extension { get; }
    public RelativePath Parent { get; }
    public string[] Segments { get; }

    // Methods
    public RelativePath ChangeExtension(FileExtension newExtension);

    // Operators
    public static RelativePath operator +(RelativePath left, RelativePath right);
    public static RelativePath operator +(RelativePath left, string right);

    // Conversions (use these instead of .Value)
    public static implicit operator RelativePath(string path);
    public static implicit operator string(RelativePath path);
}
```

### AbsolutePath

```csharp
public readonly record struct AbsolutePath : IParsable<AbsolutePath>, IComparable<AbsolutePath>
{
    // Construction
    public AbsolutePath(string path);

    // Properties
    public RelativePath FileName { get; }
    public RelativePath FileNameWithoutExtension { get; }
    public FileExtension Extension { get; }
    public AbsolutePath? Root { get; }
    public AbsolutePath? Parent { get; }

    // Methods
    public AbsolutePath ChangeExtension(FileExtension newExtension);
    public bool StartsWith(AbsolutePath basePath);

    // Operators
    public static AbsolutePath operator +(AbsolutePath left, RelativePath right);
    public static AbsolutePath operator +(AbsolutePath left, string right);
    public static RelativePath operator -(AbsolutePath right, AbsolutePath left);

    // Conversions (use these instead of .Value)
    public static implicit operator AbsolutePath(string path);
    public static implicit operator string(AbsolutePath path);
}
```

### FileExtension

```csharp
public readonly record struct FileExtension : IParsable<FileExtension>, IComparable<FileExtension>
{
    // Construction
    public FileExtension(string extension);
    public static readonly FileExtension None;

    // Properties
    public bool IsEmpty { get; }
    public string WithDot { get; }      // ".txt"
    public string WithoutDot { get; }   // "txt"

    // Common extensions
    public static FileExtension Txt { get; }
    public static FileExtension Json { get; }
    public static FileExtension Cs { get; }
    public static FileExtension Md { get; }
    // ... and more

    // Methods
    public bool IsOneOf(params FileExtension[] extensions);
    public bool IsOneOf(params string[] extensions);

    // Conversions
    public static implicit operator FileExtension(string extension);
    public static implicit operator string(FileExtension extension);
}
```

## JSON Serialization

All types serialize as strings:

```csharp
using System.Text.Json;

var config = new AppConfig
{
    ProjectRoot = (AbsolutePath)"/projects/myapp",
    SourceFolder = (RelativePath)"src/main",
    OutputExtension = FileExtension.Json
};

string json = JsonSerializer.Serialize(config);
// {"ProjectRoot":"/projects/myapp","SourceFolder":"src/main","OutputExtension":".json"}
```

## Use Cases

### Building file paths safely

```csharp
AbsolutePath outputDir = (AbsolutePath)config.BuildOutput;
RelativePath artifactPath = (RelativePath)"bin" + config.Configuration + config.TargetFramework;
AbsolutePath fullOutput = outputDir + artifactPath;
```

### Working with extensions

```csharp
RelativePath file = "document.txt";
if (file.Extension.IsOneOf(".txt", ".md", ".rst"))
{
    // Handle text files
    var newFile = file.ChangeExtension(".html");
}
```

### Computing relative paths for links

```csharp
AbsolutePath docFile = (AbsolutePath)"/docs/api/classes/MyClass.md";
AbsolutePath imageFile = (AbsolutePath)"/docs/images/diagram.png";
RelativePath relativeLink = imageFile - docFile.Parent!.Value;
// Result: ../../images/diagram.png
```

## License

MIT
