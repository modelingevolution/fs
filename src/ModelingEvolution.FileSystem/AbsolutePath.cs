using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents an absolute (rooted) file system path.
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<AbsolutePath>))]
public readonly record struct AbsolutePath : IParsable<AbsolutePath>, IComparable<AbsolutePath>
{
    private readonly string _value;

    /// <summary>
    /// Initializes a new instance of AbsolutePath.
    /// </summary>
    /// <param name="path">The absolute path string.</param>
    /// <exception cref="ArgumentException">Thrown when the path is not rooted (not absolute).</exception>
    public AbsolutePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!Path.IsPathRooted(path))
            throw new ArgumentException($"Path '{path}' is not absolute (not rooted).", nameof(path));

        _value = NormalizePath(path);
    }

    /// <summary>
    /// Gets the path value.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets the file name portion of the path.
    /// </summary>
    public string FileName => Path.GetFileName(Value);

    /// <summary>
    /// Gets the file extension including the dot.
    /// </summary>
    public string Extension => Path.GetExtension(Value);

    /// <summary>
    /// Gets the root of the path (e.g., "C:\" on Windows or "/" on Unix).
    /// </summary>
    public string Root => Path.GetPathRoot(Value) ?? string.Empty;

    /// <summary>
    /// Gets the parent directory as an AbsolutePath.
    /// </summary>
    public AbsolutePath? Parent
    {
        get
        {
            var dir = Path.GetDirectoryName(Value);
            if (string.IsNullOrEmpty(dir))
                return null;
            return new AbsolutePath(dir);
        }
    }

    /// <summary>
    /// Checks if the file or directory exists.
    /// </summary>
    public bool Exists => File.Exists(Value) || Directory.Exists(Value);

    /// <summary>
    /// Checks if this is a file path.
    /// </summary>
    public bool IsFile => File.Exists(Value);

    /// <summary>
    /// Checks if this is a directory path.
    /// </summary>
    public bool IsDirectory => Directory.Exists(Value);

    #region Operators

    /// <summary>
    /// Combines an absolute path with a relative path.
    /// </summary>
    public static AbsolutePath operator +(AbsolutePath left, RelativePath right)
    {
        if (right.IsEmpty) return left;
        return new AbsolutePath(Path.Combine(left.Value, right.Value));
    }

    /// <summary>
    /// Combines an absolute path with a string (relative path).
    /// </summary>
    public static AbsolutePath operator +(AbsolutePath left, string right)
    {
        if (string.IsNullOrEmpty(right)) return left;

        // If right is absolute, it replaces left (Path.Combine behavior)
        if (Path.IsPathRooted(right))
            return new AbsolutePath(right);

        return left + new RelativePath(right);
    }

    /// <summary>
    /// Gets the relative path from one absolute path to another.
    /// Result represents how to get from left to right.
    /// </summary>
    public static RelativePath operator -(AbsolutePath right, AbsolutePath left)
    {
        var relativePath = Path.GetRelativePath(left.Value, right.Value);
        return new RelativePath(relativePath);
    }

    public static bool operator <(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) < 0;
    public static bool operator <=(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) <= 0;
    public static bool operator >(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) > 0;
    public static bool operator >=(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) >= 0;

    #endregion

    #region Conversions

    public static implicit operator AbsolutePath(string path) => new(path);
    public static implicit operator string(AbsolutePath path) => path.Value;

    #endregion

    #region IParsable

    public static AbsolutePath Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;
        throw new FormatException($"Unable to parse '{s}' as AbsolutePath.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out AbsolutePath result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        if (!Path.IsPathRooted(s))
            return false;

        result = new AbsolutePath(s);
        return true;
    }

    #endregion

    /// <summary>
    /// Checks if this path starts with (is contained within) the specified base path.
    /// </summary>
    public bool StartsWith(AbsolutePath basePath)
    {
        var normalizedBase = basePath.Value.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedThis = Value.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedThis.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the current working directory as an AbsolutePath.
    /// </summary>
    public static AbsolutePath CurrentDirectory => new(Directory.GetCurrentDirectory());

    public int CompareTo(AbsolutePath other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;

    private static string NormalizePath(string path)
    {
        // Normalize separators before GetFullPath for consistent behavior
        // This handles both Windows (\) and Unix (/) separators regardless of platform
        path = path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);

        // Get the full path to resolve . and .. segments
        var fullPath = Path.GetFullPath(path);

        // Normalize directory separators (GetFullPath may reintroduce platform separators)
        fullPath = fullPath.Replace('\\', Path.DirectorySeparatorChar)
                           .Replace('/', Path.DirectorySeparatorChar);

        // Remove trailing separator (except for root paths like "C:\" or "/")
        if (fullPath.Length > Path.GetPathRoot(fullPath)?.Length)
            fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar);

        return fullPath;
    }
}
