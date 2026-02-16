using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class SettingsFocusCoordinator
{
    private readonly SettingsInteractionController _controller;
    private readonly SettingsViewAdapter _view;
    private readonly IReadOnlyList<string> _categories;
    private readonly Func<int> _getOptionCount;
    private readonly Action _announceSelectedOption;
    private readonly Action<string> _announce;

    public SettingsFocusCoordinator(
        SettingsInteractionController controller,
        SettingsViewAdapter view,
        IReadOnlyList<string> categories,
        Func<int> getOptionCount,
        Action announceSelectedOption,
        Action<string> announce)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _getOptionCount = getOptionCount ?? throw new ArgumentNullException(nameof(getOptionCount));
        _announceSelectedOption = announceSelectedOption ?? throw new ArgumentNullException(nameof(announceSelectedOption));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public void FocusRegion()
    {
        switch (_controller.FocusRegion)
        {
            case SettingsFocusRegion.Categories:
                FocusCategories();
                return;
            case SettingsFocusRegion.Options:
                FocusOptions();
                return;
            default:
                FocusActionButton();
                return;
        }
    }

    public void FocusCategories()
    {
        if (_categories.Count == 0)
        {
            return;
        }

        _view.FocusCategoryIndex(_controller.CategoryIndex);
        _announce($"Settings category. {_categories[_controller.CategoryIndex]}.");
    }

    public void FocusOptions()
    {
        if (_getOptionCount() == 0)
        {
            _announce("No options in this category.");
            return;
        }

        MoveFocusToOptions();
        _announceSelectedOption();
    }

    public void MoveFocusToOptions()
    {
        _view.FocusOptionIndex(_controller.OptionIndex);
    }

    public void MoveAction(int delta)
    {
        _controller.MoveAction(delta);
        FocusActionButton();
    }

    public void FocusActionButton()
    {
        _view.FocusActionButton(_controller.ActionIndex);

        var label = _controller.ActionIndex switch
        {
            0 => "Save",
            1 => "Reset Defaults",
            _ => "Back"
        };
        _announce($"Settings action. {label}.");
    }
}
