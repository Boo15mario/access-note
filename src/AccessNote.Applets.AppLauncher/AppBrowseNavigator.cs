using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal sealed class AppBrowseNavigator
{
    private AppBrowseNode _root;
    private readonly List<AppBrowseNode> _stack;

    public AppBrowseNavigator()
    {
        _root = AppBrowseNode.CreateFolder("Browse");
        _stack = new List<AppBrowseNode> { _root };
    }

    public IReadOnlyList<AppBrowseNode> CurrentEntries => _stack[_stack.Count - 1].Children;

    public void Load(AppBrowseNode root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _stack.Clear();
        _stack.Add(_root);
    }

    public void RestorePath(string pathDisplay)
    {
        _stack.Clear();
        _stack.Add(_root);

        if (string.IsNullOrWhiteSpace(pathDisplay)
            || string.Equals(pathDisplay, "Browse", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var segments = pathDisplay.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var current = _stack[_stack.Count - 1];
            var child = current.FindChildFolder(segment);
            if (child == null)
            {
                break;
            }

            _stack.Add(child);
        }
    }

    public bool TryEnterFolder(int selectedIndex, out string folderName)
    {
        folderName = string.Empty;

        var entry = GetEntry(selectedIndex);
        if (entry == null || !entry.IsFolder)
        {
            return false;
        }

        _stack.Add(entry);
        folderName = entry.Name;
        return true;
    }

    public bool TryNavigateUp(out string folderName)
    {
        folderName = string.Empty;

        if (_stack.Count <= 1)
        {
            return false;
        }

        _stack.RemoveAt(_stack.Count - 1);
        folderName = _stack.Count == 1
            ? "Browse"
            : _stack[_stack.Count - 1].Name;
        return true;
    }

    public AppBrowseNode? GetEntry(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return null;
        }

        var entries = CurrentEntries;
        if (selectedIndex >= entries.Count)
        {
            return null;
        }

        return entries[selectedIndex];
    }

    public string GetCurrentPathDisplay()
    {
        if (_stack.Count <= 1)
        {
            return "Browse";
        }

        return string.Join("\\", _stack.Skip(1).Select(static node => node.Name));
    }
}
