using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AccessNote;

namespace AccessNote.Tests;

public sealed class MainMenuModuleTests
{
    [Fact]
    public void HandleInput_Enter_ActivatesTypedMainMenuEntries()
    {
        StaTestRunner.Run(
            () =>
            {
                var entries = new List<MainMenuEntry>
                {
                    MainMenuEntry.ForApplet(new AppletDescriptor(AppletId.Notes, "Notes")),
                    MainMenuEntry.ForApplet(new AppletDescriptor(AppletId.Settings, "Settings")),
                    MainMenuEntry.Utilities(),
                    MainMenuEntry.Exit(),
                };

                var shellView = CreateShellView(entries, out var mainMenuList);
                var notesOpenCount = 0;
                var settingsOpenCount = 0;
                var exitPromptCount = 0;
                var announcements = new List<string>();

                var module = new MainMenuModule(
                    shellView: shellView,
                    entries: entries,
                    openApplet: appletId =>
                    {
                        if (appletId == AppletId.Notes)
                        {
                            notesOpenCount++;
                        }
                        else if (appletId == AppletId.Settings)
                        {
                            settingsOpenCount++;
                        }
                    },
                    showExitPrompt: () => exitPromptCount++,
                    announce: announcements.Add);

                module.ShowMainMenu(0, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                module.ShowMainMenu(1, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                module.ShowMainMenu(2, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                module.ShowMainMenu(3, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                Assert.Equal(1, notesOpenCount);
                Assert.Equal(1, settingsOpenCount);
                Assert.Equal(1, exitPromptCount);
                Assert.Contains("Utilities is not implemented yet.", announcements);
                Assert.Equal(3, mainMenuList.SelectedIndex);
            });
    }

    private static ShellViewAdapter CreateShellView(
        IReadOnlyList<MainMenuEntry> entries,
        out ListBox mainMenuList)
    {
        var mainMenuScreen = new Grid();
        var notesScreen = new Grid();
        var settingsScreen = new Grid();
        mainMenuList = new ListBox
        {
            ItemsSource = entries,
        };

        return new ShellViewAdapter(
            mainMenuScreen: mainMenuScreen,
            notesScreen: notesScreen,
            settingsScreen: settingsScreen,
            mainMenuList: mainMenuList,
            dispatcher: Dispatcher.CurrentDispatcher);
    }
}
