using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class AbsolutePathTests
{
    private static string TestRoot => OperatingSystem.IsWindows() ? @"C:\" : "/";
    private static string TestPath => OperatingSystem.IsWindows() ? @"C:\foo\bar" : "/foo/bar";
    private static string TestPath2 => OperatingSystem.IsWindows() ? @"C:\foo\bar\baz" : "/foo/bar/baz";

    [Fact]
    public void Constructor_WithValidAbsolutePath_CreatesInstance()
    {
        var path = new AbsolutePath(TestPath);

        path.Value.Should().Be(NormalizePath(TestPath));
    }

    [Fact]
    public void Constructor_WithRelativePath_ThrowsArgumentException()
    {
        var act = () => new AbsolutePath("foo/bar");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentException()
    {
        var act = () => new AbsolutePath(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FileName_ReturnsLastSegment()
    {
        var path = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\foo\bar.txt" : "/foo/bar.txt");

        path.FileName.Should().Be("bar.txt");
    }

    [Fact]
    public void Extension_ReturnsFileExtension()
    {
        var path = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\foo\bar.txt" : "/foo/bar.txt");

        path.Extension.Should().Be(".txt");
    }

    [Fact]
    public void Root_ReturnsRootPath()
    {
        var path = new AbsolutePath(TestPath);

        path.Root.Should().Be(TestRoot);
    }

    [Fact]
    public void Parent_ReturnsParentDirectory()
    {
        var path = new AbsolutePath(TestPath);

        path.Parent!.Value.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\foo" : "/foo"));
    }

    [Fact]
    public void Parent_WhenAtRoot_ReturnsNull()
    {
        var path = new AbsolutePath(TestRoot);

        path.Parent.Should().BeNull();
    }

    #region Operators

    [Fact]
    public void Addition_AbsoluteAndRelative_CombinesThem()
    {
        var absolute = new AbsolutePath(TestPath);
        var relative = new RelativePath("baz/qux");

        var result = absolute + relative;

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\foo\bar\baz\qux" : "/foo/bar/baz/qux"));
    }

    [Fact]
    public void Addition_AbsoluteAndString_CombinesThem()
    {
        var absolute = new AbsolutePath(TestPath);

        var result = absolute + "baz";

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\foo\bar\baz" : "/foo/bar/baz"));
    }

    [Fact]
    public void Addition_WithEmptyRelative_ReturnsOriginal()
    {
        var absolute = new AbsolutePath(TestPath);

        var result = absolute + RelativePath.Empty;

        result.Should().Be(absolute);
    }

    [Fact]
    public void Subtraction_TwoAbsolutePaths_ReturnsRelativePath()
    {
        var basePath = new AbsolutePath(TestPath);
        var fullPath = new AbsolutePath(TestPath2);

        RelativePath result = fullPath - basePath;

        result.Value.Should().Be("baz");
    }

    [Fact]
    public void Subtraction_SamePath_ReturnsCurrentDirectory()
    {
        var path = new AbsolutePath(TestPath);

        RelativePath result = path - path;

        result.Value.Should().Be(".");
    }

    [Fact]
    public void Subtraction_WithParentTraversal_ReturnsRelativeWithDotDot()
    {
        var path1 = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\foo\bar" : "/foo/bar");
        var path2 = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\foo\baz" : "/foo/baz");

        RelativePath result = path2 - path1;

        // Going from /foo/bar to /foo/baz requires going up one level and then to baz
        result.Value.Should().Be(NormalizeSeparators("../baz"));
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitConversion_FromString_CreatesAbsolutePath()
    {
        AbsolutePath path = TestPath;

        path.Value.Should().Be(NormalizePath(TestPath));
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var path = new AbsolutePath(TestPath);

        string value = path;

        value.Should().Be(NormalizePath(TestPath));
    }

    #endregion

    #region Parsing

    [Fact]
    public void Parse_ValidAbsolutePath_ReturnsInstance()
    {
        var result = AbsolutePath.Parse(TestPath);

        result.Value.Should().Be(NormalizePath(TestPath));
    }

    [Fact]
    public void Parse_RelativePath_ThrowsFormatException()
    {
        var act = () => AbsolutePath.Parse("foo/bar");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryParse_ValidPath_ReturnsTrue()
    {
        var success = AbsolutePath.TryParse(TestPath, null, out var result);

        success.Should().BeTrue();
        result.Value.Should().Be(NormalizePath(TestPath));
    }

    [Fact]
    public void TryParse_RelativePath_ReturnsFalse()
    {
        var success = AbsolutePath.TryParse("foo/bar", null, out _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        var success = AbsolutePath.TryParse(null, null, out _);

        success.Should().BeFalse();
    }

    #endregion

    #region StartsWith

    [Fact]
    public void StartsWith_WhenContained_ReturnsTrue()
    {
        var basePath = new AbsolutePath(TestPath);
        var fullPath = new AbsolutePath(TestPath2);

        fullPath.StartsWith(basePath).Should().BeTrue();
    }

    [Fact]
    public void StartsWith_WhenNotContained_ReturnsFalse()
    {
        var path1 = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\foo\bar" : "/foo/bar");
        var path2 = new AbsolutePath(OperatingSystem.IsWindows() ? @"C:\other" : "/other");

        path2.StartsWith(path1).Should().BeFalse();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SamePath_ReturnsTrue()
    {
        var path1 = new AbsolutePath(TestPath);
        var path2 = new AbsolutePath(TestPath);

        path1.Should().Be(path2);
    }

    [Fact]
    public void Equals_DifferentPath_ReturnsFalse()
    {
        var path1 = new AbsolutePath(TestPath);
        var path2 = new AbsolutePath(TestPath2);

        path1.Should().NotBe(path2);
    }

    #endregion

    #region CurrentDirectory

    [Fact]
    public void CurrentDirectory_ReturnsValidAbsolutePath()
    {
        var current = AbsolutePath.CurrentDirectory;

        current.Value.Should().NotBeEmpty();
        Path.IsPathRooted(current.Value).Should().BeTrue();
    }

    #endregion

    private static string NormalizeSeparators(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);
}
