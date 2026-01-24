using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class Sha1Tests
{
    private static readonly byte[] ValidHash = new byte[20] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
    private static readonly byte[] AnotherHash = new byte[20] { 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

    [Fact]
    public void Constructor_WithValidBytes_CreatesInstance()
    {
        var sha = new Sha1(ValidHash);

        sha.Bytes.ToArray().Should().BeEquivalentTo(ValidHash);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        var act = () => new Sha1(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithInvalidLength_ThrowsArgumentException()
    {
        var act = () => new Sha1(new byte[10]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*20 bytes*");
    }

    [Fact]
    public void Equals_SameBytes_ReturnsTrue()
    {
        var sha1 = new Sha1(ValidHash);
        var sha2 = new Sha1((byte[])ValidHash.Clone());

        sha1.Equals(sha2).Should().BeTrue();
        (sha1 == sha2).Should().BeTrue();
        (sha1 != sha2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentBytes_ReturnsFalse()
    {
        var sha1 = new Sha1(ValidHash);
        var sha2 = new Sha1(AnotherHash);

        sha1.Equals(sha2).Should().BeFalse();
        (sha1 == sha2).Should().BeFalse();
        (sha1 != sha2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameBytes_ReturnsSameValue()
    {
        var sha1 = new Sha1(ValidHash);
        var sha2 = new Sha1((byte[])ValidHash.Clone());

        sha1.GetHashCode().Should().Be(sha2.GetHashCode());
    }

    [Fact]
    public void SequenceEqual_MatchingArray_ReturnsTrue()
    {
        var sha = new Sha1(ValidHash);

        sha.SequenceEqual((byte[])ValidHash.Clone()).Should().BeTrue();
    }

    [Fact]
    public void SequenceEqual_DifferentArray_ReturnsFalse()
    {
        var sha = new Sha1(ValidHash);

        sha.SequenceEqual(AnotherHash).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToByteArray_ReturnsBytes()
    {
        var sha = new Sha1(ValidHash);

        byte[] bytes = sha;

        bytes.Should().BeEquivalentTo(ValidHash);
    }

    [Fact]
    public void ToString_ReturnsLowercaseHex()
    {
        var sha = new Sha1(ValidHash);

        var result = sha.ToString();

        result.Should().Be("0102030405060708090a0b0c0d0e0f1011121314");
    }

    [Fact]
    public void Parse_ValidHex_ReturnsInstance()
    {
        var sha = Sha1.Parse("0102030405060708090a0b0c0d0e0f1011121314");

        sha.Bytes.ToArray().Should().BeEquivalentTo(ValidHash);
    }

    [Fact]
    public void Parse_InvalidLength_ThrowsFormatException()
    {
        var act = () => Sha1.Parse("0102030405");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_InvalidChars_ThrowsFormatException()
    {
        var act = () => Sha1.Parse("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryParse_ValidHex_ReturnsTrue()
    {
        var success = Sha1.TryParse("0102030405060708090a0b0c0d0e0f1011121314", null, out var result);

        success.Should().BeTrue();
        result.Bytes.ToArray().Should().BeEquivalentTo(ValidHash);
    }

    [Fact]
    public void TryParse_InvalidLength_ReturnsFalse()
    {
        var success = Sha1.TryParse("0102030405", null, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        var success = Sha1.TryParse(null, null, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void Default_HasEmptyBytes()
    {
        var sha = default(Sha1);

        sha.Bytes.ToArray().Should().BeEmpty();
        sha.ToString().Should().BeEmpty();
    }
}
