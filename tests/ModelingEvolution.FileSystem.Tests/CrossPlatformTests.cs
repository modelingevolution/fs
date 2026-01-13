using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

/// <summary>
/// Tests for cross-platform path normalization.
/// Paths should be normalized to the current platform's separator regardless of input format.
/// </summary>
public class CrossPlatformTests
{
    private static char Sep => Path.DirectorySeparatorChar;

    #region RelativePath Normalization

    [Fact]
    public void RelativePath_WithForwardSlashes_NormalizesToPlatformSeparator()
    {
        var path = new RelativePath("foo/bar/baz");

        path.Value.Should().Be($"foo{Sep}bar{Sep}baz");
    }

    [Fact]
    public void RelativePath_WithBackslashes_NormalizesToPlatformSeparator()
    {
        var path = new RelativePath("foo\\bar\\baz");

        path.Value.Should().Be($"foo{Sep}bar{Sep}baz");
    }

    [Fact]
    public void RelativePath_WithMixedSlashes_NormalizesToPlatformSeparator()
    {
        var path = new RelativePath("foo/bar\\baz");

        path.Value.Should().Be($"foo{Sep}bar{Sep}baz");
    }

    [Fact]
    public void RelativePath_WithDuplicateSlashes_RemovesDuplicates()
    {
        var path = new RelativePath("foo//bar///baz");

        path.Value.Should().Be($"foo{Sep}bar{Sep}baz");
    }

    [Fact]
    public void RelativePath_WithMixedDuplicateSlashes_NormalizesCorrectly()
    {
        var path = new RelativePath("foo\\\\bar//baz");

        path.Value.Should().Be($"foo{Sep}bar{Sep}baz");
    }

    [Fact]
    public void RelativePath_WithTrailingSlash_RemovesTrailingSlash()
    {
        var path = new RelativePath("foo/bar/");

        path.Value.Should().Be($"foo{Sep}bar");
    }

    [Fact]
    public void RelativePath_Addition_PreservesPlatformSeparator()
    {
        var left = new RelativePath("foo/bar");
        var right = new RelativePath("baz\\qux");

        var result = left + right;

        result.Value.Should().Be($"foo{Sep}bar{Sep}baz{Sep}qux");
    }

    #endregion

    #region AbsolutePath Normalization

    [Fact]
    public void AbsolutePath_WithMixedSlashes_NormalizesToPlatformSeparator()
    {
        string inputPath;
        string expectedPath;

        if (OperatingSystem.IsWindows())
        {
            inputPath = @"C:/foo\bar/baz";
            expectedPath = @"C:\foo\bar\baz";
        }
        else
        {
            inputPath = "/foo/bar/baz";
            expectedPath = "/foo/bar/baz";
        }

        var path = new AbsolutePath(inputPath);

        path.Value.Should().Be(expectedPath);
    }

    [Fact]
    public void AbsolutePath_Addition_WithMixedSlashes_NormalizesCorrectly()
    {
        AbsolutePath basePath;
        string expectedPath;

        if (OperatingSystem.IsWindows())
        {
            basePath = new AbsolutePath(@"C:\projects");
            expectedPath = @"C:\projects\foo\bar\baz";
        }
        else
        {
            basePath = new AbsolutePath("/projects");
            expectedPath = "/projects/foo/bar/baz";
        }

        var result = basePath + new RelativePath("foo/bar\\baz");

        result.Value.Should().Be(expectedPath);
    }

    #endregion

    #region Segments

    [Fact]
    public void RelativePath_Segments_WorksWithMixedSlashes()
    {
        var path = new RelativePath("foo/bar\\baz");

        path.Segments.Should().BeEquivalentTo(["foo", "bar", "baz"]);
    }

    #endregion

    #region Parsing

    [Fact]
    public void RelativePath_Parse_WithMixedSlashes_Normalizes()
    {
        var path = RelativePath.Parse("src/components\\ui");

        path.Value.Should().Be($"src{Sep}components{Sep}ui");
    }

    [Fact]
    public void AbsolutePath_TryParse_WithMixedSlashes_Normalizes()
    {
        string input;
        string expected;

        if (OperatingSystem.IsWindows())
        {
            input = @"C:/foo\bar";
            expected = @"C:\foo\bar";
        }
        else
        {
            input = "/foo/bar";
            expected = "/foo/bar";
        }

        var success = AbsolutePath.TryParse(input, null, out var path);

        success.Should().BeTrue();
        path.Value.Should().Be(expected);
    }

    #endregion

    #region Operator Results

    [Fact]
    public void Subtraction_Result_UsesPlatformSeparator()
    {
        AbsolutePath from, to;

        if (OperatingSystem.IsWindows())
        {
            from = new AbsolutePath(@"C:\projects\app");
            to = new AbsolutePath(@"C:\projects\app\src\components");
        }
        else
        {
            from = new AbsolutePath("/projects/app");
            to = new AbsolutePath("/projects/app/src/components");
        }

        RelativePath result = to - from;

        result.Value.Should().Be($"src{Sep}components");
    }

    #endregion
}
