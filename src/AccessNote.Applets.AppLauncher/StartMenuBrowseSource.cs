using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AccessNote;

internal sealed class StartMenuBrowseSource : IAppBrowseSource
{
    private static readonly AppBrowseTitleMetadata StartMenuTitleMetadata = new(
        Confidence: TitleConfidence.Inferred,
        Provenance: "Start Menu item name");

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".lnk",
        ".exe",
        ".bat",
    };

    public string RootLabel => "Start Menu";

    public IReadOnlyList<AppBrowseSourceEntry> Discover()
    {
        var roots = GetRoots();
        if (roots.Count == 0)
        {
            return Array.Empty<AppBrowseSourceEntry>();
        }

        var namesSeenInSubfolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots)
        {
            foreach (var directory in EnumerateDirectoriesSafe(root))
            {
                if (string.Equals(directory, root, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var filePath in EnumerateFilesSafe(directory))
                {
                    if (!IsSupported(filePath))
                    {
                        continue;
                    }

                    var baseName = Path.GetFileNameWithoutExtension(filePath);
                    if (!string.IsNullOrWhiteSpace(baseName))
                    {
                        namesSeenInSubfolders.Add(baseName);
                    }
                }
            }
        }

        var entries = new List<AppBrowseSourceEntry>();
        foreach (var root in roots)
        {
            foreach (var directory in EnumerateDirectoriesSafe(root))
            {
                var isRoot = string.Equals(directory, root, StringComparison.OrdinalIgnoreCase);
                var relativePath = Path.GetRelativePath(root, directory);
                var categories = ToCategoryPath(relativePath);

                foreach (var filePath in EnumerateFilesSafe(directory))
                {
                    if (!IsSupported(filePath))
                    {
                        continue;
                    }

                    var baseName = Path.GetFileNameWithoutExtension(filePath);
                    if (string.IsNullOrWhiteSpace(baseName))
                    {
                        continue;
                    }

                    if (isRoot && namesSeenInSubfolders.Contains(baseName))
                    {
                        continue;
                    }

                    entries.Add(new AppBrowseSourceEntry(
                        DisplayName: baseName,
                        CategoryPath: categories,
                        LaunchSpec: LaunchSpec.DirectPath(filePath),
                        DetailPath: filePath,
                        TitleMetadata: StartMenuTitleMetadata));
                }
            }
        }

        return entries;
    }

    private static List<string> GetRoots()
    {
        var roots = new List<string>();

        var commonPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
        if (!string.IsNullOrWhiteSpace(commonPrograms) && Directory.Exists(commonPrograms))
        {
            roots.Add(commonPrograms);
        }

        var userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        if (!string.IsNullOrWhiteSpace(userPrograms) && Directory.Exists(userPrograms))
        {
            if (!roots.Any(existing => string.Equals(existing, userPrograms, StringComparison.OrdinalIgnoreCase)))
            {
                roots.Add(userPrograms);
            }
        }

        return roots;
    }

    private static IReadOnlyList<string> ToCategoryPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == ".")
        {
            return Array.Empty<string>();
        }

        return relativePath
            .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }

    private static IEnumerable<string> EnumerateDirectoriesSafe(string root)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            string[] subdirectories;
            try
            {
                subdirectories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            Array.Sort(subdirectories, StringComparer.OrdinalIgnoreCase);
            for (var index = subdirectories.Length - 1; index >= 0; index--)
            {
                stack.Push(subdirectories[index]);
            }
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string directory)
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(directory);
        }
        catch
        {
            yield break;
        }

        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            yield return file;
        }
    }

    private static bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return SupportedExtensions.Contains(extension);
    }
}
