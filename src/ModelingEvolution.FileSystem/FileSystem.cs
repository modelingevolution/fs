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

    public void Rename(AbsolutePath oldPath, AbsolutePath newPath)
    {
        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);
        else if (Directory.Exists(oldPath))
            Directory.Move(oldPath, newPath);
        else
            throw new FileNotFoundException($"Path not found: {oldPath}");
    }

    #region Async Methods

    public Task<IReadOnlyList<FileSystemEntry>> EnumerateEntriesAsync(AbsolutePath path, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var entries = new List<FileSystemEntry>();
            var dirInfo = new DirectoryInfo(path);

            if (!dirInfo.Exists)
                return (IReadOnlyList<FileSystemEntry>)entries;

            foreach (var fsInfo in dirInfo.EnumerateFileSystemInfos())
            {
                ct.ThrowIfCancellationRequested();

                var isDirectory = (fsInfo.Attributes & FileAttributes.Directory) != 0;
                var size = isDirectory ? 0 : ((FileInfo)fsInfo).Length;

                entries.Add(new FileSystemEntry(
                    new AbsolutePath(fsInfo.FullName),
                    new RelativePath(fsInfo.Name),
                    isDirectory,
                    size,
                    fsInfo.LastWriteTime));
            }

            return (IReadOnlyList<FileSystemEntry>)entries;
        }, ct);
    }

    public Task<FileSystemInfo?> GetFileSystemInfoAsync(AbsolutePath path, CancellationToken ct = default)
    {
        return Task.Run<FileSystemInfo?>(() =>
        {
            if (File.Exists(path))
                return new FileInfo(path);
            if (Directory.Exists(path))
                return new DirectoryInfo(path);
            return null;
        }, ct);
    }

    public Task<bool> FileExistsAsync(AbsolutePath path, CancellationToken ct = default)
        => Task.FromResult(File.Exists(path));

    public Task<bool> DirectoryExistsAsync(AbsolutePath path, CancellationToken ct = default)
        => Task.FromResult(Directory.Exists(path));

    public Task<string> ReadAllTextAsync(AbsolutePath path, CancellationToken ct = default)
        => File.ReadAllTextAsync(path, ct);

    public Task WriteAllTextAsync(AbsolutePath path, string content, CancellationToken ct = default)
        => File.WriteAllTextAsync(path, content, ct);

    public Task CreateDirectoryAsync(AbsolutePath path, CancellationToken ct = default)
    {
        Directory.CreateDirectory(path);
        return Task.CompletedTask;
    }

    public Task DeleteFileAsync(AbsolutePath path, CancellationToken ct = default)
    {
        File.Delete(path);
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(AbsolutePath path, bool recursive = false, CancellationToken ct = default)
    {
        Directory.Delete(path, recursive);
        return Task.CompletedTask;
    }

    public Task RenameAsync(AbsolutePath oldPath, AbsolutePath newPath, CancellationToken ct = default)
    {
        Rename(oldPath, newPath);
        return Task.CompletedTask;
    }

    #endregion
}
