using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelingEvolution.FileSystem.Blazor.Models;

/// <summary>
/// Represents a file or directory node in the file tree.
/// Supports lazy loading of children and INotifyPropertyChanged for UI binding.
/// </summary>
public class FileNode : INotifyPropertyChanged
{
    private RelativePath _name = RelativePath.Empty;
    private RelativePath _relativePath = RelativePath.Empty;
    private bool _isDirectory;
    private bool _isExpanded;
    private bool _childrenLoaded;

    public RelativePath Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public RelativePath RelativePath
    {
        get => _relativePath;
        set => SetField(ref _relativePath, value);
    }

    public bool IsDirectory
    {
        get => _isDirectory;
        set => SetField(ref _isDirectory, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public bool ChildrenLoaded
    {
        get => _childrenLoaded;
        internal set => SetField(ref _childrenLoaded, value);
    }

    /// <summary>
    /// Child nodes. For directories, populated via lazy loading.
    /// </summary>
    public ObservableCollection<FileNode> Children { get; } = [];

    /// <summary>
    /// Parent node reference for tree navigation.
    /// </summary>
    public FileNode? Parent { get; internal set; }

    /// <summary>
    /// Gets the full path by combining BasePath with RelativePath.
    /// </summary>
    public AbsolutePath GetFullPath(AbsolutePath basePath) => basePath + RelativePath;

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

    /// <summary>
    /// Finds a child node by relative path.
    /// </summary>
    public FileNode? FindByPath(RelativePath relativePath)
    {
        if (RelativePath == relativePath)
            return this;

        foreach (var child in Children)
        {
            var found = child.FindByPath(relativePath);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Adds a child node maintaining sort order (directories first, then alphabetical).
    /// </summary>
    public void AddChildSorted(FileNode child)
    {
        child.Parent = this;

        // Find insertion index to maintain order: directories first, then alphabetical
        var index = 0;
        foreach (var existing in Children)
        {
            if (child.IsDirectory && !existing.IsDirectory)
                break;
            if (child.IsDirectory == existing.IsDirectory &&
                string.Compare(child.Name.ToString(), existing.Name.ToString(), StringComparison.OrdinalIgnoreCase) < 0)
                break;
            index++;
        }

        Children.Insert(index, child);
    }

    /// <summary>
    /// Removes this node from its parent.
    /// </summary>
    public bool RemoveFromParent()
    {
        return Parent?.Children.Remove(this) ?? false;
    }
}
