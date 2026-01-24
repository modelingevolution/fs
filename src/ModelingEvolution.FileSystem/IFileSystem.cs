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
}
