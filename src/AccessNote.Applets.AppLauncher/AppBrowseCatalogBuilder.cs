using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class AppBrowseCatalogBuilder
{
    private const string GamesFolderName = "Games";

    public AppBrowseNode Build(IReadOnlyList<IAppBrowseSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var root = AppBrowseNode.CreateFolder("Browse");
        var gamesRoot = AppBrowseNode.CreateFolder(GamesFolderName, detailPath: GamesFolderName);
        var hasGames = false;
        var dedupeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sources)
        {
            if (source == null)
            {
                continue;
            }

            IReadOnlyList<AppBrowseSourceEntry> entries;
            try
            {
                entries = source.Discover();
            }
            catch
            {
                // Source failures are skipped by design to avoid browse noise.
                continue;
            }

            if (entries.Count == 0)
            {
                continue;
            }

            var sourceNode = AppBrowseNode.CreateFolder(source.RootLabel, detailPath: source.RootLabel);
            root.AddChild(sourceNode);

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.DisplayName) || string.IsNullOrWhiteSpace(entry.LaunchSpec.Target))
                {
                    continue;
                }

                var identityKey = AppLauncherFavoriteLaunchCodec.GetIdentityKey(entry.LaunchSpec);
                if (!dedupeKeys.Add(identityKey))
                {
                    continue;
                }

                AddEntry(sourceNode, source.RootLabel, entry);
                if (IsGameEntry(entry))
                {
                    AddLeafEntry(gamesRoot, source.RootLabel, entry);
                    hasGames = true;
                }
            }

            if (sourceNode.Children.Count == 0)
            {
                root.RemoveLastChild();
            }
        }

        if (hasGames)
        {
            root.AddChild(gamesRoot);
        }

        root.SortRecursively();
        return root;
    }

    private static void AddEntry(AppBrowseNode sourceNode, string sourceLabel, AppBrowseSourceEntry entry)
    {
        var node = sourceNode;

        foreach (var category in entry.CategoryPath)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                continue;
            }

            var existing = node.FindChildFolder(category);
            if (existing != null)
            {
                node = existing;
                continue;
            }

            var categoryNode = AppBrowseNode.CreateFolder(category, detailPath: category);
            node.AddChild(categoryNode);
            node = categoryNode;
        }

        AddLeafEntry(node, sourceLabel, entry);
    }

    private static void AddLeafEntry(AppBrowseNode parentNode, string sourceLabel, AppBrowseSourceEntry entry)
    {
        var leafName = EnsureUniqueName(parentNode, entry.DisplayName);
        var detailPath = string.IsNullOrWhiteSpace(entry.DetailPath)
            ? entry.LaunchSpec.Target
            : entry.DetailPath;
        var detailArguments = entry.LaunchSpec.TargetType switch
        {
            LaunchTargetType.DirectPath => string.IsNullOrWhiteSpace(entry.LaunchSpec.Arguments)
                ? string.Empty
                : entry.LaunchSpec.Arguments,
            LaunchTargetType.ShellApp => "App package",
            LaunchTargetType.Uri => "URI",
            _ => string.Empty,
        };
        var titleMetadata = entry.TitleMetadata ?? AppBrowseTitleMetadata.Unknown;

        parentNode.AddChild(AppBrowseNode.CreateApp(
            name: leafName,
            launchSpec: entry.LaunchSpec,
            detailPath: detailPath,
            detailArguments: detailArguments,
            sourceLabel: sourceLabel,
            titleMetadata: titleMetadata));
    }

    private static bool IsGameEntry(AppBrowseSourceEntry entry)
    {
        if (entry.IsGame)
        {
            return true;
        }

        foreach (var category in entry.CategoryPath)
        {
            if (string.Equals(category, GamesFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string EnsureUniqueName(AppBrowseNode parent, string preferredName)
    {
        if (!parent.ContainsChildName(preferredName))
        {
            return preferredName;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{preferredName} ({suffix})";
            if (!parent.ContainsChildName(candidate))
            {
                return candidate;
            }

            suffix++;
        }
    }

}
