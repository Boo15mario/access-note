using System;
using System.Collections.Generic;

namespace AccessNote;

internal enum LaunchTargetType
{
    DirectPath,
    ShellApp,
    Uri
}

internal enum TitleConfidence
{
    Unknown,
    Verified,
    Inferred,
    Fallback
}

internal readonly record struct AppBrowseTitleMetadata(
    TitleConfidence Confidence,
    string Provenance)
{
    public static AppBrowseTitleMetadata Unknown => new(TitleConfidence.Unknown, string.Empty);
}

internal readonly record struct LaunchSpec(LaunchTargetType TargetType, string Target, string Arguments = "")
{
    public static LaunchSpec DirectPath(string path, string? arguments = null)
    {
        return new LaunchSpec(LaunchTargetType.DirectPath, path, arguments ?? string.Empty);
    }

    public static LaunchSpec ShellApp(string appUserModelId)
    {
        return new LaunchSpec(LaunchTargetType.ShellApp, appUserModelId, string.Empty);
    }

    public static LaunchSpec Uri(string uri)
    {
        return new LaunchSpec(LaunchTargetType.Uri, uri, string.Empty);
    }
}

internal readonly record struct AppBrowseSourceEntry(
    string DisplayName,
    IReadOnlyList<string> CategoryPath,
    LaunchSpec LaunchSpec,
    string DetailPath,
    AppBrowseTitleMetadata? TitleMetadata = null,
    bool IsGame = false);

internal interface IAppBrowseSource
{
    string RootLabel { get; }

    IReadOnlyList<AppBrowseSourceEntry> Discover();
}
