using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;

namespace AccessNote;

internal sealed class XboxBrowseSource : IAppBrowseSource
{
    private static readonly string[] GamingPackageTokens =
    {
        "xbox",
        "gaming",
        "gamepass",
    };

    private static readonly string[] XboxOnlyTokens =
    {
        "xbox",
    };

    private static readonly string[] GameInstallPathMarkers =
    {
        "XboxGames",
        "MSIXVC",
    };

    private static readonly string[] InfrastructureSuppressTokens =
    {
        "GamingServices",
        "Gaming Services",
        "XboxGamingOverlay",
        "Game Bar",
        "XboxSeriesX",
        "Xbox Series X",
        "SecHealthUI",
        "Microsoft Defender",
        "XboxIdentityProvider",
        "XboxGameCallableUI",
        "XboxSpeechToTextOverlay",
        "Xbox.TCUI",
        "Xbox TCUI",
    };

    private const string StoreGamePackagePrefix = "Microsoft.";
    private const int MinStoreGamePackageSuffixLength = 8;

    private static readonly AppBrowseTitleMetadata XboxStartAppsTitleMetadata = new(
        Confidence: TitleConfidence.Verified,
        Provenance: "Windows Start Apps");
    private static readonly AppBrowseTitleMetadata XboxManifestTitleMetadata = new(
        Confidence: TitleConfidence.Inferred,
        Provenance: "Appx manifest display name");
    private static readonly AppBrowseTitleMetadata XboxFallbackTitleMetadata = new(
        Confidence: TitleConfidence.Fallback,
        Provenance: "Package and app id fallback");

    public string RootLabel => "Xbox";

    public IReadOnlyList<AppBrowseSourceEntry> Discover()
    {
        var entries = new List<AppBrowseSourceEntry>();
        var startAppNames = LoadStartAppNames();

        foreach (var package in LoadPackages())
        {
            if (package.IsFramework || string.IsNullOrWhiteSpace(package.PackageFamilyName) || string.IsNullOrWhiteSpace(package.InstallLocation))
            {
                continue;
            }

            var manifestPath = Path.Combine(package.InstallLocation, "AppxManifest.xml");
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            if (!TryLoadManifestApplications(manifestPath, package, startAppNames, out var applications))
            {
                continue;
            }

            foreach (var app in applications)
            {
                var appUserModelId = package.PackageFamilyName + "!" + app.ApplicationId;
                var detailPath = "shell:AppsFolder\\" + appUserModelId;
                entries.Add(new AppBrowseSourceEntry(
                    DisplayName: app.DisplayName,
                    CategoryPath: app.CategoryPath,
                    LaunchSpec: LaunchSpec.ShellApp(appUserModelId),
                    DetailPath: detailPath,
                    TitleMetadata: app.TitleMetadata,
                    IsGame: app.IsGame));
            }
        }

        return entries;
    }

    private static bool TryLoadManifestApplications(
        string manifestPath,
        AppxPackage package,
        IReadOnlyDictionary<string, string> startAppNames,
        out List<AppManifestApplication> applications)
    {
        applications = new List<AppManifestApplication>();

        XDocument document;
        try
        {
            document = XDocument.Load(manifestPath);
        }
        catch
        {
            return false;
        }

        var packageName = package.Name ?? string.Empty;
        var familyName = package.PackageFamilyName ?? string.Empty;

        var dependencyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dependencyElement in document.Descendants())
        {
            if (!string.Equals(dependencyElement.Name.LocalName, "PackageDependency", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var dependencyName = dependencyElement.Attribute("Name")?.Value;
            if (!string.IsNullOrWhiteSpace(dependencyName))
            {
                dependencyNames.Add(dependencyName);
            }
        }

        var looksGamingPackage = ContainsGamingToken(packageName)
            || ContainsGamingToken(familyName)
            || HasGamingDependency(dependencyNames)
            || LooksLikeXboxGameInstallLocation(package.InstallLocation)
            || (LooksLikeStoreGamePackageName(packageName)
                && HasStartAppsForPackage(
                    packageFamilyName: familyName,
                    startAppNames: startAppNames));

        if (!looksGamingPackage)
        {
            return false;
        }

        foreach (var applicationElement in document.Descendants())
        {
            if (!string.Equals(applicationElement.Name.LocalName, "Application", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var appId = applicationElement.Attribute("Id")?.Value;
            if (string.IsNullOrWhiteSpace(appId))
            {
                continue;
            }

            var appUserModelId = package.PackageFamilyName + "!" + appId;
            var manifestDisplayName = ExtractDisplayName(applicationElement);
            var resolvedTitle = ResolveTitle(
                manifestDisplayName: manifestDisplayName,
                packageName: packageName,
                appId: appId,
                appUserModelId: appUserModelId,
                startAppNames: startAppNames);
            if (ShouldSuppressInfrastructureApp(
                displayName: resolvedTitle.DisplayName,
                packageName: packageName,
                appId: appId))
            {
                continue;
            }

            var isGame = !ContainsXboxToken(packageName) && !ContainsXboxToken(resolvedTitle.DisplayName);
            var hasExactStartApp = HasExactStartApp(startAppNames, appUserModelId);
            if (!isGame && !hasExactStartApp)
            {
                // Avoid surfacing internal package endpoints that are not user-visible Start apps.
                continue;
            }

            applications.Add(new AppManifestApplication(
                ApplicationId: appId,
                DisplayName: resolvedTitle.DisplayName,
                CategoryPath: isGame ? new[] { "Games" } : Array.Empty<string>(),
                IsGame: isGame,
                TitleMetadata: resolvedTitle.TitleMetadata));
        }

        return applications.Count > 0;
    }

    private static string ExtractDisplayName(XElement applicationElement)
    {
        foreach (var child in applicationElement.DescendantsAndSelf())
        {
            if (!child.Name.LocalName.EndsWith("VisualElements", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var displayName = child.Attribute("DisplayName")?.Value;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName.Trim();
            }
        }

        return string.Empty;
    }

    internal static string ResolveDisplayName(
        string manifestDisplayName,
        string packageName,
        string appId,
        string appUserModelId,
        IReadOnlyDictionary<string, string> startAppNames)
    {
        return ResolveTitle(manifestDisplayName, packageName, appId, appUserModelId, startAppNames).DisplayName;
    }

    private static ResolvedTitle ResolveTitle(
        string manifestDisplayName,
        string packageName,
        string appId,
        string appUserModelId,
        IReadOnlyDictionary<string, string> startAppNames)
    {
        if (TryGetStartAppName(startAppNames, appUserModelId, out var startAppName))
        {
            return new ResolvedTitle(startAppName, XboxStartAppsTitleMetadata);
        }

        if (!string.IsNullOrWhiteSpace(manifestDisplayName)
            && !manifestDisplayName.StartsWith("ms-resource:", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedTitle(manifestDisplayName.Trim(), XboxManifestTitleMetadata);
        }

        return new ResolvedTitle(BuildFallbackDisplayName(packageName, appId), XboxFallbackTitleMetadata);
    }

    private static bool TryGetStartAppName(
        IReadOnlyDictionary<string, string> startAppNames,
        string appUserModelId,
        out string displayName)
    {
        displayName = string.Empty;
        if (string.IsNullOrWhiteSpace(appUserModelId) || startAppNames.Count == 0)
        {
            return false;
        }

        if (startAppNames.TryGetValue(appUserModelId, out var exactName)
            && !string.IsNullOrWhiteSpace(exactName))
        {
            displayName = exactName.Trim();
            return true;
        }

        var separatorIndex = appUserModelId.IndexOf('!');
        if (separatorIndex <= 0)
        {
            return false;
        }

        var packageFamilyNamePrefix = appUserModelId[..separatorIndex] + "!";
        string? singlePackageName = null;
        foreach (var startApp in startAppNames)
        {
            if (!startApp.Key.StartsWith(packageFamilyNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var candidate = startApp.Value?.Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (singlePackageName == null)
            {
                singlePackageName = candidate;
                continue;
            }

            if (!string.Equals(singlePackageName, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (singlePackageName == null)
        {
            return false;
        }

        displayName = singlePackageName;
        return true;
    }

    private static string BuildFallbackDisplayName(string packageName, string appId)
    {
        var normalizedPackageName = string.IsNullOrWhiteSpace(packageName)
            ? "Xbox App"
            : packageName.Replace('.', ' ');

        if (string.IsNullOrWhiteSpace(appId) || string.Equals(appId, "App", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedPackageName;
        }

        return normalizedPackageName + " " + appId;
    }

    private static bool HasGamingDependency(HashSet<string> dependencyNames)
    {
        foreach (var dependencyName in dependencyNames)
        {
            if (ContainsGamingToken(dependencyName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsGamingToken(string value)
    {
        return ContainsAnyToken(value, GamingPackageTokens);
    }

    private static bool LooksLikeXboxGameInstallLocation(string installLocation)
    {
        return ContainsAnyToken(installLocation, GameInstallPathMarkers);
    }

    internal static bool LooksLikeStoreGamePackageName(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return false;
        }

        if (!packageName.StartsWith(StoreGamePackagePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var suffix = packageName[StoreGamePackagePrefix.Length..];
        if (suffix.Length < MinStoreGamePackageSuffixLength)
        {
            return false;
        }

        foreach (var character in suffix)
        {
            if (!Uri.IsHexDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasExactStartApp(
        IReadOnlyDictionary<string, string> startAppNames,
        string appUserModelId)
    {
        return !string.IsNullOrWhiteSpace(appUserModelId)
            && startAppNames.ContainsKey(appUserModelId);
    }

    private static bool HasStartAppsForPackage(
        string packageFamilyName,
        IReadOnlyDictionary<string, string> startAppNames)
    {
        if (string.IsNullOrWhiteSpace(packageFamilyName) || startAppNames.Count == 0)
        {
            return false;
        }

        var prefix = packageFamilyName + "!";
        foreach (var startApp in startAppNames)
        {
            if (!startApp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var appId = startApp.Key.Length > prefix.Length
                ? startApp.Key[prefix.Length..]
                : string.Empty;
            var displayName = startApp.Value?.Trim() ?? string.Empty;
            if (ShouldSuppressInfrastructureApp(displayName, packageFamilyName, appId))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    internal static bool ShouldSuppressInfrastructureApp(string displayName, string packageName, string appId)
    {
        return HasInfrastructureToken(displayName)
            || HasInfrastructureToken(packageName)
            || HasInfrastructureToken(appId);
    }

    private static bool HasInfrastructureToken(string value)
    {
        return ContainsAnyToken(value, InfrastructureSuppressTokens);
    }

    private static bool ContainsXboxToken(string value)
    {
        return ContainsAnyToken(value, XboxOnlyTokens);
    }

    private static bool ContainsAnyToken(string value, IReadOnlyList<string> tokens)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (value.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static List<AppxPackage> LoadPackages()
    {
        const string command = "Get-AppxPackage | Select-Object Name,PackageFamilyName,InstallLocation,IsFramework | ConvertTo-Json -Compress";
        var output = RunPowerShellCommand(command);
        if (string.IsNullOrWhiteSpace(output))
        {
            return new List<AppxPackage>();
        }

        return ParsePackageJson(output);
    }

    private static List<AppxPackage> ParsePackageJson(string json)
    {
        var packages = new List<AppxPackage>();

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch
        {
            return packages;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (TryParsePackage(item, out var package))
                    {
                        packages.Add(package);
                    }
                }

                return packages;
            }

            if (root.ValueKind == JsonValueKind.Object && TryParsePackage(root, out var singlePackage))
            {
                packages.Add(singlePackage);
            }
        }

        return packages;
    }

    private static IReadOnlyDictionary<string, string> LoadStartAppNames()
    {
        const string command = "Get-StartApps | Select-Object Name,AppID | ConvertTo-Json -Compress";
        var output = RunPowerShellCommand(command);
        if (string.IsNullOrWhiteSpace(output))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return ParseStartAppsJson(output);
    }

    internal static IReadOnlyDictionary<string, string> ParseStartAppsJson(string json)
    {
        var startApps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch
        {
            return startApps;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    TryAddStartApp(item, startApps);
                }

                return startApps;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                TryAddStartApp(root, startApps);
            }
        }

        return startApps;
    }

    private static void TryAddStartApp(JsonElement element, Dictionary<string, string> startApps)
    {
        if (!TryGetString(element, "AppID", out var appId)
            || !TryGetString(element, "Name", out var name))
        {
            return;
        }

        if (!startApps.ContainsKey(appId))
        {
            startApps.Add(appId, name.Trim());
        }
    }

    private static bool TryParsePackage(JsonElement element, out AppxPackage package)
    {
        package = default;

        if (!TryGetString(element, "Name", out var name)
            || !TryGetString(element, "PackageFamilyName", out var familyName)
            || !TryGetString(element, "InstallLocation", out var installLocation))
        {
            return false;
        }

        var isFramework = false;
        if (element.TryGetProperty("IsFramework", out var frameworkProperty)
            && frameworkProperty.ValueKind == JsonValueKind.True)
        {
            isFramework = true;
        }

        package = new AppxPackage(name, familyName, installLocation, isFramework);
        return true;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string RunPowerShellCommand(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return string.Empty;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }

    private readonly record struct AppxPackage(
        string Name,
        string PackageFamilyName,
        string InstallLocation,
        bool IsFramework);

    private readonly record struct AppManifestApplication(
        string ApplicationId,
        string DisplayName,
        IReadOnlyList<string> CategoryPath,
        bool IsGame,
        AppBrowseTitleMetadata TitleMetadata);

    private readonly record struct ResolvedTitle(
        string DisplayName,
        AppBrowseTitleMetadata TitleMetadata);
}
