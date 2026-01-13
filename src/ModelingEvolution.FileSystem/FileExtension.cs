using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents a file extension (e.g., ".txt", ".cs").
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<FileExtension>))]
public readonly record struct FileExtension : IParsable<FileExtension>, IComparable<FileExtension>
{
    /// <summary>
    /// Represents no extension (empty).
    /// </summary>
    public static readonly FileExtension None = new(string.Empty);

    private readonly string _value;

    /// <summary>
    /// Initializes a new instance of FileExtension.
    /// </summary>
    /// <param name="extension">The extension string (with or without leading dot).</param>
    public FileExtension(string extension)
    {
        _value = Normalize(extension);
    }

    /// <summary>
    /// Gets whether this extension is empty/none.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Gets the extension with the leading dot (e.g., ".txt").
    /// Returns empty string if no extension.
    /// </summary>
    public string WithDot => _value ?? string.Empty;

    /// <summary>
    /// Gets the extension without the leading dot (e.g., "txt").
    /// Returns empty string if no extension.
    /// </summary>
    public string WithoutDot => string.IsNullOrEmpty(_value) ? string.Empty : _value[1..];

    #region Common Extensions

    public static FileExtension Txt => new(".txt");
    public static FileExtension Json => new(".json");
    public static FileExtension Xml => new(".xml");
    public static FileExtension Cs => new(".cs");
    public static FileExtension Js => new(".js");
    public static FileExtension Ts => new(".ts");
    public static FileExtension Html => new(".html");
    public static FileExtension Css => new(".css");
    public static FileExtension Md => new(".md");
    public static FileExtension Yaml => new(".yaml");
    public static FileExtension Yml => new(".yml");
    public static FileExtension Png => new(".png");
    public static FileExtension Jpg => new(".jpg");
    public static FileExtension Gif => new(".gif");
    public static FileExtension Pdf => new(".pdf");
    public static FileExtension Zip => new(".zip");
    public static FileExtension Exe => new(".exe");
    public static FileExtension Dll => new(".dll");

    #endregion

    #region Operators

    public static bool operator ==(FileExtension left, string right) =>
        string.Equals(left._value, Normalize(right), StringComparison.OrdinalIgnoreCase);

    public static bool operator !=(FileExtension left, string right) => !(left == right);

    public static bool operator <(FileExtension left, FileExtension right) => left.CompareTo(right) < 0;
    public static bool operator <=(FileExtension left, FileExtension right) => left.CompareTo(right) <= 0;
    public static bool operator >(FileExtension left, FileExtension right) => left.CompareTo(right) > 0;
    public static bool operator >=(FileExtension left, FileExtension right) => left.CompareTo(right) >= 0;

    #endregion

    #region Conversions

    public static implicit operator FileExtension(string extension) => new(extension);
    public static implicit operator string(FileExtension extension) => extension.WithDot;

    #endregion

    #region IParsable

    public static FileExtension Parse(string s, IFormatProvider? provider = null)
    {
        return new FileExtension(s);
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out FileExtension result)
    {
        result = s is null ? None : new FileExtension(s);
        return true;
    }

    #endregion

    /// <summary>
    /// Checks if this extension matches any of the specified extensions (case-insensitive).
    /// </summary>
    public bool IsOneOf(params FileExtension[] extensions)
    {
        foreach (var ext in extensions)
        {
            if (Equals(ext))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if this extension matches any of the specified extension strings (case-insensitive).
    /// </summary>
    public bool IsOneOf(params string[] extensions)
    {
        foreach (var ext in extensions)
        {
            if (this == ext)
                return true;
        }
        return false;
    }

    public int CompareTo(FileExtension other) =>
        string.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);

    public bool Equals(FileExtension other) =>
        string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(_value ?? string.Empty);

    public override string ToString() => WithDot;

    private static string Normalize(string? extension)
    {
        if (string.IsNullOrEmpty(extension))
            return string.Empty;

        extension = extension.Trim();

        if (string.IsNullOrEmpty(extension))
            return string.Empty;

        // Ensure leading dot
        if (!extension.StartsWith('.'))
            extension = "." + extension;

        // Convert to lowercase for consistency
        return extension.ToLowerInvariant();
    }
}
