using System;

namespace AccessNote;

internal sealed class SettingsInteractionController
{
    private const int ActionCount = 3;
    private readonly int _categoryCount;

    public SettingsInteractionController(int categoryCount)
    {
        _categoryCount = Math.Max(categoryCount, 0);
        FocusRegion = SettingsFocusRegion.Categories;
    }

    public int CategoryIndex { get; private set; }

    public int OptionIndex { get; private set; }

    public int ActionIndex { get; private set; }

    public SettingsFocusRegion FocusRegion { get; private set; }

    public void EnterScreen()
    {
        FocusRegion = SettingsFocusRegion.Categories;
        ActionIndex = 0;
    }

    public bool IsFocusRegion(SettingsFocusRegion region)
    {
        return FocusRegion == region;
    }

    public void SelectSaveAction()
    {
        FocusRegion = SettingsFocusRegion.Actions;
        ActionIndex = 0;
    }

    public void SelectResetAction()
    {
        FocusRegion = SettingsFocusRegion.Actions;
        ActionIndex = 1;
    }

    public void CycleFocus(bool forward)
    {
        FocusRegion = (FocusRegion, forward) switch
        {
            (SettingsFocusRegion.Categories, true) => SettingsFocusRegion.Options,
            (SettingsFocusRegion.Options, true) => SettingsFocusRegion.Actions,
            (SettingsFocusRegion.Actions, true) => SettingsFocusRegion.Categories,
            (SettingsFocusRegion.Categories, false) => SettingsFocusRegion.Actions,
            (SettingsFocusRegion.Options, false) => SettingsFocusRegion.Categories,
            _ => SettingsFocusRegion.Options
        };
    }

    public void MoveAction(int delta)
    {
        ActionIndex = Wrap(ActionIndex + delta, ActionCount);
    }

    public bool PromoteCategoriesToOptions()
    {
        if (FocusRegion != SettingsFocusRegion.Categories)
        {
            return false;
        }

        FocusRegion = SettingsFocusRegion.Options;
        return true;
    }

    public bool MoveOption(int delta, int optionCount)
    {
        if (optionCount <= 0)
        {
            return false;
        }

        OptionIndex = Wrap(OptionIndex + delta, optionCount);
        return true;
    }

    public void SetCategoryIndex(int index)
    {
        if (_categoryCount == 0)
        {
            CategoryIndex = 0;
            OptionIndex = 0;
            return;
        }

        CategoryIndex = Wrap(index, _categoryCount);
        OptionIndex = 0;
    }

    public void SetCategoryIndexFromUi(int index)
    {
        if (index < 0 || _categoryCount == 0)
        {
            return;
        }

        CategoryIndex = Wrap(index, _categoryCount);
        OptionIndex = 0;
    }

    public void SetOptionIndex(int index, int optionCount)
    {
        if (optionCount <= 0)
        {
            OptionIndex = 0;
            return;
        }

        OptionIndex = Wrap(index, optionCount);
    }

    public void SetOptionIndexFromUi(int index, int optionCount)
    {
        if (index < 0 || optionCount <= 0)
        {
            return;
        }

        OptionIndex = Wrap(index, optionCount);
    }

    public void ClampOptionIndex(int optionCount)
    {
        if (optionCount <= 0)
        {
            OptionIndex = 0;
            return;
        }

        if (OptionIndex >= optionCount)
        {
            OptionIndex = optionCount - 1;
        }
        else if (OptionIndex < 0)
        {
            OptionIndex = 0;
        }
    }

    private static int Wrap(int value, int count)
    {
        return ((value % count) + count) % count;
    }
}
