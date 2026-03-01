using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Cross-platform path resolution. Translates between Windows drive paths and Linux/WSL mount paths.
/// Platform detection happens once at static initialization — no branching on each call.
/// </summary>
public static partial class FileSystemPath
{
    private static readonly Func<string, string> _resolve;

    static FileSystemPath()
    {
        _resolve = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ResolveOnWindows
            : ResolveOnLinux;
    }

    /// <summary>
    /// Resolves a path string for the current platform.
    /// On Linux/WSL: converts Windows drive paths (e.g., "D:\source") to "/mnt/d/source".
    /// On Windows: converts Linux mount paths (e.g., "/mnt/d/source") to "D:\source".
    /// Paths already native to the current platform are returned with normalized separators.
    /// Works with both absolute and relative-looking paths (only drive/mount prefixes are translated).
    /// </summary>
    public static string Resolve(string path) => _resolve(path);

    private static string ResolveOnWindows(string path)
    {
        // /mnt/X/... → X:\...
        var match = WslMountPattern().Match(path);
        if (match.Success)
        {
            var drive = char.ToUpper(match.Groups[1].ValueSpan[0]);
            var rest = match.Groups[2].Success ? match.Groups[2].Value : @"\";
            return drive + ":" + rest.Replace('/', '\\');
        }

        return path.Replace('/', '\\');
    }

    private static string ResolveOnLinux(string path)
    {
        // X:\ or X:/ → /mnt/x/...
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
        {
            var drive = char.ToLower(path[0]);
            var rest = path.Length > 2 ? path[2..] : "/";
            return "/mnt/" + drive + rest.Replace('\\', '/');
        }

        return path.Replace('\\', '/');
    }

    [GeneratedRegex(@"^/mnt/([a-zA-Z])(/.*)?$")]
    private static partial Regex WslMountPattern();
}
