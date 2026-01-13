using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class RelativePathTests
{
    [Fact]
    public void Constructor_WithValidRelativePath_CreatesInstance()
    {
        var path = new RelativePath("foo/bar");

        path.Value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    [Fact]
    public void Constructor_WithAbsolutePath_ThrowsArgumentException()
    {
        var absolutePath = OperatingSystem.IsWindows() ? @"C:\foo" : "/foo";

        var act = () => new RelativePath(absolutePath);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Empty_IsEmptyPath()
    {
        RelativePath.Empty.IsEmpty.Should().BeTrue();
        RelativePath.Empty.Value.Should().BeEmpty();
    }

    [Fact]
    public void FileName_ReturnsLastSegment()
    {
        var path = new RelativePath("foo/bar/baz.txt");

        path.FileName.Should().Be("baz.txt");
    }

    [Fact]
    public void Extension_ReturnsFileExtension()
    {
        var path = new RelativePath("foo/bar.txt");

        path.Extension.Should().Be(".txt");
    }

    [Fact]
    public void Parent_ReturnsParentDirectory()
    {
        var path = new RelativePath("foo/bar/baz");

        path.Parent.Value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    [Fact]
    public void Parent_WhenSingleSegment_ReturnsEmpty()
    {
        var path = new RelativePath("foo");

        path.Parent.Should().Be(RelativePath.Empty);
    }

    [Fact]
    public void Segments_ReturnsAllPathParts()
    {
        var path = new RelativePath("foo/bar/baz");

        path.Segments.Should().BeEquivalentTo(["foo", "bar", "baz"]);
    }

    #region Operators

    [Fact]
    public void Addition_TwoRelativePaths_CombinesThem()
    {
        var left = new RelativePath("foo/bar");
        var right = new RelativePath("baz/qux");

        var result = left + right;

        result.Value.Should().Be(NormalizeSeparators("foo/bar/baz/qux"));
    }

    [Fact]
    public void Addition_RelativePathAndString_CombinesThem()
    {
        var left = new RelativePath("foo");
        var right = "bar/baz";

        var result = left + right;

        result.Value.Should().Be(NormalizeSeparators("foo/bar/baz"));
    }

    [Fact]
    public void Addition_WithEmptyLeft_ReturnsRight()
    {
        var result = RelativePath.Empty + new RelativePath("foo");

        result.Value.Should().Be("foo");
    }

    [Fact]
    public void Addition_WithEmptyRight_ReturnsLeft()
    {
        var left = new RelativePath("foo");

        var result = left + RelativePath.Empty;

        result.Value.Should().Be("foo");
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitConversion_FromString_CreatesRelativePath()
    {
        RelativePath path = "foo/bar";

        path.Value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var path = new RelativePath("foo/bar");

        string value = path;

        value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    #endregion

    #region Parsing

    [Fact]
    public void Parse_ValidRelativePath_ReturnsInstance()
    {
        var result = RelativePath.Parse("foo/bar");

        result.Value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    [Fact]
    public void Parse_AbsolutePath_ThrowsFormatException()
    {
        var absolutePath = OperatingSystem.IsWindows() ? @"C:\foo" : "/foo";

        var act = () => RelativePath.Parse(absolutePath);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryParse_ValidPath_ReturnsTrue()
    {
        var success = RelativePath.TryParse("foo/bar", null, out var result);

        success.Should().BeTrue();
        result.Value.Should().Be(NormalizeSeparators("foo/bar"));
    }

    [Fact]
    public void TryParse_AbsolutePath_ReturnsFalse()
    {
        var absolutePath = OperatingSystem.IsWindows() ? @"C:\foo" : "/foo";

        var success = RelativePath.TryParse(absolutePath, null, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        var success = RelativePath.TryParse(null, null, out _);

        success.Should().BeFalse();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SamePath_ReturnsTrue()
    {
        var path1 = new RelativePath("foo/bar");
        var path2 = new RelativePath("foo/bar");

        path1.Should().Be(path2);
    }

    [Fact]
    public void Equals_DifferentPath_ReturnsFalse()
    {
        var path1 = new RelativePath("foo/bar");
        var path2 = new RelativePath("foo/baz");

        path1.Should().NotBe(path2);
    }

    #endregion

    private static string NormalizeSeparators(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);
}
