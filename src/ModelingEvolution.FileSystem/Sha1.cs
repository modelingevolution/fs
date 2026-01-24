using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents a SHA-1 hash value (20 bytes / 160 bits).
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<Sha1>))]
public readonly record struct Sha1 : IParsable<Sha1>
{
    private readonly byte[] _value;

    public Sha1(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length != 20)
            throw new ArgumentException("SHA-1 hash must be 20 bytes.", nameof(value));
        _value = value;
    }

    public ReadOnlySpan<byte> Bytes => _value ?? [];

    public bool SequenceEqual(byte[] other) => Bytes.SequenceEqual(other);

    public static implicit operator byte[](Sha1 sha) => sha._value ?? [];

    public bool Equals(Sha1 other) => Bytes.SequenceEqual(other.Bytes);
    public override int GetHashCode() => _value is { Length: >= 4 } ? BitConverter.ToInt32(_value, 0) : 0;

    public static Sha1 Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;
        throw new FormatException($"Unable to parse '{s}' as Sha1.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Sha1 result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s) || s.Length != 40)
            return false;

        try
        {
            var bytes = Convert.FromHexString(s);
            result = new Sha1(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => _value != null ? Convert.ToHexString(_value).ToLowerInvariant() : string.Empty;
}
