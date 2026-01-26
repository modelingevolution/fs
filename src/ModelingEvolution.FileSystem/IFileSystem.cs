namespace ModelingEvolution.FileSystem;

/// <summary>
/// Abstraction for file system operations. Enables unit testing.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Computes the SHA-1 hash of a file.
    /// </summary>
    Sha1 ComputeSha1(AbsolutePath path);

    /// <summary>
    /// Computes the SHA-256 hash of a file.
    /// </summary>
    Sha256 ComputeSha256(AbsolutePath path);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    bool FileExists(AbsolutePath path);

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    bool DirectoryExists(AbsolutePath path);

    /// <summary>
    /// Reads all text from a file.
    /// </summary>
    string ReadAllText(AbsolutePath path);

    /// <summary>
    /// Reads all bytes from a file.
    /// </summary>
    byte[] ReadAllBytes(AbsolutePath path);

    /// <summary>
    /// Writes text to a file.
    /// </summary>
    void WriteAllText(AbsolutePath path, string content);

    /// <summary>
    /// Writes bytes to a file.
    /// </summary>
    void WriteAllBytes(AbsolutePath path, byte[] bytes);

    /// <summary>
    /// Creates a directory if it doesn't exist.
    /// </summary>
    void CreateDirectory(AbsolutePath path);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    void DeleteFile(AbsolutePath path);

    /// <summary>
    /// Deletes a directory and its contents.
    /// </summary>
    void DeleteDirectory(AbsolutePath path, bool recursive = false);

    /// <summary>
    /// Renames a file or directory.
    /// </summary>
    void Rename(AbsolutePath oldPath, AbsolutePath newPath);

    #region Async Methods

    /// <summary>
    /// Enumerates file system entries in a directory asynchronously.
    /// </summary>
    Task<IReadOnlyList<FileSystemEntry>> EnumerateEntriesAsync(
        AbsolutePath path,
        CancellationToken ct = default);

    /// <summary>
    /// Gets file info asynchronously.
    /// </summary>
    Task<FileSystemInfo?> GetFileSystemInfoAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Checks if a file exists asynchronously.
    /// </summary>
    Task<bool> FileExistsAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Checks if a directory exists asynchronously.
    /// </summary>
    Task<bool> DirectoryExistsAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Reads all text from a file asynchronously.
    /// </summary>
    Task<string> ReadAllTextAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Writes text to a file asynchronously.
    /// </summary>
    Task WriteAllTextAsync(AbsolutePath path, string content, CancellationToken ct = default);

    /// <summary>
    /// Creates a directory asynchronously.
    /// </summary>
    Task CreateDirectoryAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    Task DeleteFileAsync(AbsolutePath path, CancellationToken ct = default);

    /// <summary>
    /// Deletes a directory asynchronously.
    /// </summary>
    Task DeleteDirectoryAsync(AbsolutePath path, bool recursive = false, CancellationToken ct = default);

    /// <summary>
    /// Renames a file or directory asynchronously.
    /// </summary>
    Task RenameAsync(AbsolutePath oldPath, AbsolutePath newPath, CancellationToken ct = default);

    #endregion
}
