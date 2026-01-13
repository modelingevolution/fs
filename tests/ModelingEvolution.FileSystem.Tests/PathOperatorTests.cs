using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

/// <summary>
/// Tests specifically for the path arithmetic operators:
/// - Relative + Relative = Relative
/// - Absolute + Relative = Absolute
/// - Absolute - Absolute = Relative
/// </summary>
public class PathOperatorTests
{
    private static string RootPath => OperatingSystem.IsWindows() ? @"C:\" : "/";

    #region Relative + Relative = Relative

    [Fact]
    public void RelativePlusRelative_SimpleCase()
    {
        RelativePath left = "src";
        RelativePath right = "components";

        RelativePath result = left + right;

        result.Value.Should().Be(NormalizeSeparators("src/components"));
    }

    [Fact]
    public void RelativePlusRelative_MultipleSegments()
    {
        RelativePath left = "src/app";
        RelativePath right = "components/ui";

        RelativePath result = left + right;

        result.Value.Should().Be(NormalizeSeparators("src/app/components/ui"));
    }

    [Fact]
    public void RelativePlusRelative_ChainedAdditions()
    {
        RelativePath a = "a";
        RelativePath b = "b";
        RelativePath c = "c";

        RelativePath result = a + b + c;

        result.Value.Should().Be(NormalizeSeparators("a/b/c"));
    }

    #endregion

    #region Absolute + Relative = Absolute

    [Fact]
    public void AbsolutePlusRelative_SimpleCase()
    {
        AbsolutePath absolute = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";
        RelativePath relative = "myapp";

        AbsolutePath result = absolute + relative;

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\projects\myapp" : "/projects/myapp"));
    }

    [Fact]
    public void AbsolutePlusRelative_MultipleSegments()
    {
        AbsolutePath absolute = OperatingSystem.IsWindows() ? @"C:\users\john" : "/users/john";
        RelativePath relative = "documents/work";

        AbsolutePath result = absolute + relative;

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\users\john\documents\work" : "/users/john/documents/work"));
    }

    [Fact]
    public void AbsolutePlusRelative_ChainedAdditions()
    {
        AbsolutePath absolute = OperatingSystem.IsWindows() ? @"C:\root" : "/root";
        RelativePath rel1 = "level1";
        RelativePath rel2 = "level2";

        AbsolutePath result = absolute + rel1 + rel2;

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\root\level1\level2" : "/root/level1/level2"));
    }

    [Fact]
    public void AbsolutePlusString_RelativeString()
    {
        AbsolutePath absolute = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";

        AbsolutePath result = absolute + "src/main";

        result.Value.Should().Be(NormalizePath(OperatingSystem.IsWindows() ? @"C:\projects\src\main" : "/projects/src/main"));
    }

    #endregion

    #region Absolute - Absolute = Relative

    [Fact]
    public void AbsoluteMinusAbsolute_ChildPath()
    {
        AbsolutePath parent = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";
        AbsolutePath child = OperatingSystem.IsWindows() ? @"C:\projects\myapp\src" : "/projects/myapp/src";

        RelativePath result = child - parent;

        result.Value.Should().Be(NormalizeSeparators("myapp/src"));
    }

    [Fact]
    public void AbsoluteMinusAbsolute_SamePath()
    {
        AbsolutePath path = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";

        RelativePath result = path - path;

        result.Value.Should().Be(".");
    }

    [Fact]
    public void AbsoluteMinusAbsolute_SiblingPaths()
    {
        AbsolutePath from = OperatingSystem.IsWindows() ? @"C:\projects\app1" : "/projects/app1";
        AbsolutePath to = OperatingSystem.IsWindows() ? @"C:\projects\app2" : "/projects/app2";

        RelativePath result = to - from;

        result.Value.Should().Be(NormalizeSeparators("../app2"));
    }

    [Fact]
    public void AbsoluteMinusAbsolute_DeepTraversal()
    {
        AbsolutePath from = OperatingSystem.IsWindows() ? @"C:\a\b\c" : "/a/b/c";
        AbsolutePath to = OperatingSystem.IsWindows() ? @"C:\x\y\z" : "/x/y/z";

        RelativePath result = to - from;

        result.Value.Should().Be(NormalizeSeparators("../../../x/y/z"));
    }

    #endregion

    #region Combined Operations

    [Fact]
    public void RoundTrip_SubtractThenAdd()
    {
        AbsolutePath basePath = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";
        AbsolutePath targetPath = OperatingSystem.IsWindows() ? @"C:\projects\myapp\src" : "/projects/myapp/src";

        // Get relative path
        RelativePath relative = targetPath - basePath;

        // Apply it back
        AbsolutePath reconstructed = basePath + relative;

        reconstructed.Should().Be(targetPath);
    }

    [Fact]
    public void BuildPath_UsingAllOperators()
    {
        // Start with an absolute base
        AbsolutePath projects = OperatingSystem.IsWindows() ? @"C:\projects" : "/projects";

        // Add relative paths
        RelativePath appName = "myapp";
        RelativePath srcFolder = "src";
        RelativePath components = "components";

        // Build full path: projects + appName + srcFolder + components
        AbsolutePath fullPath = projects + appName + srcFolder + components;

        // Get relative path from projects
        RelativePath fromProjects = fullPath - projects;

        fromProjects.Value.Should().Be(NormalizeSeparators("myapp/src/components"));
    }

    #endregion

    private static string NormalizeSeparators(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);
}
