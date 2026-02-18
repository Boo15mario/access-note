using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AccessNote;

internal sealed class HeroicBrowseSource : IAppBrowseSource
{
    private static readonly AppBrowseTitleMetadata HeroicLauncherTitleMetadata = new(
        Confidence: TitleConfidence.Verified,
        Provenance: "Heroic executable");

    public string RootLabel => "Heroic";

    public IReadOnlyList<AppBrowseSourceEntry> Discover()
    {
        var entries = new List<AppBrowseSourceEntry>();
        var launcherPath = GetHeroicLauncherPath();

        if (!string.IsNullOrWhiteSpace(launcherPath))
        {
            entries.Add(new AppBrowseSourceEntry(
                DisplayName: "Heroic Launcher",
                CategoryPath: Array.Empty<string>(),
                LaunchSpec: LaunchSpec.DirectPath(launcherPath),
                DetailPath: launcherPath,
                TitleMetadata: HeroicLauncherTitleMetadata));
        }

        var heroicDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "heroic");
        if (!Directory.Exists(heroicDataRoot))
        {
            return entries;
        }

        var seenIdentities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var installedJsonPath in EnumerateInstalledJsonFiles(heroicDataRoot))
        {
            ReadInstalledFile(installedJsonPath, launcherPath, seenIdentities, entries);
        }

        return entries;
    }

    private static void ReadInstalledFile(
        string installedJsonPath,
        string launcherPath,
        HashSet<string> seenIdentities,
        List<AppBrowseSourceEntry> entries)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(installedJsonPath));
        }
        catch
        {
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    TryAddGame(property.Value, property.Name, launcherPath, seenIdentities, entries);
                }

                return;
            }

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in document.RootElement.EnumerateArray())
                {
                    TryAddGame(item, string.Empty, launcherPath, seenIdentities, entries);
                }
            }
        }
    }

    private static void TryAddGame(
        JsonElement item,
        string fallbackId,
        string launcherPath,
        HashSet<string> seenIdentities,
        List<AppBrowseSourceEntry> entries)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var gameId = GetString(item, "app_name")
            ?? GetString(item, "appName")
            ?? GetString(item, "id")
            ?? fallbackId;

        var explicitDisplayName = GetString(item, "title")
            ?? GetString(item, "app_title")
            ?? GetString(item, "name");

        var executable = GetString(item, "executable")
            ?? GetString(item, "exe")
            ?? GetString(item, "launchExecutable");
        var installPath = GetString(item, "install_path")
            ?? GetString(item, "installPath")
            ?? GetString(item, "path");
        var resolvedTitle = ResolveTitle(explicitDisplayName, gameId, installPath);
        if (string.IsNullOrWhiteSpace(resolvedTitle.DisplayName))
        {
            return;
        }

        if (TryResolveExecutable(executable, installPath, out var resolvedExecutable))
        {
            var identity = AppLauncherFavoriteLaunchCodec.GetIdentityKey(LaunchSpec.DirectPath(resolvedExecutable));
            if (!seenIdentities.Add(identity))
            {
                return;
            }

            entries.Add(new AppBrowseSourceEntry(
                DisplayName: resolvedTitle.DisplayName,
                CategoryPath: new[] { "Games" },
                LaunchSpec: LaunchSpec.DirectPath(resolvedExecutable),
                DetailPath: resolvedExecutable,
                TitleMetadata: resolvedTitle.TitleMetadata,
                IsGame: true));
            return;
        }

        if (string.IsNullOrWhiteSpace(gameId) || string.IsNullOrWhiteSpace(launcherPath))
        {
            return;
        }

        var launchUri = "heroic://launch/" + Uri.EscapeDataString(gameId);
        var uriSpec = LaunchSpec.Uri(launchUri);
        var uriIdentity = AppLauncherFavoriteLaunchCodec.GetIdentityKey(uriSpec);
        if (!seenIdentities.Add(uriIdentity))
        {
            return;
        }

        entries.Add(new AppBrowseSourceEntry(
            DisplayName: resolvedTitle.DisplayName,
            CategoryPath: new[] { "Games" },
            LaunchSpec: uriSpec,
            DetailPath: launchUri,
            TitleMetadata: resolvedTitle.TitleMetadata,
            IsGame: true));
    }

    internal static string ResolveDisplayName(string? explicitDisplayName, string? gameId, string? installPath)
    {
        return ResolveTitle(explicitDisplayName, gameId, installPath).DisplayName;
    }

    private static ResolvedTitle ResolveTitle(string? explicitDisplayName, string? gameId, string? installPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitDisplayName))
        {
            var normalized = explicitDisplayName.Trim();
            if (!LooksLikeOpaqueIdentifier(normalized))
            {
                return new ResolvedTitle(
                    DisplayName: normalized,
                    TitleMetadata: new AppBrowseTitleMetadata(
                        Confidence: TitleConfidence.Verified,
                        Provenance: "Heroic title metadata"));
            }
        }

        var installDirectoryName = ExtractDirectoryName(installPath);
        if (!string.IsNullOrWhiteSpace(installDirectoryName))
        {
            return new ResolvedTitle(
                DisplayName: installDirectoryName,
                TitleMetadata: new AppBrowseTitleMetadata(
                    Confidence: TitleConfidence.Inferred,
                    Provenance: "Heroic install path"));
        }

        if (!string.IsNullOrWhiteSpace(explicitDisplayName))
        {
            return new ResolvedTitle(
                DisplayName: explicitDisplayName.Trim(),
                TitleMetadata: new AppBrowseTitleMetadata(
                    Confidence: TitleConfidence.Fallback,
                    Provenance: "Heroic opaque title id"));
        }

        if (!string.IsNullOrWhiteSpace(gameId))
        {
            return new ResolvedTitle(
                DisplayName: gameId.Trim(),
                TitleMetadata: new AppBrowseTitleMetadata(
                    Confidence: TitleConfidence.Fallback,
                    Provenance: "Heroic game id"));
        }

        return new ResolvedTitle(
            DisplayName: string.Empty,
            TitleMetadata: AppBrowseTitleMetadata.Unknown);
    }

    private static bool LooksLikeOpaqueIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.StartsWith("amzn1.adg.product.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Guid.TryParse(value, out _))
        {
            return true;
        }

        if (value.Length >= 16 && IsHex(value))
        {
            return true;
        }

        if (value.Length >= 6 && IsNumeric(value))
        {
            return true;
        }

        return false;
    }

    private static string ExtractDirectoryName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var normalizedPath = path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return string.Empty;
        }

        try
        {
            var leaf = Path.GetFileName(normalizedPath);
            return string.IsNullOrWhiteSpace(leaf) ? string.Empty : leaf.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IsHex(string value)
    {
        foreach (var character in value)
        {
            if (!Uri.IsHexDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNumeric(string value)
    {
        foreach (var character in value)
        {
            if (!char.IsDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryResolveExecutable(string? executable, string? installPath, out string resolvedExecutable)
    {
        resolvedExecutable = string.Empty;

        if (string.IsNullOrWhiteSpace(executable))
        {
            return false;
        }

        if (Path.IsPathRooted(executable) && File.Exists(executable))
        {
            resolvedExecutable = executable;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(installPath))
        {
            var combinedPath = Path.Combine(installPath, executable);
            if (File.Exists(combinedPath))
            {
                resolvedExecutable = combinedPath;
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> EnumerateInstalledJsonFiles(string root)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            string[] subdirectories;
            try
            {
                subdirectories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var subdirectory in subdirectories)
            {
                stack.Push(subdirectory);
            }

            var filePath = Path.Combine(current, "installed.json");
            if (File.Exists(filePath))
            {
                yield return filePath;
            }
        }
    }

    private static string GetHeroicLauncherPath()
    {
        var candidates = new List<string>();

        var localPrograms = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "heroic",
            "Heroic.exe");
        candidates.Add(localPrograms);

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            candidates.Add(Path.Combine(programFiles, "Heroic", "Heroic.exe"));
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            candidates.Add(Path.Combine(programFilesX86, "Heroic", "Heroic.exe"));
        }

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private readonly record struct ResolvedTitle(
        string DisplayName,
        AppBrowseTitleMetadata TitleMetadata);
}
