using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class AppBrowseNode
{
    private readonly List<AppBrowseNode> _children;

    private AppBrowseNode(
        string name,
        LaunchSpec? launchSpec,
        string detailPath,
        string detailArguments,
        string sourceLabel,
        AppBrowseTitleMetadata titleMetadata)
    {
        Name = name;
        LaunchSpec = launchSpec;
        DetailPath = detailPath;
        DetailArguments = detailArguments;
        SourceLabel = sourceLabel;
        TitleMetadata = titleMetadata;
        _children = new List<AppBrowseNode>();
    }

    public string Name { get; }

    public LaunchSpec? LaunchSpec { get; }

    public bool IsFolder => !LaunchSpec.HasValue;

    public string DetailPath { get; }

    public string DetailArguments { get; }

    public string SourceLabel { get; }

    public AppBrowseTitleMetadata TitleMetadata { get; }

    public IReadOnlyList<AppBrowseNode> Children => _children;

    public static AppBrowseNode CreateFolder(string name, string detailPath = "")
    {
        return new AppBrowseNode(
            name: name,
            launchSpec: null,
            detailPath: detailPath,
            detailArguments: "Folder",
            sourceLabel: string.Empty,
            titleMetadata: AppBrowseTitleMetadata.Unknown);
    }

    public static AppBrowseNode CreateApp(
        string name,
        LaunchSpec launchSpec,
        string detailPath,
        string detailArguments,
        string sourceLabel,
        AppBrowseTitleMetadata titleMetadata)
    {
        return new AppBrowseNode(name, launchSpec, detailPath, detailArguments, sourceLabel, titleMetadata);
    }

    public void AddChild(AppBrowseNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
    }

    public bool RemoveLastChild()
    {
        if (_children.Count == 0)
        {
            return false;
        }

        _children.RemoveAt(_children.Count - 1);
        return true;
    }

    public AppBrowseNode? FindChildFolder(string name)
    {
        foreach (var child in _children)
        {
            if (child.IsFolder && string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }

        return null;
    }

    public bool ContainsChildName(string name)
    {
        foreach (var child in _children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void SortRecursively()
    {
        _children.Sort(CompareNodes);
        foreach (var child in _children)
        {
            child.SortRecursively();
        }
    }

    private static int CompareNodes(AppBrowseNode left, AppBrowseNode right)
    {
        if (left.IsFolder && !right.IsFolder)
        {
            return -1;
        }

        if (!left.IsFolder && right.IsFolder)
        {
            return 1;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
    }
}
