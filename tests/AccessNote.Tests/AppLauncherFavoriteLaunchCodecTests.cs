using AccessNote;

namespace AccessNote.Tests;

public sealed class AppLauncherFavoriteLaunchCodecTests
{
    [Fact]
    public void TryParse_LegacyPath_UsesDirectPathTarget()
    {
        var stored = @"C:\\Apps\\calc.exe";

        var parsed = AppLauncherFavoriteLaunchCodec.TryParse(stored, out var launchSpec);

        Assert.True(parsed);
        Assert.Equal(LaunchTargetType.DirectPath, launchSpec.TargetType);
        Assert.Equal(stored, launchSpec.Target);
    }

    [Fact]
    public void EncodeAndParse_ShellApp_RoundTrips()
    {
        var spec = LaunchSpec.ShellApp("Contoso.App_123!App");

        var encoded = AppLauncherFavoriteLaunchCodec.Encode(spec);
        var parsed = AppLauncherFavoriteLaunchCodec.TryParse(encoded, out var decoded);

        Assert.True(parsed);
        Assert.Equal(LaunchTargetType.ShellApp, decoded.TargetType);
        Assert.Equal("Contoso.App_123!App", decoded.Target);
    }

    [Fact]
    public void IdentityKey_DirectPaths_AreCaseInsensitive()
    {
        var left = AppLauncherFavoriteLaunchCodec.GetIdentityKey(@"C:\\Apps\\Calc.exe");
        var right = AppLauncherFavoriteLaunchCodec.GetIdentityKey(@"c:\\apps\\calc.exe");

        Assert.Equal(left, right);
    }
}
