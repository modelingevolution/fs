using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class FileExtensionTests
{
    [Fact]
    public void Constructor_WithDot_PreservesDot()
    {
        var ext = new FileExtension(".txt");

        ext.WithDot.Should().Be(".txt");
        ext.WithoutDot.Should().Be("txt");
    }

    [Fact]
    public void Constructor_WithoutDot_AddsDot()
    {
        var ext = new FileExtension("txt");

        ext.WithDot.Should().Be(".txt");
        ext.WithoutDot.Should().Be("txt");
    }

    [Fact]
    public void Constructor_NormalizesToLowercase()
    {
        var ext = new FileExtension(".TXT");

        ext.WithDot.Should().Be(".txt");
    }

    [Fact]
    public void None_IsEmpty()
    {
        FileExtension.None.IsEmpty.Should().BeTrue();
        FileExtension.None.WithDot.Should().BeEmpty();
        FileExtension.None.WithoutDot.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmpty_ReturnsEmpty()
    {
        var ext = new FileExtension("");

        ext.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNull_ReturnsEmpty()
    {
        var ext = new FileExtension(null!);

        ext.IsEmpty.Should().BeTrue();
    }

    #region Common Extensions

    [Fact]
    public void CommonExtensions_HaveCorrectValues()
    {
        FileExtension.Txt.WithDot.Should().Be(".txt");
        FileExtension.Json.WithDot.Should().Be(".json");
        FileExtension.Cs.WithDot.Should().Be(".cs");
        FileExtension.Md.WithDot.Should().Be(".md");
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameExtension_ReturnsTrue()
    {
        var ext1 = new FileExtension(".txt");
        var ext2 = new FileExtension(".txt");

        ext1.Should().Be(ext2);
    }

    [Fact]
    public void Equals_DifferentCase_ReturnsTrue()
    {
        var ext1 = new FileExtension(".txt");
        var ext2 = new FileExtension(".TXT");

        ext1.Should().Be(ext2);
    }

    [Fact]
    public void Equals_DifferentExtension_ReturnsFalse()
    {
        var ext1 = new FileExtension(".txt");
        var ext2 = new FileExtension(".md");

        ext1.Should().NotBe(ext2);
    }

    [Fact]
    public void EqualsString_WithDot_ReturnsTrue()
    {
        var ext = new FileExtension(".txt");

        (ext == ".txt").Should().BeTrue();
        (ext == ".TXT").Should().BeTrue();
    }

    [Fact]
    public void EqualsString_WithoutDot_ReturnsTrue()
    {
        var ext = new FileExtension(".txt");

        (ext == "txt").Should().BeTrue();
    }

    #endregion

    #region IsOneOf

    [Fact]
    public void IsOneOf_Extensions_WhenMatches_ReturnsTrue()
    {
        var ext = new FileExtension(".txt");

        ext.IsOneOf(FileExtension.Txt, FileExtension.Md, FileExtension.Json).Should().BeTrue();
    }

    [Fact]
    public void IsOneOf_Extensions_WhenNoMatch_ReturnsFalse()
    {
        var ext = new FileExtension(".txt");

        ext.IsOneOf(FileExtension.Md, FileExtension.Json).Should().BeFalse();
    }

    [Fact]
    public void IsOneOf_Strings_WhenMatches_ReturnsTrue()
    {
        var ext = new FileExtension(".txt");

        ext.IsOneOf(".txt", ".md", ".json").Should().BeTrue();
        ext.IsOneOf("txt", "md").Should().BeTrue();
    }

    [Fact]
    public void IsOneOf_Strings_WhenNoMatch_ReturnsFalse()
    {
        var ext = new FileExtension(".txt");

        ext.IsOneOf(".md", ".json").Should().BeFalse();
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitConversion_FromString_CreatesExtension()
    {
        FileExtension ext = ".txt";

        ext.WithDot.Should().Be(".txt");
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsWithDot()
    {
        var ext = new FileExtension(".txt");

        string value = ext;

        value.Should().Be(".txt");
    }

    #endregion

    #region Parsing

    [Fact]
    public void Parse_ValidExtension_ReturnsInstance()
    {
        var ext = FileExtension.Parse(".txt");

        ext.WithDot.Should().Be(".txt");
    }

    [Fact]
    public void TryParse_ValidExtension_ReturnsTrue()
    {
        var success = FileExtension.TryParse(".txt", null, out var ext);

        success.Should().BeTrue();
        ext.WithDot.Should().Be(".txt");
    }

    [Fact]
    public void TryParse_Null_ReturnsTrueWithNone()
    {
        var success = FileExtension.TryParse(null, null, out var ext);

        success.Should().BeTrue();
        ext.Should().Be(FileExtension.None);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ReturnsWithDot()
    {
        var ext = new FileExtension("txt");

        ext.ToString().Should().Be(".txt");
    }

    #endregion
}
