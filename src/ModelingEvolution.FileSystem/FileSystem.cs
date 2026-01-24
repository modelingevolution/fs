using System.Security.Cryptography;

namespace ModelingEvolution.FileSystem;

/// <summary>
/// Default file system implementation that wraps System.IO operations.
/// </summary>
public class FileSystem : IFileSystem
{
    public Sha1 ComputeSha1(AbsolutePath path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA1.HashData(stream);
        return new Sha1(hash);
    }

    public Sha256 ComputeSha256(AbsolutePath path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new Sha256(hash);
    }

    public bool FileExists(AbsolutePath path) => File.Exists(path);

    public bool DirectoryExists(AbsolutePath path) => Directory.Exists(path);

    public string ReadAllText(AbsolutePath path) => File.ReadAllText(path);

    public byte[] ReadAllBytes(AbsolutePath path) => File.ReadAllBytes(path);

    public void WriteAllText(AbsolutePath path, string content) => File.WriteAllText(path, content);

    public void WriteAllBytes(AbsolutePath path, byte[] bytes) => File.WriteAllBytes(path, bytes);

    public void CreateDirectory(AbsolutePath path) => Directory.CreateDirectory(path);

    public void DeleteFile(AbsolutePath path) => File.Delete(path);

    public void DeleteDirectory(AbsolutePath path, bool recursive = false) => Directory.Delete(path, recursive);
}
