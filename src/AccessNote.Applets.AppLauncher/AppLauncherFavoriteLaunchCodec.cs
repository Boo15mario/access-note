using System;
using System.IO;

namespace AccessNote;

internal static class AppLauncherFavoriteLaunchCodec
{
    private const string ShellAppPrefix = "shellapp:";
    private const string UriPrefix = "uri:";

    public static string Encode(LaunchSpec launchSpec)
    {
        return launchSpec.TargetType switch
        {
            LaunchTargetType.DirectPath => launchSpec.Target,
            LaunchTargetType.ShellApp => ShellAppPrefix + launchSpec.Target,
            LaunchTargetType.Uri => UriPrefix + launchSpec.Target,
            _ => launchSpec.Target,
        };
    }

    public static bool TryParse(string storedTarget, out LaunchSpec launchSpec)
    {
        launchSpec = default;

        if (string.IsNullOrWhiteSpace(storedTarget))
        {
            return false;
        }

        if (storedTarget.StartsWith(ShellAppPrefix, StringComparison.OrdinalIgnoreCase))
        {
            launchSpec = LaunchSpec.ShellApp(storedTarget[ShellAppPrefix.Length..]);
            return !string.IsNullOrWhiteSpace(launchSpec.Target);
        }

        if (storedTarget.StartsWith(UriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            launchSpec = LaunchSpec.Uri(storedTarget[UriPrefix.Length..]);
            return !string.IsNullOrWhiteSpace(launchSpec.Target);
        }

        launchSpec = LaunchSpec.DirectPath(storedTarget);
        return true;
    }

    public static string GetIdentityKey(LaunchSpec launchSpec)
    {
        return launchSpec.TargetType switch
        {
            LaunchTargetType.DirectPath => "direct:" + NormalizePath(launchSpec.Target),
            LaunchTargetType.ShellApp => "shellapp:" + NormalizeToken(launchSpec.Target),
            LaunchTargetType.Uri => "uri:" + NormalizeToken(launchSpec.Target),
            _ => "unknown:" + NormalizeToken(launchSpec.Target),
        };
    }

    public static string GetIdentityKey(string storedTarget)
    {
        if (TryParse(storedTarget, out var launchSpec))
        {
            return GetIdentityKey(launchSpec);
        }

        return "invalid:" + NormalizeToken(storedTarget);
    }

    private static string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path).Trim().ToLowerInvariant();
        }
        catch
        {
            return path.Trim().ToLowerInvariant();
        }
    }

    private static string NormalizeToken(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
