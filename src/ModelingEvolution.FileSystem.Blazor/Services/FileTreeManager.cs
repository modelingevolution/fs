using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelingEvolution.FileSystem.Blazor.Models;

namespace ModelingEvolution.FileSystem.Blazor.Services;

/// <summary>
/// Manages file system tree state for the FileExplorer component.
/// Supports lazy loading of directories.
/// </summary>
public class FileTreeManager : INotifyPropertyChanged
{
    private readonly IFileSystem _fileSystem;
    private readonly int _maxInitialDepth;
    private AbsolutePath _basePath;
    private bool _isLoading;

    public FileTreeManager(IFileSystem fileSystem, AbsolutePath basePath, int maxInitialDepth = 2)
    {
        _fileSystem = fileSystem;
        _basePath = basePath;
        _maxInitialDepth = maxInitialDepth;
    }

    /// <summary>
    /// Root nodes of the file tree.
    /// </summary>
    public ObservableCollection<FileNode> RootNodes { get; } = [];

    /// <summary>
    /// Whether the tree is currently loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetField(ref _isLoading, value);
    }

    /// <summary>
    /// The base path of the file tree.
    /// </summary>
    public AbsolutePath BasePath => _basePath;

    /// <summary>
    /// Updates the base path and reloads the tree.
    /// </summary>
    public async Task SetBasePathAsync(AbsolutePath basePath, CancellationToken ct = default)
    {
        _basePath = basePath;
        await LoadAsync(ct);
    }

    /// <summary>
    /// Loads the initial file tree (up to maxInitialDepth levels).
    /// </summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            RootNodes.Clear();

            if (!_fileSystem.DirectoryExists(_basePath))
                return;

            // Load root level and 1-2 levels deep
            await LoadDirectoryAsync(RelativePath.Empty, RootNodes, 0, ct);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDirectoryAsync(
        RelativePath relativePath,
        ObservableCollection<FileNode> targetCollection,
        int currentDepth,
        CancellationToken ct)
    {
        var fullPath = relativePath.IsEmpty ? _basePath : _basePath + relativePath;

        var entries = await _fileSystem.EnumerateEntriesAsync(fullPath, ct);

        var sortedEntries = entries
            .Where(e => !ShouldIgnore(e.Name, e.IsDirectory))
            .OrderBy(e => !e.IsDirectory)
            .ThenBy(e => e.Name.ToString(), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in sortedEntries)
        {
            ct.ThrowIfCancellationRequested();

            var entryRelativePath = relativePath.IsEmpty ? entry.Name : relativePath + entry.Name;

            var node = new FileNode
            {
                Name = entry.Name,
                RelativePath = entryRelativePath,
                IsDirectory = entry.IsDirectory,
                ChildrenLoaded = false
            };

            targetCollection.Add(node);

            // Load children if directory and within depth limit
            if (entry.IsDirectory && currentDepth < _maxInitialDepth)
            {
                node.ChildrenLoaded = true;
                await LoadDirectoryAsync(entryRelativePath, node.Children, currentDepth + 1, ct);
            }
        }
    }

    /// <summary>
    /// Loads children of a directory node on demand.
    /// </summary>
    public async Task LoadChildrenAsync(FileNode directory, CancellationToken ct = default)
    {
        if (!directory.IsDirectory || directory.ChildrenLoaded)
            return;

        await LoadDirectoryAsync(directory.RelativePath, directory.Children, 0, ct);
        directory.ChildrenLoaded = true;
    }

    private static bool ShouldIgnore(RelativePath name, bool isDirectory)
    {
        var nameStr = name.ToString();

        // Ignore hidden files/folders
        if (nameStr.StartsWith('.'))
            return true;

        if (isDirectory)
        {
            return nameStr is "bin" or "obj" or "node_modules" or ".git" or ".vs" or ".idea";
        }

        return false;
    }

    /// <summary>
    /// Finds a node by relative path.
    /// </summary>
    public FileNode? FindByPath(RelativePath relativePath)
    {
        foreach (var root in RootNodes)
        {
            var found = root.FindByPath(relativePath);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Refreshes the tree by reloading from file system.
    /// </summary>
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        await LoadAsync(ct);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
