using FluentAssertions;
using Xunit;

namespace ModelingEvolution.FileSystem.Tests;

public class FileSystemTests : IDisposable
{
    private readonly FileSystem _fileSystem = new();
    private readonly AbsolutePath _testDir;

    public FileSystemTests()
    {
        var tempPath = Path.GetTempPath();
        _testDir = new AbsolutePath(Path.Combine(tempPath, "FileSystemTests_" + Guid.NewGuid().ToString("N")[..8]));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public void ComputeSha1_EmptyFile_ReturnsExpectedHash()
    {
        var path = _testDir + "empty.txt";
        File.WriteAllBytes(path, []);

        var result = _fileSystem.ComputeSha1(path);

        // SHA-1 of empty file: da39a3ee5e6b4b0d3255bfef95601890afd80709
        result.ToString().Should().Be("da39a3ee5e6b4b0d3255bfef95601890afd80709");
    }

    [Fact]
    public void ComputeSha1_KnownContent_ReturnsExpectedHash()
    {
        var path = _testDir + "test.txt";
        File.WriteAllText(path, "hello");

        var result = _fileSystem.ComputeSha1(path);

        // SHA-1 of "hello": aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d
        result.ToString().Should().Be("aaf4c61ddcc5e8a2dabede0f3b482cd9aea9434d");
    }

    [Fact]
    public void ComputeSha256_EmptyFile_ReturnsExpectedHash()
    {
        var path = _testDir + "empty.txt";
        File.WriteAllBytes(path, []);

        var result = _fileSystem.ComputeSha256(path);

        // SHA-256 of empty file: e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
        result.ToString().Should().Be("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
    }

    [Fact]
    public void ComputeSha256_KnownContent_ReturnsExpectedHash()
    {
        var path = _testDir + "test.txt";
        File.WriteAllText(path, "hello");

        var result = _fileSystem.ComputeSha256(path);

        // SHA-256 of "hello": 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
        result.ToString().Should().Be("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
    }

    [Fact]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        var path = _testDir + "exists.txt";
        File.WriteAllText(path, "content");

        _fileSystem.FileExists(path).Should().BeTrue();
    }

    [Fact]
    public void FileExists_NonExistingFile_ReturnsFalse()
    {
        var path = _testDir + "nonexistent.txt";

        _fileSystem.FileExists(path).Should().BeFalse();
    }

    [Fact]
    public void DirectoryExists_ExistingDirectory_ReturnsTrue()
    {
        _fileSystem.DirectoryExists(_testDir).Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_NonExistingDirectory_ReturnsFalse()
    {
        var path = _testDir + "nonexistent";

        _fileSystem.DirectoryExists(path).Should().BeFalse();
    }

    [Fact]
    public void ReadAllText_ExistingFile_ReturnsContent()
    {
        var path = _testDir + "read.txt";
        File.WriteAllText(path, "test content");

        var result = _fileSystem.ReadAllText(path);

        result.Should().Be("test content");
    }

    [Fact]
    public void ReadAllBytes_ExistingFile_ReturnsBytes()
    {
        var path = _testDir + "bytes.bin";
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(path, bytes);

        var result = _fileSystem.ReadAllBytes(path);

        result.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void WriteAllText_CreatesFile()
    {
        var path = _testDir + "write.txt";

        _fileSystem.WriteAllText(path, "written content");

        File.ReadAllText(path).Should().Be("written content");
    }

    [Fact]
    public void WriteAllBytes_CreatesFile()
    {
        var path = _testDir + "writebytes.bin";
        var bytes = new byte[] { 10, 20, 30 };

        _fileSystem.WriteAllBytes(path, bytes);

        File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void CreateDirectory_CreatesDirectory()
    {
        var path = _testDir + "newdir";

        _fileSystem.CreateDirectory(path);

        Directory.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void DeleteFile_RemovesFile()
    {
        var path = _testDir + "todelete.txt";
        File.WriteAllText(path, "delete me");

        _fileSystem.DeleteFile(path);

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void DeleteDirectory_EmptyDirectory_RemovesDirectory()
    {
        var path = _testDir + "emptydir";
        Directory.CreateDirectory(path);

        _fileSystem.DeleteDirectory(path);

        Directory.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void DeleteDirectory_WithRecursive_RemovesDirectoryAndContents()
    {
        var path = _testDir + "dirwithfiles";
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "file.txt"), "content");

        _fileSystem.DeleteDirectory(path, recursive: true);

        Directory.Exists(path).Should().BeFalse();
    }
}
