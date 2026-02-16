using AccessNote;
using System.Windows.Input;

namespace AccessNote.Tests;

public sealed class ScreenRouterTests
{
    [Fact]
    public void OpenApplet_Notes_SetsActiveAppletId_AndInvokesCallback()
    {
        var notesEnterCalls = 0;
        var router = CreateRouter(
            notesApplet: CreateApplet(
                appletId: AppletId.Notes,
                enter: () => notesEnterCalls++,
                restoreFocus: () => { },
                canLeave: () => true),
            settingsApplet: CreateApplet(
                appletId: AppletId.Settings,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () => true),
            showMainMenu: (_, _) => { },
            restoreMainMenuFocus: () => { });

        router.OpenApplet(AppletId.Notes);

        Assert.Equal(AppletId.Notes, router.ActiveAppletId);
        Assert.Equal(1, notesEnterCalls);
    }

    [Fact]
    public void ShowMainMenu_ClearsActiveApplet_AndPassesArguments()
    {
        var focusIndex = -1;
        var shouldAnnounce = false;
        var router = CreateRouter(
            notesApplet: CreateApplet(
                appletId: AppletId.Notes,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () => true),
            settingsApplet: CreateApplet(
                appletId: AppletId.Settings,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () => true),
            showMainMenu: (index, announce) =>
            {
                focusIndex = index;
                shouldAnnounce = announce;
            },
            restoreMainMenuFocus: () => { });

        router.OpenApplet(AppletId.Notes);
        router.ShowMainMenu(2, shouldAnnounce: true);

        Assert.Null(router.ActiveAppletId);
        Assert.Equal(2, focusIndex);
        Assert.True(shouldAnnounce);
    }

    [Fact]
    public void CanLeaveActiveScreen_UsesScreenSpecificGuards()
    {
        var notesChecks = 0;
        var settingsChecks = 0;
        var router = CreateRouter(
            notesApplet: CreateApplet(
                appletId: AppletId.Notes,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () =>
                {
                    notesChecks++;
                    return false;
                }),
            settingsApplet: CreateApplet(
                appletId: AppletId.Settings,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () =>
                {
                    settingsChecks++;
                    return true;
                }),
            showMainMenu: (_, _) => { },
            restoreMainMenuFocus: () => { });

        router.OpenApplet(AppletId.Notes);
        var canLeaveNotes = router.CanLeaveActiveScreen();

        router.OpenApplet(AppletId.Settings);
        var canLeaveSettings = router.CanLeaveActiveScreen();

        router.ShowMainMenu(0, shouldAnnounce: false);
        var canLeaveMainMenu = router.CanLeaveActiveScreen();

        Assert.False(canLeaveNotes);
        Assert.True(canLeaveSettings);
        Assert.True(canLeaveMainMenu);
        Assert.Equal(1, notesChecks);
        Assert.Equal(1, settingsChecks);
    }

    [Fact]
    public void RestoreFocusForActiveScreen_UsesMatchingCallback()
    {
        var restored = string.Empty;
        var router = CreateRouter(
            notesApplet: CreateApplet(
                appletId: AppletId.Notes,
                enter: () => { },
                restoreFocus: () => restored = "notes",
                canLeave: () => true),
            settingsApplet: CreateApplet(
                appletId: AppletId.Settings,
                enter: () => { },
                restoreFocus: () => restored = "settings",
                canLeave: () => true),
            showMainMenu: (_, _) => { },
            restoreMainMenuFocus: () => restored = "main");

        router.ShowMainMenu(0, shouldAnnounce: false);
        router.RestoreFocusForActiveScreen();
        Assert.Equal("main", restored);

        router.OpenApplet(AppletId.Notes);
        router.RestoreFocusForActiveScreen();
        Assert.Equal("notes", restored);

        router.OpenApplet(AppletId.Settings);
        router.RestoreFocusForActiveScreen();
        Assert.Equal("settings", restored);
    }

    [Fact]
    public void HandleInputForActiveApplet_DelegatesToCurrentApplet()
    {
        var router = CreateRouter(
            notesApplet: CreateApplet(
                appletId: AppletId.Notes,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () => true,
                handleInput: (_, key, _) => key == Key.F2),
            settingsApplet: CreateApplet(
                appletId: AppletId.Settings,
                enter: () => { },
                restoreFocus: () => { },
                canLeave: () => true,
                handleInput: (_, _, _) => false),
            showMainMenu: (_, _) => { },
            restoreMainMenuFocus: () => { });

        router.OpenApplet(AppletId.Notes);
        var handledFromNotes = router.HandleInputForActiveApplet(
            e: null!,
            key: Key.F2,
            modifiers: ModifierKeys.None);

        router.OpenApplet(AppletId.Settings);
        var handledFromSettings = router.HandleInputForActiveApplet(
            e: null!,
            key: Key.F2,
            modifiers: ModifierKeys.None);

        Assert.True(handledFromNotes);
        Assert.False(handledFromSettings);
    }

    private static ScreenRouter CreateRouter(
        IApplet notesApplet,
        IApplet settingsApplet,
        Action<int, bool> showMainMenu,
        Action restoreMainMenuFocus)
    {
        var registry = new AppletRegistry(new[] { notesApplet, settingsApplet });
        return new ScreenRouter(
            registry,
            showMainMenu,
            restoreMainMenuFocus);
    }

    private static IApplet CreateApplet(
        AppletId appletId,
        Action enter,
        Action restoreFocus,
        Func<bool> canLeave,
        Func<KeyEventArgs, Key, ModifierKeys, bool>? handleInput = null)
    {
        return new TestApplet(
            appletId,
            enter,
            restoreFocus,
            canLeave,
            handleInput ?? ((_, _, _) => false));
    }

    private sealed class TestApplet : IApplet
    {
        private readonly Action _enter;
        private readonly Action _restoreFocus;
        private readonly Func<bool> _canLeave;
        private readonly Func<KeyEventArgs, Key, ModifierKeys, bool> _handleInput;

        public TestApplet(
            AppletId appletId,
            Action enter,
            Action restoreFocus,
            Func<bool> canLeave,
            Func<KeyEventArgs, Key, ModifierKeys, bool> handleInput)
        {
            _enter = enter;
            _restoreFocus = restoreFocus;
            _canLeave = canLeave;
            _handleInput = handleInput;
            Descriptor = new AppletDescriptor(appletId, appletId.ToString());
        }

        public AppletDescriptor Descriptor { get; }

        public void Enter()
        {
            _enter();
        }

        public void RestoreFocus()
        {
            _restoreFocus();
        }

        public bool CanLeave()
        {
            return _canLeave();
        }

        public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
        {
            return _handleInput(e, key, modifiers);
        }
    }
}
