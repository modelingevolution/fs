using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents a SHA-256 hash value (32 bytes / 256 bits).
/// </summary>
[JsonConverter(typeof(JsonParsableConverter<Sha256>))]
public readonly record struct Sha256 : IParsable<Sha256>
{
    private readonly byte[] _value;

    public Sha256(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length != 32)
            throw new ArgumentException("SHA-256 hash must be 32 bytes.", nameof(value));
        _value = value;
    }

    public ReadOnlySpan<byte> Bytes => _value ?? [];

    public bool SequenceEqual(byte[] other) => Bytes.SequenceEqual(other);

    public static implicit operator byte[](Sha256 sha) => sha._value ?? [];

    public bool Equals(Sha256 other) => Bytes.SequenceEqual(other.Bytes);
    public override int GetHashCode() => _value is { Length: >= 4 } ? BitConverter.ToInt32(_value, 0) : 0;

    public static Sha256 Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;
        throw new FormatException($"Unable to parse '{s}' as Sha256.");
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Sha256 result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s) || s.Length != 64)
            return false;

        try
        {
            var bytes = Convert.FromHexString(s);
            result = new Sha256(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => _value != null ? Convert.ToHexString(_value).ToLowerInvariant() : string.Empty;
}
