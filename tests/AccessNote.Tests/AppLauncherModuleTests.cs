using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccessNote;

namespace AccessNote.Tests;

public sealed class AppLauncherModuleTests
{
    [Fact]
    public void AddButton_AddsFavorite_WhenDialogReturnsLaunchablePath()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var favoritePath = CreateLaunchableFile(testRoot, "Calculator.exe");
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    var dialog = new StubDialogService
                    {
                        ShouldReturnResult = true,
                        Result = new AddFavoriteResult(
                            Source: AddFavoriteSource.Browse,
                            Path: favoritePath,
                            DisplayName: "Calculator")
                    };
                    var announcements = new List<string>();
                    var module = new AppLauncherModule(storage, dialog, announcements.Add);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    controls.AddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    var favorites = storage.GetAll();
                    var favorite = Assert.Single(favorites);
                    Assert.Equal(favoritePath, favorite.Path);
                    Assert.Contains("Added Calculator to favorites.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void AddButton_AnnouncesDuplicate_WhenFavoriteAlreadyExists()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var favoritePath = CreateLaunchableFile(testRoot, "Calculator.exe");
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    storage.Add("Calculator", favoritePath);

                    var dialog = new StubDialogService
                    {
                        ShouldReturnResult = true,
                        Result = new AddFavoriteResult(
                            Source: AddFavoriteSource.Browse,
                            Path: favoritePath,
                            DisplayName: "Calculator")
                    };
                    var announcements = new List<string>();
                    var module = new AppLauncherModule(storage, dialog, announcements.Add);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    controls.AddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    Assert.Single(storage.GetAll());
                    Assert.Contains("Calculator is already in favorites.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void RemoveButton_AnnouncesWhenNoSelection()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var favoritePath = CreateLaunchableFile(testRoot, "Calculator.exe");
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    storage.Add("Calculator", favoritePath);

                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var module = new AppLauncherModule(storage, dialog, announcements.Add);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    controls.AppList.SelectedIndex = -1;
                    controls.RemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    Assert.Single(storage.GetAll());
                    Assert.Contains("No favorite selected.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void RemoveButton_RemovesSelectedFavorite()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var favoritePath = CreateLaunchableFile(testRoot, "Calculator.exe");
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    storage.Add("Calculator", favoritePath);

                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var module = new AppLauncherModule(storage, dialog, announcements.Add);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    controls.RemoveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    Assert.Empty(storage.GetAll());
                    Assert.Contains("Removed Calculator from favorites.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void BrowseMode_TypeFilter_FiltersCurrentFolderAndAnnouncesCount()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var browseSources = new IAppBrowseSource[]
                    {
                        new StubBrowseSource(
                            rootLabel: "Start Menu",
                            entries:
                            [
                                new AppBrowseSourceEntry(
                                    DisplayName: "Alpha Tool",
                                    CategoryPath: Array.Empty<string>(),
                                    LaunchSpec: LaunchSpec.Uri("custom://alpha"),
                                    DetailPath: "custom://alpha"),
                                new AppBrowseSourceEntry(
                                    DisplayName: "Beta Tool",
                                    CategoryPath: Array.Empty<string>(),
                                    LaunchSpec: LaunchSpec.Uri("custom://beta"),
                                    DetailPath: "custom://beta"),
                            ]),
                    };
                    var module = new AppLauncherModule(storage, dialog, announcements.Add, browseSources: browseSources);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    Assert.True(module.HandleInput(Key.Tab, ModifierKeys.None));
                    Assert.True(module.HandleInput(Key.Enter, ModifierKeys.None));
                    Assert.True(module.HandleInput(Key.B, ModifierKeys.None));

                    Assert.Single(controls.AppList.Items);
                    Assert.Equal("Beta Tool", controls.AppList.Items[0]);
                    Assert.Contains("Filter b. 1 items.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void BrowseMode_Insert_AddsCurrentSelectionToFavorites()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var browseSources = new IAppBrowseSource[]
                    {
                        new StubBrowseSource(
                            rootLabel: "Steam",
                            entries:
                            [
                                new AppBrowseSourceEntry(
                                    DisplayName: "Steam Sample",
                                    CategoryPath: Array.Empty<string>(),
                                    LaunchSpec: LaunchSpec.Uri("steam://rungameid/10"),
                                    DetailPath: "steam://rungameid/10"),
                            ]),
                    };
                    var module = new AppLauncherModule(storage, dialog, announcements.Add, browseSources: browseSources);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    Assert.True(module.HandleInput(Key.Tab, ModifierKeys.None));
                    Assert.True(module.HandleInput(Key.Enter, ModifierKeys.None));
                    Assert.True(module.HandleInput(Key.Insert, ModifierKeys.None));

                    var favorite = Assert.Single(storage.GetAll());
                    Assert.Equal("Steam Sample", favorite.DisplayName);
                    Assert.Equal("uri:steam://rungameid/10", favorite.Path);
                    Assert.Contains("Added Steam Sample to favorites.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void BrowseDetails_IncludeSourceAndTitleConfidence()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var browseSources = new IAppBrowseSource[]
                    {
                        new StubBrowseSource(
                            rootLabel: "Heroic",
                            entries:
                            [
                                new AppBrowseSourceEntry(
                                    DisplayName: "Hogwarts Legacy",
                                    CategoryPath: Array.Empty<string>(),
                                    LaunchSpec: LaunchSpec.Uri("heroic://launch/test"),
                                    DetailPath: "heroic://launch/test",
                                    TitleMetadata: new AppBrowseTitleMetadata(
                                        Confidence: TitleConfidence.Verified,
                                        Provenance: "Heroic title metadata")),
                            ]),
                    };
                    var module = new AppLauncherModule(storage, dialog, announcements.Add, browseSources: browseSources);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    Assert.True(module.HandleInput(Key.Tab, ModifierKeys.None));
                    Assert.True(module.HandleInput(Key.Enter, ModifierKeys.None));

                    Assert.Contains("Source: Heroic", controls.DetailArguments.Text);
                    Assert.Contains("Title: verified (Heroic title metadata)", controls.DetailArguments.Text);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    [Fact]
    public void BrowseMode_EnterGamesFolder_ShowsAggregatedGameTitles()
    {
        StaTestRunner.Run(
            () =>
            {
                var testRoot = CreateTestDirectory();
                try
                {
                    var storage = new FavoriteAppStorage(Path.Combine(testRoot, "favorites.db"));
                    var dialog = new StubDialogService();
                    var announcements = new List<string>();
                    var browseSources = new IAppBrowseSource[]
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
                    var module = new AppLauncherModule(storage, dialog, announcements.Add, browseSources: browseSources);

                    var controls = CreateControls();
                    module.Enter(
                        controls.ModeText,
                        controls.AppList,
                        controls.DetailName,
                        controls.DetailPath,
                        controls.DetailArguments,
                        controls.AddButton,
                        controls.RemoveButton,
                        controls.LaunchButton);

                    Assert.True(module.HandleInput(Key.Tab, ModifierKeys.None));

                    var gamesIndex = -1;
                    for (var index = 0; index < controls.AppList.Items.Count; index++)
                    {
                        if (!string.Equals(controls.AppList.Items[index]?.ToString(), "[Games]", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        gamesIndex = index;
                        break;
                    }

                    Assert.True(gamesIndex >= 0);
                    controls.AppList.SelectedIndex = gamesIndex;
                    Assert.True(module.HandleInput(Key.Enter, ModifierKeys.None));

                    var gameEntries = controls.AppList.Items.Cast<object>().Select(item => item?.ToString() ?? string.Empty).ToList();
                    Assert.Contains("Bop It! The Video Game", gameEntries);
                    Assert.Contains("Death Squared", gameEntries);
                    Assert.DoesNotContain("Calculator", gameEntries);
                    Assert.Contains("Games. 2 items.", announcements);

                    module.CanLeave();
                }
                finally
                {
                    DeleteTestDirectory(testRoot);
                }
            });
    }

    private static (
        TextBlock ModeText,
        ListBox AppList,
        TextBlock DetailName,
        TextBlock DetailPath,
        TextBlock DetailArguments,
        Button AddButton,
        Button RemoveButton,
        Button LaunchButton) CreateControls()
    {
        return (
            ModeText: new TextBlock(),
            AppList: new ListBox(),
            DetailName: new TextBlock(),
            DetailPath: new TextBlock(),
            DetailArguments: new TextBlock(),
            AddButton: new Button(),
            RemoveButton: new Button(),
            LaunchButton: new Button());
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "AccessNoteTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteTestDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
                return;
            }
            catch (IOException)
            {
                if (attempt == 4)
                {
                    return;
                }

                System.Threading.Thread.Sleep(40);
            }
            catch (UnauthorizedAccessException)
            {
                if (attempt == 4)
                {
                    return;
                }

                System.Threading.Thread.Sleep(40);
            }
        }
    }

    private static string CreateLaunchableFile(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, "");
        return path;
    }

    private sealed class StubDialogService : IAppLauncherDialogService
    {
        public bool ShouldReturnResult { get; init; }

        public AddFavoriteResult Result { get; init; }

        public bool TryPromptAddFavorite(AddFavoriteRequest request, out AddFavoriteResult result)
        {
            result = Result;
            return ShouldReturnResult;
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
