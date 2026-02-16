using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AccessNote;

internal sealed class SettingsViewStateCoordinator
{
    private readonly SettingsInteractionController _controller;
    private readonly SettingsViewAdapter _view;
    private readonly SettingsSession _session;
    private readonly IReadOnlyList<string> _categories;
    private readonly ObservableCollection<string> _visibleOptionRows;
    private readonly SettingsOptionListBuilder _optionListBuilder;
    private readonly SettingsSelectionSynchronizer _selectionSynchronizer;
    private readonly SettingsOptionAnnouncer _optionAnnouncer;
    private readonly List<SettingsOptionEntry> _optionEntries = new();

    public SettingsViewStateCoordinator(
        SettingsInteractionController controller,
        SettingsViewAdapter view,
        SettingsSession session,
        IReadOnlyList<string> categories,
        ObservableCollection<string> visibleOptionRows,
        Action resetSettingsFromCatalog,
        Action<string> announce)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _visibleOptionRows = visibleOptionRows ?? throw new ArgumentNullException(nameof(visibleOptionRows));
        _optionListBuilder = new SettingsOptionListBuilder(
            _session,
            resetSettingsFromCatalog ?? throw new ArgumentNullException(nameof(resetSettingsFromCatalog)));
        _selectionSynchronizer = new SettingsSelectionSynchronizer();
        _optionAnnouncer = new SettingsOptionAnnouncer(
            _view,
            announce ?? throw new ArgumentNullException(nameof(announce)));
    }

    public bool IsCategorySelectionChangeSuppressed => _selectionSynchronizer.IsCategorySelectionChangeSuppressed;

    public bool IsOptionSelectionChangeSuppressed => _selectionSynchronizer.IsOptionSelectionChangeSuppressed;

    public int OptionCount => _optionEntries.Count;

    public void PrepareScreen()
    {
        _controller.EnterScreen();
        _session.BeginEditing();

        _selectionSynchronizer.SetCategorySelection(_view, _controller.CategoryIndex, _categories);

        RebuildOptions(announceSelection: false);
    }

    public void HandleCategorySelectionChangedFromUi(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        _controller.SetCategoryIndexFromUi(selectedIndex);
        RebuildOptions(announceSelection: _controller.IsFocusRegion(SettingsFocusRegion.Options));
        if (_controller.IsFocusRegion(SettingsFocusRegion.Categories))
        {
            _optionAnnouncer.AnnounceCategory(_categories[_controller.CategoryIndex]);
        }
    }

    public void HandleOptionSelectionChangedFromUi(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= _optionEntries.Count)
        {
            return;
        }

        _controller.SetOptionIndexFromUi(selectedIndex, _optionEntries.Count);
        AnnounceSelectedOption();
    }

    public bool TryMoveCategorySelection(int delta)
    {
        if (!_controller.IsFocusRegion(SettingsFocusRegion.Categories))
        {
            return false;
        }

        SetCategoryIndex(_controller.CategoryIndex + delta, announceSelection: false);
        _optionAnnouncer.AnnounceCategory(_categories[_controller.CategoryIndex]);
        return true;
    }

    public bool TryMoveOptionSelection(int delta)
    {
        if (!_controller.IsFocusRegion(SettingsFocusRegion.Options))
        {
            return false;
        }

        if (!_controller.MoveOption(delta, _optionEntries.Count))
        {
            return true;
        }

        SetOptionIndex(_controller.OptionIndex, announceSelection: true);
        return true;
    }

    public void ChangeSelectedOption(int delta)
    {
        if (!_controller.IsFocusRegion(SettingsFocusRegion.Options) || _optionEntries.Count == 0)
        {
            return;
        }

        var selected = _optionEntries[_controller.OptionIndex];
        if (selected.ChangeBy == null)
        {
            _optionAnnouncer.AnnounceReadOnlyOption();
            return;
        }

        selected.ChangeBy(delta);
        _session.RefreshDirtyState();
        RebuildOptions(announceSelection: false);
        SetOptionIndex(_controller.OptionIndex, announceSelection: true);
    }

    public void RebuildOptions(bool announceSelection)
    {
        _optionEntries.Clear();
        _optionEntries.AddRange(_optionListBuilder.Build(_controller.CategoryIndex));
        SettingsOptionListBuilder.PopulateVisibleRows(_optionEntries, _visibleOptionRows);

        _view.SetCategoryTitle(_categories[_controller.CategoryIndex]);

        if (_optionEntries.Count == 0)
        {
            _controller.SetOptionIndex(0, 0);
            _view.SetOptionHint("No options in this category.");
            return;
        }

        _controller.ClampOptionIndex(_optionEntries.Count);
        SetOptionIndex(_controller.OptionIndex, announceSelection);
    }

    public void AnnounceSelectedOption()
    {
        _optionAnnouncer.AnnounceSelectedOption(_optionEntries, _controller.OptionIndex);
    }

    private void SetCategoryIndex(int index, bool announceSelection)
    {
        if (_categories.Count == 0)
        {
            return;
        }

        _controller.SetCategoryIndex(index);

        _selectionSynchronizer.SetCategorySelection(_view, _controller.CategoryIndex, _categories);

        RebuildOptions(announceSelection);
    }

    private void SetOptionIndex(int index, bool announceSelection)
    {
        if (_optionEntries.Count == 0)
        {
            return;
        }

        _controller.SetOptionIndex(index, _optionEntries.Count);

        _selectionSynchronizer.SetOptionSelection(_view, _controller.OptionIndex, _visibleOptionRows);

        if (announceSelection)
        {
            AnnounceSelectedOption();
        }
    }
}
