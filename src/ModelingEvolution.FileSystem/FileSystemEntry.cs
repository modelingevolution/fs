namespace ModelingEvolution.FileSystem;

/// <summary>
/// Represents a file system entry (file or directory) with its metadata.
/// </summary>
public readonly record struct FileSystemEntry(
    AbsolutePath Path,
    RelativePath Name,
    bool IsDirectory,
    long Size,
    DateTime LastModified);
