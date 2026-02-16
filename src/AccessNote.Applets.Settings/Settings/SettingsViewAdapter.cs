using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SettingsViewAdapter
{
    private readonly ListBox _categoryList;
    private readonly ListBox _optionsList;
    private readonly TextBlock _categoryTitleText;
    private readonly TextBlock _optionHintText;
    private readonly Button _saveButton;
    private readonly Button _resetButton;
    private readonly Button _backButton;
    private readonly Dispatcher _dispatcher;

    public SettingsViewAdapter(
        ListBox categoryList,
        ListBox optionsList,
        TextBlock categoryTitleText,
        TextBlock optionHintText,
        Button saveButton,
        Button resetButton,
        Button backButton,
        Dispatcher dispatcher)
    {
        _categoryList = categoryList;
        _optionsList = optionsList;
        _categoryTitleText = categoryTitleText;
        _optionHintText = optionHintText;
        _saveButton = saveButton;
        _resetButton = resetButton;
        _backButton = backButton;
        _dispatcher = dispatcher;
    }

    public int CategorySelectedIndex => _categoryList.SelectedIndex;

    public int OptionSelectedIndex => _optionsList.SelectedIndex;

    public bool IsBackButtonFocused(object? focusedElement)
    {
        return ReferenceEquals(focusedElement, _backButton);
    }

    public bool IsSaveButtonFocused(object? focusedElement)
    {
        return ReferenceEquals(focusedElement, _saveButton);
    }

    public bool IsResetButtonFocused(object? focusedElement)
    {
        return ReferenceEquals(focusedElement, _resetButton);
    }

    public void SetCategorySelection(int index, IReadOnlyList<string> categories)
    {
        _categoryList.SelectedIndex = index;
        _categoryList.ScrollIntoView(categories[index]);
    }

    public void SetOptionSelection(int index, IReadOnlyList<string> optionRows)
    {
        _optionsList.SelectedIndex = index;
        _optionsList.ScrollIntoView(optionRows[index]);
    }

    public void SetCategoryTitle(string title)
    {
        _categoryTitleText.Text = title;
    }

    public void SetOptionHint(string hint)
    {
        _optionHintText.Text = hint;
    }

    public void FocusCategoryIndex(int index)
    {
        _dispatcher.BeginInvoke(() =>
        {
            if (_categoryList.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item)
            {
                item.Focus();
            }
            else
            {
                _categoryList.Focus();
            }
        }, DispatcherPriority.Input);
    }

    public void FocusOptionIndex(int index)
    {
        _dispatcher.BeginInvoke(() =>
        {
            if (_optionsList.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item)
            {
                item.Focus();
            }
            else
            {
                _optionsList.Focus();
            }
        }, DispatcherPriority.Input);
    }

    public void FocusActionButton(int actionIndex)
    {
        _dispatcher.BeginInvoke(() =>
        {
            switch (actionIndex)
            {
                case 0:
                    _saveButton.Focus();
                    break;
                case 1:
                    _resetButton.Focus();
                    break;
                default:
                    _backButton.Focus();
                    break;
            }
        }, DispatcherPriority.Input);
    }
}
