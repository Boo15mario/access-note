namespace AccessNote;

internal static class InputFeatureFactory
{
    public static ShellInputController CreateController(
        StatusAnnouncer statusAnnouncer,
        ScreenRouter screenRouter,
        MainMenuModule mainMenuModule,
        ShellNavigationController navigationController)
    {
        return new ShellInputController(
            getActiveAppletId: () => screenRouter.ActiveAppletId,
            handleMainMenu: mainMenuModule.HandleInput,
            handleActiveAppletInput: screenRouter.HandleInputForActiveApplet,
            showExitPrompt: navigationController.ShowExitPrompt,
            getHelpText: navigationController.GetHelpText,
            announce: statusAnnouncer.Announce);
    }
}
