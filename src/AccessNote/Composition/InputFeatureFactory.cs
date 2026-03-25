namespace AccessNote;

internal static class InputFeatureFactory
{
    public static ShellInputController CreateController(
        StatusAnnouncer statusAnnouncer,
        ScreenRouter screenRouter,
        HomeScreenModule mainMenuModule,
        ShellNavigationController navigationController,
        ShellDialogService dialogService)
    {
        return new ShellInputController(
            getActiveAppletId: () => screenRouter.ActiveAppletId,
            handleHomeScreen: mainMenuModule.HandleInput,
            handleActiveAppletInput: screenRouter.HandleInputForActiveApplet,
            showExitPrompt: navigationController.ShowExitPrompt,
            getHelpText: navigationController.GetHelpText,
            announce: statusAnnouncer.Announce,
            returnToHomeScreen: () => navigationController.ShowHomeScreen(0, shouldAnnounce: true),
            showContextMenu: () =>
            {
                var appletId = screenRouter.ActiveAppletId;
                var appletName = appletId.HasValue ? appletId.Value.ToString() : "Home Screen";
                var helpText = navigationController.GetHelpText();

                var menuItems = new List<string> { "Help", "Back to Home Screen", "Exit" };

                // If not on Settings, add Settings option
                if (!appletId.HasValue || appletId.Value != AppletId.Settings)
                {
                    menuItems.Insert(0, "Settings");
                }

                var selected = dialogService.ShowContextMenuDialog(appletName, menuItems, cmd => { });
                if (string.IsNullOrEmpty(selected)) return;

                switch (selected)
                {
                    case "Settings":
                        if (appletId.HasValue && appletId.Value != AppletId.Settings)
                        {
                            navigationController.OpenSettingsForCategory(appletName);
                            statusAnnouncer.Announce($"Settings. {appletName} options.");
                        }
                        else
                        {
                            navigationController.ShowHomeScreen(3, shouldAnnounce: true);
                            statusAnnouncer.Announce("Settings.");
                        }
                        break;
                    case "Help":
                        statusAnnouncer.Announce(helpText);
                        break;
                    case "Back to Home Screen":
                        navigationController.ShowHomeScreen(0, shouldAnnounce: true);
                        statusAnnouncer.Announce("Home screen.");
                        break;
                    case "Exit":
                        navigationController.ShowExitPrompt();
                        break;
                }
            });
    }
}
