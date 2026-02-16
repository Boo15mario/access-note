using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SettingsModule
{
    private readonly SettingsViewAdapter _view;
    private readonly SettingsViewStateCoordinator _state;
    private readonly SettingsFocusCoordinator _focus;
    private readonly SettingsActionsCoordinator _actions;
    private readonly SettingsFlowCoordinator _flow;

    public SettingsModule(
        ISettingsDialogService dialogs,
        SettingsSession session,
        IReadOnlyList<string> categories,
        ObservableCollection<string> visibleOptionRows,
        ListBox categoryList,
        ListBox optionsList,
        TextBlock categoryTitleText,
        TextBlock optionHintText,
        Button saveButton,
        Button resetButton,
        Button backButton,
        Dispatcher dispatcher,
        Action returnToMainMenu,
        Action<Exception> handleSaveError,
        Action<string> announce)
    {
        var controller = new SettingsInteractionController(categories.Count);
        _view = new SettingsViewAdapter(
            categoryList,
            optionsList,
            categoryTitleText,
            optionHintText,
            saveButton,
            resetButton,
            backButton,
            dispatcher);
        _state = new SettingsViewStateCoordinator(
            controller,
            _view,
            session,
            categories,
            visibleOptionRows,
            resetSettingsFromCatalog: () => _actions!.ResetSettingsDraft(announce: true),
            announce: announce);
        _focus = new SettingsFocusCoordinator(
            controller,
            _view,
            categories,
            getOptionCount: () => _state.OptionCount,
            announceSelectedOption: _state.AnnounceSelectedOption,
            announce: announce);
        _actions = new SettingsActionsCoordinator(
            dialogs: dialogs,
            session: session,
            dispatcher: dispatcher,
            rebuildOptions: () => _state.RebuildOptions(announceSelection: false),
            focusSettingsRegion: _focus.FocusRegion,
            returnToMainMenu: returnToMainMenu,
            handleSaveError: handleSaveError,
            announce: announce);
        _flow = new SettingsFlowCoordinator(
            controller,
            _view,
            moveSelection: MoveSelection,
            moveAction: _focus.MoveAction,
            changeOption: _state.ChangeSelectedOption,
            focusRegion: _focus.FocusRegion,
            moveFocusToOptions: _focus.MoveFocusToOptions,
            attemptReturnToMainMenu: _actions.AttemptReturnToMainMenu,
            saveSettings: shouldAnnounce => { _actions.SaveSettingsDraft(shouldAnnounce); },
            resetSettings: shouldAnnounce => { _actions.ResetSettingsDraft(shouldAnnounce); });
    }

    public void RebuildOptions()
    {
        _state.RebuildOptions(announceSelection: false);
    }

    public void PrepareScreen()
    {
        _state.PrepareScreen();
    }

    public void HandleCategorySelectionChanged(bool isSettingsScreen)
    {
        if (!isSettingsScreen || _state.IsCategorySelectionChangeSuppressed)
        {
            return;
        }

        _state.HandleCategorySelectionChangedFromUi(_view.CategorySelectedIndex);
    }

    public void HandleOptionSelectionChanged(bool isSettingsScreen)
    {
        if (!isSettingsScreen || _state.IsOptionSelectionChangeSuppressed)
        {
            return;
        }

        _state.HandleOptionSelectionChangedFromUi(_view.OptionSelectedIndex);
    }

    public void HandleSaveClick()
    {
        _flow.HandleSaveClick();
    }

    public void HandleResetClick()
    {
        _flow.HandleResetClick();
    }

    public bool HandleInput(KeyEventArgs e)
    {
        return _flow.HandleInput(e);
    }

    public void FocusRegion()
    {
        _focus.FocusRegion();
    }

    public void AttemptReturnToMainMenu()
    {
        _actions.AttemptReturnToMainMenu();
    }

    public bool EnsureCanLeaveSettings()
    {
        return _actions.EnsureCanLeaveSettings();
    }

    private void MoveSelection(int delta)
    {
        if (_state.TryMoveCategorySelection(delta))
        {
            return;
        }

        if (_state.TryMoveOptionSelection(delta))
        {
            return;
        }

        _focus.MoveAction(delta);
    }
}
