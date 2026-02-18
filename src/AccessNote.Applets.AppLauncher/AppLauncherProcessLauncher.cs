using System;
using System.Diagnostics;

namespace AccessNote;

internal interface IAppLaunchService
{
    bool TryLaunch(LaunchSpec launchSpec, out string errorMessage);
}

internal sealed class AppLauncherProcessLauncher : IAppLaunchService
{
    public bool TryLaunch(LaunchSpec launchSpec, out string errorMessage)
    {
        try
        {
            var startInfo = CreateStartInfo(launchSpec);
            Process.Start(startInfo);
            errorMessage = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private static ProcessStartInfo CreateStartInfo(LaunchSpec launchSpec)
    {
        return launchSpec.TargetType switch
        {
            LaunchTargetType.DirectPath => new ProcessStartInfo
            {
                FileName = launchSpec.Target,
                Arguments = launchSpec.Arguments,
                UseShellExecute = true,
            },
            LaunchTargetType.ShellApp => new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"shell:AppsFolder\\{launchSpec.Target}",
                UseShellExecute = true,
            },
            LaunchTargetType.Uri => new ProcessStartInfo
            {
                FileName = launchSpec.Target,
                UseShellExecute = true,
            },
            _ => throw new InvalidOperationException("Unsupported launch target type."),
        };
    }
}
