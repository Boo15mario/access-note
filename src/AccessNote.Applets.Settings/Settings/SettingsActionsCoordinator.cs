using System;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SettingsActionsCoordinator
{
    private readonly ISettingsDialogService _dialogs;
    private readonly SettingsSession _session;
    private readonly Dispatcher _dispatcher;
    private readonly Action _rebuildOptions;
    private readonly Action _focusSettingsRegion;
    private readonly Action _returnToMainMenu;
    private readonly Action<Exception> _handleSaveError;
    private readonly Action<string> _announce;
    private readonly Action _applyTheme;

    public SettingsActionsCoordinator(
        ISettingsDialogService dialogs,
        SettingsSession session,
        Dispatcher dispatcher,
        Action rebuildOptions,
        Action focusSettingsRegion,
        Action returnToMainMenu,
        Action<Exception> handleSaveError,
        Action<string> announce,
        Action applyTheme)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _rebuildOptions = rebuildOptions ?? throw new ArgumentNullException(nameof(rebuildOptions));
        _focusSettingsRegion = focusSettingsRegion ?? throw new ArgumentNullException(nameof(focusSettingsRegion));
        _returnToMainMenu = returnToMainMenu ?? throw new ArgumentNullException(nameof(returnToMainMenu));
        _handleSaveError = handleSaveError ?? throw new ArgumentNullException(nameof(handleSaveError));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _applyTheme = applyTheme ?? throw new ArgumentNullException(nameof(applyTheme));
    }

    public bool SaveSettingsDraft(bool announce)
    {
        var error = _session.TrySaveDraft();
        if (error != null)
        {
            _handleSaveError(error);
            return false;
        }

        _applyTheme();

        if (announce)
        {
            _announce("Settings saved.");
        }

        return true;
    }

    public void ResetSettingsDraft(bool announce)
    {
        _session.ResetDraftToDefaults();
        _rebuildOptions();

        if (!announce)
        {
            return;
        }

        if (_session.IsDirty)
        {
            _announce("Settings reset to defaults. Press Save to apply.");
            return;
        }

        _announce("Settings already match defaults.");
    }

    public void AttemptReturnToMainMenu()
    {
        if (!EnsureCanLeaveSettings())
        {
            return;
        }

        _returnToMainMenu();
    }

    public bool EnsureCanLeaveSettings()
    {
        if (!_session.IsDirty)
        {
            return true;
        }

        var choice = _dialogs.ShowUnsavedSettingsDialog();
        var leaveResolution = _session.ResolveLeave(choice);
        switch (leaveResolution.Result)
        {
            case SettingsLeaveResult.LeaveAfterSave:
                _applyTheme();
                _announce("Settings saved.");
                return true;
            case SettingsLeaveResult.StayAfterSaveFailure:
                if (leaveResolution.SaveError != null)
                {
                    _handleSaveError(leaveResolution.SaveError);
                }

                _dispatcher.BeginInvoke(_focusSettingsRegion, DispatcherPriority.Input);
                return false;
            case SettingsLeaveResult.LeaveAfterDiscard:
                _rebuildOptions();
                _announce("Settings changes discarded.");
                return true;
            case SettingsLeaveResult.StayCanceled:
            default:
                _dispatcher.BeginInvoke(_focusSettingsRegion, DispatcherPriority.Input);
                _announce("Navigation canceled.");
                return false;
        }
    }
}
