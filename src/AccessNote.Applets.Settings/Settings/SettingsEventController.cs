using System;

namespace AccessNote;

internal sealed class SettingsEventController
{
    private readonly SettingsModule _settingsModule;
    private readonly Func<AppletId?> _getActiveAppletId;

    public SettingsEventController(
        SettingsModule settingsModule,
        Func<AppletId?> getActiveAppletId)
    {
        _settingsModule = settingsModule ?? throw new ArgumentNullException(nameof(settingsModule));
        _getActiveAppletId = getActiveAppletId ?? throw new ArgumentNullException(nameof(getActiveAppletId));
    }

    public void HandleCategorySelectionChanged()
    {
        _settingsModule.HandleCategorySelectionChanged(IsSettingsScreen());
    }

    public void HandleOptionSelectionChanged()
    {
        _settingsModule.HandleOptionSelectionChanged(IsSettingsScreen());
    }

    public void HandleSaveClick()
    {
        _settingsModule.HandleSaveClick();
    }

    public void HandleResetClick()
    {
        _settingsModule.HandleResetClick();
    }

    public void HandleBackClick()
    {
        _settingsModule.AttemptReturnToMainMenu();
    }

    private bool IsSettingsScreen()
    {
        return _getActiveAppletId() == AppletId.Settings;
    }
}
