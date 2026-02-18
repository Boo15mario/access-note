using System;
using System.Collections.Generic;
using System.Linq;
using AccessNote;

namespace AccessNote.Tests;

public sealed class AppBrowseCatalogBuilderTests
{
    [Fact]
    public void Build_DedupesLaunchTargetsAcrossSources()
    {
        var duplicateTarget = @"C:\\Apps\\calc.exe";
        var sources = new IAppBrowseSource[]
        {
            new StubBrowseSource(
                rootLabel: "Start Menu",
                entries:
                [
                    new AppBrowseSourceEntry(
                        DisplayName: "Calculator",
                        CategoryPath: Array.Empty<string>(),
                        LaunchSpec: LaunchSpec.DirectPath(duplicateTarget),
                        DetailPath: duplicateTarget),
                ]),
            new StubBrowseSource(
                rootLabel: "Steam",
                entries:
                [
                    new AppBrowseSourceEntry(
                        DisplayName: "Calc Duplicate",
                        CategoryPath: new[] { "Games" },
                        LaunchSpec: LaunchSpec.DirectPath(duplicateTarget),
                        DetailPath: duplicateTarget),
                    new AppBrowseSourceEntry(
                        DisplayName: "Game One",
                        CategoryPath: new[] { "Games" },
                        LaunchSpec: LaunchSpec.Uri("steam://rungameid/10"),
                        DetailPath: "steam://rungameid/10"),
                ]),
        };

        var builder = new AppBrowseCatalogBuilder();
        var root = builder.Build(sources);

        var allApps = Traverse(root).Where(node => !node.IsFolder).ToList();
        var duplicateMatches = allApps.Count(
            node => string.Equals(node.DetailPath, duplicateTarget, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(1, duplicateMatches);
        Assert.Contains(root.Children, node => node.Name == "Start Menu");
        Assert.Contains(root.Children, node => node.Name == "Steam");
    }

    [Fact]
    public void Build_SortsFoldersBeforeAppsAndThenAlphabetically()
    {
        var source = new StubBrowseSource(
            rootLabel: "Start Menu",
            entries:
            [
                new AppBrowseSourceEntry(
                    DisplayName: "zeta",
                    CategoryPath: Array.Empty<string>(),
                    LaunchSpec: LaunchSpec.DirectPath(@"C:\\Apps\\zeta.exe"),
                    DetailPath: @"C:\\Apps\\zeta.exe"),
                new AppBrowseSourceEntry(
                    DisplayName: "alpha",
                    CategoryPath: Array.Empty<string>(),
                    LaunchSpec: LaunchSpec.DirectPath(@"C:\\Apps\\alpha.exe"),
                    DetailPath: @"C:\\Apps\\alpha.exe"),
                new AppBrowseSourceEntry(
                    DisplayName: "inside",
                    CategoryPath: new[] { "Folder" },
                    LaunchSpec: LaunchSpec.DirectPath(@"C:\\Apps\\inside.exe"),
                    DetailPath: @"C:\\Apps\\inside.exe"),
            ]);

        var builder = new AppBrowseCatalogBuilder();
        var root = builder.Build(new[] { source });

        var startMenuNode = Assert.Single(root.Children);
        Assert.Equal("Start Menu", startMenuNode.Name);

        var children = startMenuNode.Children;
        Assert.Equal(3, children.Count);
        Assert.True(children[0].IsFolder);
        Assert.Equal("Folder", children[0].Name);
        Assert.Equal("alpha", children[1].Name);
        Assert.Equal("zeta", children[2].Name);
    }

    [Fact]
    public void Build_AddsUnifiedGamesFolderAcrossSources()
    {
        var sources = new IAppBrowseSource[]
        {
            new StubBrowseSource(
                rootLabel: "Start Menu",
                entries:
                [
                    new AppBrowseSourceEntry(
                        DisplayName: "Calculator",
                        CategoryPath: Array.Empty<string>(),
                        LaunchSpec: LaunchSpec.DirectPath(@"C:\\Apps\\calc.exe"),
                        DetailPath: @"C:\\Apps\\calc.exe"),
                ]),
            new StubBrowseSource(
                rootLabel: "Steam",
                entries:
                [
                    new AppBrowseSourceEntry(
                        DisplayName: "Steam",
                        CategoryPath: Array.Empty<string>(),
                        LaunchSpec: LaunchSpec.DirectPath(@"C:\\Steam\\steam.exe"),
                        DetailPath: @"C:\\Steam\\steam.exe"),
                    new AppBrowseSourceEntry(
                        DisplayName: "Bop It! The Video Game",
                        CategoryPath: new[] { "Games" },
                        LaunchSpec: LaunchSpec.Uri("steam://rungameid/112233"),
                        DetailPath: "steam://rungameid/112233",
                        IsGame: true),
                ]),
            new StubBrowseSource(
                rootLabel: "Heroic",
                entries:
                [
                    new AppBrowseSourceEntry(
                        DisplayName: "Death Squared",
                        CategoryPath: new[] { "Games" },
                        LaunchSpec: LaunchSpec.Uri("heroic://launch/death-squared"),
                        DetailPath: "heroic://launch/death-squared",
                        IsGame: true),
                ]),
        };

        var builder = new AppBrowseCatalogBuilder();
        var root = builder.Build(sources);

        var gamesRoot = Assert.Single(root.Children, node => node.Name == "Games");
        var gameNames = gamesRoot.Children.Select(node => node.Name).ToList();
        Assert.Contains("Bop It! The Video Game", gameNames);
        Assert.Contains("Death Squared", gameNames);
        Assert.DoesNotContain("Steam", gameNames);

        Assert.Contains(root.Children, node => node.Name == "Start Menu");
        Assert.Contains(root.Children, node => node.Name == "Steam");
        Assert.Contains(root.Children, node => node.Name == "Heroic");
    }

    [Fact]
    public void Build_UsesExplicitIsGameFlag_WhenCategoryPathIsEmpty()
    {
        var source = new StubBrowseSource(
            rootLabel: "Xbox",
            entries:
            [
                new AppBrowseSourceEntry(
                    DisplayName: "Forza Horizon",
                    CategoryPath: Array.Empty<string>(),
                    LaunchSpec: LaunchSpec.ShellApp("Microsoft.Forza_8wekyb3d8bbwe!App"),
                    DetailPath: "shell:AppsFolder\\Microsoft.Forza_8wekyb3d8bbwe!App",
                    IsGame: true),
            ]);

        var builder = new AppBrowseCatalogBuilder();
        var root = builder.Build(new[] { source });

        var gamesRoot = Assert.Single(root.Children, node => node.Name == "Games");
        Assert.Contains(gamesRoot.Children, node => node.Name == "Forza Horizon");
    }

    private static IEnumerable<AppBrowseNode> Traverse(AppBrowseNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in Traverse(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class StubBrowseSource : IAppBrowseSource
    {
        public StubBrowseSource(string rootLabel, IReadOnlyList<AppBrowseSourceEntry> entries)
        {
            RootLabel = rootLabel;
            Entries = entries;
        }

        public string RootLabel { get; }

        private IReadOnlyList<AppBrowseSourceEntry> Entries { get; }

        public IReadOnlyList<AppBrowseSourceEntry> Discover() => Entries;
    }
}
