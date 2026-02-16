namespace AccessNote;

internal static class SettingsFeatureFactory
{
    public static SettingsModule CreateModule(
        MainWindowCoreInputs core,
        MainWindowSettingsInputs settings,
        MainWindowNavigationInputs navigation,
        StatusAnnouncer statusAnnouncer,
        ErrorNotifier errorNotifier,
        ISettingsDialogService settingsDialogs)
    {
        return new SettingsModule(
            dialogs: settingsDialogs,
            session: core.SettingsSession,
            categories: settings.SettingsCategories,
            visibleOptionRows: settings.VisibleSettingsOptions,
            categoryList: settings.SettingsCategoryList,
            optionsList: settings.SettingsOptionsList,
            categoryTitleText: settings.SettingsCategoryTitleText,
            optionHintText: settings.SettingsOptionHintText,
            saveButton: settings.SettingsSaveButton,
            resetButton: settings.SettingsResetButton,
            backButton: settings.SettingsBackButton,
            dispatcher: core.Dispatcher,
            returnToMainMenu: navigation.ReturnToMainMenuFromSettings,
            handleSaveError: errorNotifier.ShowSettingsSaveError,
            announce: statusAnnouncer.Announce);
    }
}
