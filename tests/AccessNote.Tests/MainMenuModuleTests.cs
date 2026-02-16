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
                var utilityChild = MainMenuEntry.ForApplet(new AppletDescriptor(AppletId.Notes, "Placeholder Utility"));
                var entries = new List<MainMenuEntry>
                {
                    MainMenuEntry.ForApplet(new AppletDescriptor(AppletId.Notes, "Notes")),
                    MainMenuEntry.ForApplet(new AppletDescriptor(AppletId.Settings, "Settings")),
                    MainMenuEntry.Submenu("Utilities", new[] { utilityChild }),
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

                // Activate Notes
                module.ShowMainMenu(0, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                // Activate Settings
                module.ShowMainMenu(1, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                // Activate Utilities submenu (enters submenu)
                module.ShowMainMenu(2, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                // Activate Exit
                module.ShowMainMenu(3, shouldAnnounce: false);
                Assert.True(module.HandleInput(Key.Enter));

                Assert.Equal(1, notesOpenCount);
                Assert.Equal(1, settingsOpenCount);
                Assert.Equal(1, exitPromptCount);
                Assert.Contains(announcements, a => a.Contains("Utilities"));
            });
            });
    }

    private static ShellViewAdapter CreateShellView(
        IReadOnlyList<MainMenuEntry> entries,
        out ListBox mainMenuList)
    {
        var mainMenuScreen = new Grid();
        mainMenuList = new ListBox
        {
            ItemsSource = entries,
        };

        return new ShellViewAdapter(
            mainMenuScreen: mainMenuScreen,
            mainMenuList: mainMenuList,
            dispatcher: Dispatcher.CurrentDispatcher);
    }
}
