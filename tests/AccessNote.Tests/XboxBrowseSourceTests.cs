using System.Collections.Generic;
using AccessNote;

namespace AccessNote.Tests;

public sealed class XboxBrowseSourceTests
{
    [Fact]
    public void ParseStartAppsJson_Array_ParsesEntries()
    {
        const string json = "[{\"Name\":\"As Dusk Falls\",\"AppID\":\"Microsoft.3020BF20E956_8wekyb3d8bbwe!Game\"}]";

        var parsed = XboxBrowseSource.ParseStartAppsJson(json);

        Assert.Single(parsed);
        Assert.Equal("As Dusk Falls", parsed["Microsoft.3020BF20E956_8wekyb3d8bbwe!Game"]);
    }

    [Fact]
    public void ParseStartAppsJson_Object_ParsesSingleEntry()
    {
        const string json = "{\"Name\":\"Xbox\",\"AppID\":\"Microsoft.GamingApp_8wekyb3d8bbwe!Microsoft.Xbox.App\"}";

        var parsed = XboxBrowseSource.ParseStartAppsJson(json);

        Assert.Single(parsed);
        Assert.Equal("Xbox", parsed["Microsoft.GamingApp_8wekyb3d8bbwe!Microsoft.Xbox.App"]);
    }

    [Fact]
    public void ResolveDisplayName_PrefersExactStartAppsName()
    {
        var startApps = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Microsoft.3020BF20E956_8wekyb3d8bbwe!Game"] = "As Dusk Falls",
        };

        var displayName = XboxBrowseSource.ResolveDisplayName(
            manifestDisplayName: "ms-resource:AppName",
            packageName: "Microsoft.3020BF20E956",
            appId: "Game",
            appUserModelId: "Microsoft.3020BF20E956_8wekyb3d8bbwe!Game",
            startAppNames: startApps);

        Assert.Equal("As Dusk Falls", displayName);
    }

    [Fact]
    public void ResolveDisplayName_UsesSinglePackageStartAppsName_WhenExactMissing()
    {
        var startApps = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Contoso.Game_8wekyb3d8bbwe!App"] = "Contoso Game",
        };

        var displayName = XboxBrowseSource.ResolveDisplayName(
            manifestDisplayName: "ms-resource:DisplayName",
            packageName: "Contoso.Game",
            appId: "Launch",
            appUserModelId: "Contoso.Game_8wekyb3d8bbwe!Launch",
            startAppNames: startApps);

        Assert.Equal("Contoso Game", displayName);
    }

    [Fact]
    public void ResolveDisplayName_FallsBackWhenPackageHasConflictingStartAppsNames()
    {
        var startApps = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Contoso.Game_8wekyb3d8bbwe!App"] = "Contoso Game",
            ["Contoso.Game_8wekyb3d8bbwe!Editor"] = "Contoso Editor",
        };

        var displayName = XboxBrowseSource.ResolveDisplayName(
            manifestDisplayName: "ms-resource:DisplayName",
            packageName: "Contoso.Game",
            appId: "Launch",
            appUserModelId: "Contoso.Game_8wekyb3d8bbwe!Launch",
            startAppNames: startApps);

        Assert.Equal("Contoso Game Launch", displayName);
    }

    [Fact]
    public void ShouldSuppressInfrastructureApp_ReturnsTrue_ForGamingServices()
    {
        var suppressed = XboxBrowseSource.ShouldSuppressInfrastructureApp(
            displayName: "Gaming Services",
            packageName: "Microsoft.GamingServices",
            appId: "Microsoft.GamingServices");

        Assert.True(suppressed);
    }

    [Fact]
    public void ShouldSuppressInfrastructureApp_ReturnsFalse_ForGameTitle()
    {
        var suppressed = XboxBrowseSource.ShouldSuppressInfrastructureApp(
            displayName: "Forza Motorsport",
            packageName: "Microsoft.ForzaMotorsport",
            appId: "App");

        Assert.False(suppressed);
    }

    [Fact]
    public void ShouldSuppressInfrastructureApp_ReturnsTrue_ForGameBar()
    {
        var suppressed = XboxBrowseSource.ShouldSuppressInfrastructureApp(
            displayName: "Game Bar",
            packageName: "Microsoft.XboxGamingOverlay",
            appId: "App");

        Assert.True(suppressed);
    }

    [Fact]
    public void ShouldSuppressInfrastructureApp_ReturnsTrue_ForMicrosoftDefender()
    {
        var suppressed = XboxBrowseSource.ShouldSuppressInfrastructureApp(
            displayName: "Microsoft Defender",
            packageName: "Microsoft.SecHealthUI",
            appId: "App");

        Assert.True(suppressed);
    }

    [Fact]
    public void ResolveDisplayName_UsesStartAppsMatch_ForTokenlessPackage()
    {
        var startApps = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Microsoft.624F8B84B80_8wekyb3d8bbwe!Forzahorizon5"] = "Forza Horizon 5",
        };

        var displayName = XboxBrowseSource.ResolveDisplayName(
            manifestDisplayName: "ms-resource:AppName",
            packageName: "Microsoft.624F8B84B80",
            appId: "Forzahorizon5",
            appUserModelId: "Microsoft.624F8B84B80_8wekyb3d8bbwe!Forzahorizon5",
            startAppNames: startApps);

        Assert.Equal("Forza Horizon 5", displayName);
    }

    [Fact]
    public void LooksLikeStoreGamePackageName_ReturnsTrue_ForHexMicrosoftPackageName()
    {
        var looksLikeGamePackage = XboxBrowseSource.LooksLikeStoreGamePackageName("Microsoft.624F8B84B80");

        Assert.True(looksLikeGamePackage);
    }

    [Fact]
    public void LooksLikeStoreGamePackageName_ReturnsFalse_ForSystemPackageName()
    {
        var looksLikeGamePackage = XboxBrowseSource.LooksLikeStoreGamePackageName("MicrosoftWindows.Client.CBS");

        Assert.False(looksLikeGamePackage);
    }
}
