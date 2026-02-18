using System;
using AccessNote;

namespace AccessNote.Tests;

public sealed class AppBrowseNavigatorTests
{
    [Fact]
    public void NavigateIntoFolder_AndBackToRoot_Works()
    {
        var root = AppBrowseNode.CreateFolder("Browse");
        var sourceFolder = AppBrowseNode.CreateFolder("Start Menu");
        sourceFolder.AddChild(AppBrowseNode.CreateApp(
            name: "Calculator",
            launchSpec: LaunchSpec.DirectPath(@"C:\\Apps\\calc.exe"),
            detailPath: @"C:\\Apps\\calc.exe",
            detailArguments: string.Empty,
            sourceLabel: "Start Menu",
            titleMetadata: AppBrowseTitleMetadata.Unknown));
        root.AddChild(sourceFolder);

        var navigator = new AppBrowseNavigator();
        navigator.Load(root);

        Assert.True(navigator.TryEnterFolder(0, out var enteredFolder));
        Assert.Equal("Start Menu", enteredFolder);
        Assert.Single(navigator.CurrentEntries);
        Assert.Equal("Calculator", navigator.CurrentEntries[0].Name);

        Assert.True(navigator.TryNavigateUp(out var parentName));
        Assert.Equal("Browse", parentName);
        Assert.Single(navigator.CurrentEntries);
        Assert.Equal("Start Menu", navigator.CurrentEntries[0].Name);
    }

    [Fact]
    public void NavigateUp_AtRoot_DoesNothing()
    {
        var navigator = new AppBrowseNavigator();
        navigator.Load(AppBrowseNode.CreateFolder("Browse"));

        Assert.False(navigator.TryNavigateUp(out _));
    }

    [Fact]
    public void RestorePath_KeepsDeepFolderWhenAvailable()
    {
        var root = AppBrowseNode.CreateFolder("Browse");
        var sourceFolder = AppBrowseNode.CreateFolder("Steam");
        var gamesFolder = AppBrowseNode.CreateFolder("Games");
        sourceFolder.AddChild(gamesFolder);
        root.AddChild(sourceFolder);

        var navigator = new AppBrowseNavigator();
        navigator.Load(root);

        navigator.RestorePath("Steam\\Games");

        Assert.Empty(navigator.CurrentEntries);
        Assert.Equal("Steam\\Games", navigator.GetCurrentPathDisplay());
    }
}
