using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents a relative file system path.
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<RelativePath>))]
public readonly record struct RelativePath : IParsable<RelativePath>, IComparable<RelativePath>
{
    /// <summary>
    /// Represents an empty relative path.
    /// </summary>
    public static readonly RelativePath Empty = new(string.Empty);

    private readonly string _value;

    /// <summary>
    /// Initializes a new instance of RelativePath.
    /// </summary>
    /// <param name="path">The relative path string.</param>
    /// <exception cref="ArgumentException">Thrown when the path is rooted (absolute).</exception>
    public RelativePath(string path)
    {
        if (Path.IsPathRooted(path))
            throw new ArgumentException($"Path '{path}' is absolute, not relative.", nameof(path));

        _value = NormalizePath(path);
    }

    /// <summary>
    /// Gets the path value.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets whether this path is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Gets the file name portion of the path.
    /// </summary>
    public string FileName => Path.GetFileName(Value);

    /// <summary>
    /// Gets the file extension including the dot.
    /// </summary>
    public string Extension => Path.GetExtension(Value);

    /// <summary>
    /// Gets the parent directory as a RelativePath.
    /// </summary>
    public RelativePath Parent
    {
        get
        {
            var dir = Path.GetDirectoryName(Value);
            return string.IsNullOrEmpty(dir) ? Empty : new RelativePath(dir);
        }
    }

    /// <summary>
    /// Gets the path segments.
    /// </summary>
    public string[] Segments => Value.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        .Where(s => !string.IsNullOrEmpty(s))
        .ToArray();

    #region Operators

    /// <summary>
    /// Combines two relative paths.
    /// </summary>
    public static RelativePath operator +(RelativePath left, RelativePath right)
    {
        if (left.IsEmpty) return right;
        if (right.IsEmpty) return left;
        return new RelativePath(Path.Combine(left.Value, right.Value));
    }

    /// <summary>
    /// Combines a relative path with a string path.
    /// </summary>
    public static RelativePath operator +(RelativePath left, string right)
    {
        if (string.IsNullOrEmpty(right)) return left;
        return left + new RelativePath(right);
    }

    public static bool operator <(RelativePath left, RelativePath right) => left.CompareTo(right) < 0;
    public static bool operator <=(RelativePath left, RelativePath right) => left.CompareTo(right) <= 0;
    public static bool operator >(RelativePath left, RelativePath right) => left.CompareTo(right) > 0;
    public static bool operator >=(RelativePath left, RelativePath right) => left.CompareTo(right) >= 0;

    #endregion

    #region Conversions

    public static implicit operator RelativePath(string path) => new(path);
    public static implicit operator string(RelativePath path) => path.Value;

    #endregion

    #region IParsable

    public static RelativePath Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;
        throw new FormatException($"Unable to parse '{s}' as RelativePath.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out RelativePath result)
    {
        result = Empty;
        if (s is null)
            return false;

        if (Path.IsPathRooted(s))
            return false;

        result = new RelativePath(s);
        return true;
    }

    #endregion

    public int CompareTo(RelativePath other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Normalize all directory separators to current platform's separator
        // This handles both Windows (\) and Unix (/) separators regardless of platform
        path = path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);

        // Remove duplicate separators
        var sep = Path.DirectorySeparatorChar;
        var doubleSep = $"{sep}{sep}";
        while (path.Contains(doubleSep))
            path = path.Replace(doubleSep, sep.ToString());

        // Remove trailing separator
        return path.TrimEnd(Path.DirectorySeparatorChar);
    }
}
