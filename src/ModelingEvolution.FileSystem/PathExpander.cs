using System.Text.RegularExpressions;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Expands path expressions: ~ (home directory) and ${ENV_VAR} or $ENV_VAR (environment variables).
/// Used by AbsolutePath.Expand and RelativePath.Expand to resolve paths at runtime.
/// </summary>
public static class PathExpander
{
    private static readonly Regex EnvVarPattern = new(@"\$\{([^}]+)\}|\$([a-zA-Z_][a-zA-Z0-9_]*)",
        RegexOptions.Compiled);

    /// <summary>
    /// Expands ~ to home directory and ${VAR}/$VAR to environment variable values.
    /// </summary>
    /// <param name="path">Path that may contain ~ or environment variable references.</param>
    /// <returns>Fully expanded path string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HOME is not set or an env var is missing.</exception>
    public static string Expand(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        path = ExpandHome(path);
        path = ExpandEnvironmentVariables(path);

        return path;
    }

    private static string ExpandHome(string path)
    {
        if (!path.StartsWith('~'))
            return path;

        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (string.IsNullOrEmpty(home))
            throw new InvalidOperationException(
                "Cannot expand '~': neither HOME environment variable nor UserProfile folder is set.");

        // ~/foo → /home/user/foo, ~ alone → /home/user
        if (path.Length == 1)
            return home;

        if (path[1] == '/' || path[1] == '\\')
            return home + path[1..];

        // ~username syntax not supported — return as-is
        return path;
    }

    private static string ExpandEnvironmentVariables(string path)
    {
        return EnvVarPattern.Replace(path, match =>
        {
            var varName = match.Groups[1].Success
                ? match.Groups[1].Value
                : match.Groups[2].Value;

            var value = Environment.GetEnvironmentVariable(varName);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    $"Cannot expand path: environment variable '{varName}' is not set.");

            return value;
        });
    }
}
