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
    /// Gets the file name portion of the path as a RelativePath.
    /// </summary>
    public RelativePath FileName
    {
        get
        {
            var fileName = Path.GetFileName(_value ?? string.Empty);
            return string.IsNullOrEmpty(fileName) ? RelativePath.Empty : new RelativePath(fileName);
        }
    }

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public FileExtension Extension => new(Path.GetExtension(_value ?? string.Empty));

    /// <summary>
    /// Gets the root of the path (e.g., "C:\" on Windows or "/" on Unix).
    /// </summary>
    public AbsolutePath? Root
    {
        get
        {
            var root = Path.GetPathRoot(_value ?? string.Empty);
            if (string.IsNullOrEmpty(root))
                return null;
            return new AbsolutePath(root);
        }
    }

    /// <summary>
    /// Gets the parent directory as an AbsolutePath.
    /// </summary>
    public AbsolutePath? Parent
    {
        get
        {
            var dir = Path.GetDirectoryName(_value ?? string.Empty);
            if (string.IsNullOrEmpty(dir))
                return null;
            return new AbsolutePath(dir);
        }
    }

    /// <summary>
    /// Gets the file name without extension as a RelativePath.
    /// </summary>
    public RelativePath FileNameWithoutExtension
    {
        get
        {
            var name = Path.GetFileNameWithoutExtension(_value ?? string.Empty);
            return string.IsNullOrEmpty(name) ? RelativePath.Empty : new RelativePath(name);
        }
    }

    /// <summary>
    /// Changes the extension of this path.
    /// </summary>
    public AbsolutePath ChangeExtension(FileExtension newExtension)
    {
        var newPath = Path.ChangeExtension(_value ?? string.Empty, newExtension.WithDot);
        return new AbsolutePath(newPath);
    }

    #region Operators

    /// <summary>
    /// Combines an absolute path with a relative path.
    /// </summary>
    public static AbsolutePath operator +(AbsolutePath left, RelativePath right)
    {
        if (right.IsEmpty) return left;
        return new AbsolutePath(Path.Combine(left._value, (string)right));
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
    /// Appends a file extension to an absolute path.
    /// </summary>
    public static AbsolutePath operator +(AbsolutePath left, FileExtension right)
    {
        if (right.IsEmpty) return left;
        var newPath = (left._value ?? string.Empty) + right.WithDot;
        return new AbsolutePath(newPath);
    }

    /// <summary>
    /// Gets the relative path from one absolute path to another.
    /// Result represents how to get from left to right.
    /// </summary>
    public static RelativePath operator -(AbsolutePath right, AbsolutePath left)
    {
        var relativePath = Path.GetRelativePath(left._value, right._value);
        return new RelativePath(relativePath);
    }

    public static bool operator <(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) < 0;
    public static bool operator <=(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) <= 0;
    public static bool operator >(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) > 0;
    public static bool operator >=(AbsolutePath left, AbsolutePath right) => left.CompareTo(right) >= 0;

    #endregion

    #region Conversions

    public static implicit operator AbsolutePath(string path) => new(path);
    public static implicit operator string(AbsolutePath path) => path._value ?? string.Empty;

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
        var normalizedBase = ((string)basePath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedThis = (_value ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedThis.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
    }

    public int CompareTo(AbsolutePath other) =>
        string.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => _value ?? string.Empty;

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
