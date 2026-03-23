using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class PathExpanderTests
{
    [Fact]
    public void Expand_TildeOnly_ReturnsHomeDirectory()
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var result = PathExpander.Expand("~");
        result.Should().Be(home);
    }

    [Fact]
    public void Expand_TildeSlashPath_ExpandsToHomeSubpath()
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var result = PathExpander.Expand("~/.claude/.credentials.json");
        result.Should().Be($"{home}/.claude/.credentials.json");
    }

    [Fact]
    public void Expand_EnvVarBraces_Expanded()
    {
        Environment.SetEnvironmentVariable("TEST_EXPAND_DIR", "/tmp/test-expand");
        try
        {
            var result = PathExpander.Expand("${TEST_EXPAND_DIR}/data");
            result.Should().Be("/tmp/test-expand/data");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_EXPAND_DIR", null);
        }
    }

    [Fact]
    public void Expand_EnvVarDollarSign_Expanded()
    {
        Environment.SetEnvironmentVariable("TEST_EXPAND_NAME", "myapp");
        try
        {
            var result = PathExpander.Expand("$TEST_EXPAND_NAME/bin");
            result.Should().Be("myapp/bin");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_EXPAND_NAME", null);
        }
    }

    [Fact]
    public void Expand_MultipleEnvVars_AllExpanded()
    {
        Environment.SetEnvironmentVariable("TEST_BASE", "/opt");
        Environment.SetEnvironmentVariable("TEST_APP", "myapp");
        try
        {
            var result = PathExpander.Expand("${TEST_BASE}/$TEST_APP/config");
            result.Should().Be("/opt/myapp/config");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_BASE", null);
            Environment.SetEnvironmentVariable("TEST_APP", null);
        }
    }

    [Fact]
    public void Expand_TildeAndEnvVar_BothExpanded()
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Environment.SetEnvironmentVariable("TEST_SUBDIR", ".claude");
        try
        {
            var result = PathExpander.Expand("~/${TEST_SUBDIR}/credentials.json");
            result.Should().Be($"{home}/.claude/credentials.json");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_SUBDIR", null);
        }
    }

    [Fact]
    public void Expand_NoSpecialChars_ReturnsUnchanged()
    {
        var result = PathExpander.Expand("/usr/local/bin");
        result.Should().Be("/usr/local/bin");
    }

    [Fact]
    public void Expand_EmptyString_ReturnsEmpty()
    {
        var result = PathExpander.Expand("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Expand_MissingEnvVar_Throws()
    {
        Environment.SetEnvironmentVariable("TEST_MISSING_VAR_CHECK", null);

        var act = () => PathExpander.Expand("${TEST_MISSING_VAR_CHECK}/data");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TEST_MISSING_VAR_CHECK*");
    }

    [Fact]
    public void AbsolutePath_WithTilde_CanBeCreated()
    {
        AbsolutePath path = "~/.claude/.credentials.json";
        path.ToString().Should().Be("~/.claude/.credentials.json");
    }

    [Fact]
    public void AbsolutePath_Expand_TildePath_ResolvesToHome()
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        AbsolutePath path = "~/.claude/.credentials.json";
        var expanded = path.Expand();
        ((string)expanded).Should().StartWith(home);
        ((string)expanded).Should().EndWith(".credentials.json");
    }

    [Fact]
    public void AbsolutePath_Expand_EnvVar_Resolved()
    {
        Environment.SetEnvironmentVariable("TEST_ABS_DIR", "/opt/data");
        try
        {
            AbsolutePath path = "${TEST_ABS_DIR}/file.txt";
            var expanded = path.Expand();
            ((string)expanded).Should().Contain("/opt/data");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_ABS_DIR", null);
        }
    }

    [Fact]
    public void AbsolutePath_Expand_EnvVarInMiddle_Resolved()
    {
        Environment.SetEnvironmentVariable("TEST_MID_VAR", "agents");
        try
        {
            AbsolutePath path = "/var/${TEST_MID_VAR}/workspace";
            var expanded = path.Expand();
            ((string)expanded).Should().Contain("/var/agents/workspace");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_MID_VAR", null);
        }
    }

    [Fact]
    public void RelativePath_Expand_EnvVar_Resolved()
    {
        Environment.SetEnvironmentVariable("TEST_REL_DIR", "workspace");
        try
        {
            RelativePath path = "${TEST_REL_DIR}/src";
            var expanded = path.Expand();
            ((string)expanded).Should().Contain("workspace");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_REL_DIR", null);
        }
    }
}
