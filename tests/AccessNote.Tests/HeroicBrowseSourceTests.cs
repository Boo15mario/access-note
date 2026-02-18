using AccessNote;

namespace AccessNote.Tests;

public sealed class HeroicBrowseSourceTests
{
    [Fact]
    public void ResolveDisplayName_UsesFriendlyExplicitTitle()
    {
        var title = HeroicBrowseSource.ResolveDisplayName(
            explicitDisplayName: "Hogwarts Legacy",
            gameId: "fa4240e57a3c46b39f169041b7811293",
            installPath: @"D:\Heroic\HogwartsLegacy");

        Assert.Equal("Hogwarts Legacy", title);
    }

    [Fact]
    public void ResolveDisplayName_UsesInstallDirectory_WhenTitleLooksLikeIdentifier()
    {
        var title = HeroicBrowseSource.ResolveDisplayName(
            explicitDisplayName: "1566969207",
            gameId: "1566969207",
            installPath: @"D:\Heroic\Death Squared");

        Assert.Equal("Death Squared", title);
    }

    [Fact]
    public void ResolveDisplayName_UsesInstallDirectory_WhenTitleMissing()
    {
        var title = HeroicBrowseSource.ResolveDisplayName(
            explicitDisplayName: null,
            gameId: "amzn1.adg.product.3a202794-d9d0-47e7-8d0c-c7d5f05f5fda",
            installPath: @"D:\Heroic\SNK 40th Anniversary Collection");

        Assert.Equal("SNK 40th Anniversary Collection", title);
    }

    [Fact]
    public void ResolveDisplayName_FallsBackToGameId_WhenNoTitleOrInstallPath()
    {
        var title = HeroicBrowseSource.ResolveDisplayName(
            explicitDisplayName: null,
            gameId: "fa4240e57a3c46b39f169041b7811293",
            installPath: null);

        Assert.Equal("fa4240e57a3c46b39f169041b7811293", title);
    }
}
