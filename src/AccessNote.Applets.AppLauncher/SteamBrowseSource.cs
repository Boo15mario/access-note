using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AccessNote;

internal sealed class SteamBrowseSource : IAppBrowseSource
{
    private static readonly AppBrowseTitleMetadata SteamLauncherTitleMetadata = new(
        Confidence: TitleConfidence.Verified,
        Provenance: "Steam executable");
    private static readonly AppBrowseTitleMetadata SteamManifestTitleMetadata = new(
        Confidence: TitleConfidence.Verified,
        Provenance: "Steam appmanifest name");
    private static readonly AppBrowseTitleMetadata SteamFallbackTitleMetadata = new(
        Confidence: TitleConfidence.Fallback,
        Provenance: "Steam app id");

    private static readonly Regex LibraryPathRegex = new("\"path\"\\s+\"(?<path>[^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NameRegex = new("\"name\"\\s+\"(?<name>[^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ManifestIdRegex = new("appmanifest_(?<id>\\d+)\\.acf", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string RootLabel => "Steam";

    public IReadOnlyList<AppBrowseSourceEntry> Discover()
    {
        var entries = new List<AppBrowseSourceEntry>();
        var steamDirectory = GetSteamDirectory();
        if (string.IsNullOrWhiteSpace(steamDirectory))
        {
            return entries;
        }

        var steamExePath = Path.Combine(steamDirectory, "steam.exe");
        if (File.Exists(steamExePath))
        {
            entries.Add(new AppBrowseSourceEntry(
                DisplayName: "Steam",
                CategoryPath: Array.Empty<string>(),
                LaunchSpec: LaunchSpec.DirectPath(steamExePath),
                DetailPath: steamExePath,
                TitleMetadata: SteamLauncherTitleMetadata));
        }

        var libraryPaths = GetLibraryPaths(steamDirectory);
        var seenAppIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var libraryPath in libraryPaths)
        {
            var steamAppsPath = Path.Combine(libraryPath, "steamapps");
            if (!Directory.Exists(steamAppsPath))
            {
                continue;
            }

            string[] manifests;
            try
            {
                manifests = Directory.GetFiles(steamAppsPath, "appmanifest_*.acf");
            }
            catch
            {
                continue;
            }

            foreach (var manifestPath in manifests)
            {
                var fileName = Path.GetFileName(manifestPath);
                if (fileName == null)
                {
                    continue;
                }

                var manifestMatch = ManifestIdRegex.Match(fileName);
                if (!manifestMatch.Success)
                {
                    continue;
                }

                var appId = manifestMatch.Groups["id"].Value;
                if (string.IsNullOrWhiteSpace(appId) || !seenAppIds.Add(appId))
                {
                    continue;
                }

                var title = ParseTitleFromManifest(manifestPath);
                var titleMetadata = SteamManifestTitleMetadata;
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = "Steam game " + appId;
                    titleMetadata = SteamFallbackTitleMetadata;
                }

                var launchUri = "steam://rungameid/" + appId;
                entries.Add(new AppBrowseSourceEntry(
                    DisplayName: title,
                    CategoryPath: new[] { "Games" },
                    LaunchSpec: LaunchSpec.Uri(launchUri),
                    DetailPath: launchUri,
                    TitleMetadata: titleMetadata,
                    IsGame: true));
            }
        }

        return entries;
    }

    private static string ParseTitleFromManifest(string manifestPath)
    {
        string content;
        try
        {
            content = File.ReadAllText(manifestPath);
        }
        catch
        {
            return string.Empty;
        }

        var match = NameRegex.Match(content);
        if (!match.Success)
        {
            return string.Empty;
        }

        return match.Groups["name"].Value.Trim();
    }

    private static HashSet<string> GetLibraryPaths(string steamDirectory)
    {
        var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            steamDirectory,
        };

        var librariesFile = Path.Combine(steamDirectory, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(librariesFile))
        {
            return libraries;
        }

        string content;
        try
        {
            content = File.ReadAllText(librariesFile);
        }
        catch
        {
            return libraries;
        }

        var matches = LibraryPathRegex.Matches(content);
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var pathText = match.Groups["path"].Value.Replace("\\\\", "\\").Trim();
            if (string.IsNullOrWhiteSpace(pathText))
            {
                continue;
            }

            if (Directory.Exists(pathText))
            {
                libraries.Add(pathText);
            }
        }

        return libraries;
    }

    private static string GetSteamDirectory()
    {
        var steamPathFromRegistry = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null) as string;
        if (!string.IsNullOrWhiteSpace(steamPathFromRegistry))
        {
            var normalized = steamPathFromRegistry.Replace('/', '\\').Trim();
            if (Directory.Exists(normalized))
            {
                return normalized;
            }
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            var defaultPath = Path.Combine(programFilesX86, "Steam");
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }
        }

        return string.Empty;
    }
}
