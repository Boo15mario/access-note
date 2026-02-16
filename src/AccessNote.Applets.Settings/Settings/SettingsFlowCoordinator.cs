using System;
using System.Windows.Input;

namespace AccessNote;

internal sealed class SettingsFlowCoordinator
{
    private readonly SettingsInteractionController _controller;
    private readonly SettingsViewAdapter _view;
    private readonly Action<int> _moveSelection;
    private readonly Action<int> _moveAction;
    private readonly Action<int> _changeOption;
    private readonly Action _focusRegion;
    private readonly Action _moveFocusToOptions;
    private readonly Action _attemptReturnToMainMenu;
    private readonly Action<bool> _saveSettings;
    private readonly Action<bool> _resetSettings;

    public SettingsFlowCoordinator(
        SettingsInteractionController controller,
        SettingsViewAdapter view,
        Action<int> moveSelection,
        Action<int> moveAction,
        Action<int> changeOption,
        Action focusRegion,
        Action moveFocusToOptions,
        Action attemptReturnToMainMenu,
        Action<bool> saveSettings,
        Action<bool> resetSettings)
    {
        _controller = controller;
        _view = view;
        _moveSelection = moveSelection;
        _moveAction = moveAction;
        _changeOption = changeOption;
        _focusRegion = focusRegion;
        _moveFocusToOptions = moveFocusToOptions;
        _attemptReturnToMainMenu = attemptReturnToMainMenu;
        _saveSettings = saveSettings;
        _resetSettings = resetSettings;
    }

    public void HandleSaveClick()
    {
        _controller.SelectSaveAction();
        _saveSettings(true);
    }

    public void HandleResetClick()
    {
        _controller.SelectResetAction();
        _resetSettings(true);
    }

    public bool HandleInput(KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.S)
        {
            _saveSettings(true);
            return true;
        }

        if (e.Key == Key.Enter)
        {
            if (_view.IsBackButtonFocused(Keyboard.FocusedElement))
            {
                _attemptReturnToMainMenu();
                return true;
            }

            if (_view.IsSaveButtonFocused(Keyboard.FocusedElement))
            {
                _controller.SelectSaveAction();
                _saveSettings(true);
                return true;
            }

            if (_view.IsResetButtonFocused(Keyboard.FocusedElement))
            {
                _controller.SelectResetAction();
                _resetSettings(true);
                return true;
            }
        }

        switch (e.Key)
        {
            case Key.Escape:
                _attemptReturnToMainMenu();
                return true;
            case Key.Tab:
                _controller.CycleFocus(forward: (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
                _focusRegion();
                return true;
            case Key.Up:
                _moveSelection(-1);
                return true;
            case Key.Down:
                _moveSelection(1);
                return true;
            case Key.Left:
                HandleHorizontalChange(delta: -1);
                return true;
            case Key.Right:
                HandleHorizontalChange(delta: 1);
                return true;
            case Key.Enter:
                ActivateCurrentSelection();
                return true;
            default:
                return false;
        }
    }

    private void HandleHorizontalChange(int delta)
    {
        if (_controller.IsFocusRegion(SettingsFocusRegion.Actions))
        {
            _moveAction(delta);
            return;
        }

        if (_controller.PromoteCategoriesToOptions())
        {
            _moveFocusToOptions();
        }

        _changeOption(delta);
    }

    private void ActivateCurrentSelection()
    {
        if (_controller.PromoteCategoriesToOptions())
        {
            _focusRegion();
            return;
        }

        if (_controller.IsFocusRegion(SettingsFocusRegion.Options))
        {
            _changeOption(1);
            return;
        }

        switch (_controller.ActionIndex)
        {
            case 0:
                _saveSettings(true);
                return;
            case 1:
                _resetSettings(true);
                return;
            case 2:
                _attemptReturnToMainMenu();
                return;
            default:
                return;
        }
    }
}
