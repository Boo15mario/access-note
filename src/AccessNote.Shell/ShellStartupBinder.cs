using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AccessNote;

internal sealed class ShellStartupBinder
{
    private readonly ListBox _mainMenuList;
    private readonly ListBox _notesList;
    private readonly ListBox _settingsCategoryList;
    private readonly ListBox _settingsOptionsList;
    private readonly IEnumerable<MainMenuEntry> _mainMenuEntries;
    private readonly IEnumerable<NoteDocument> _visibleNotes;
    private readonly IEnumerable<string> _settingsCategories;
    private readonly IEnumerable<string> _visibleSettingsOptions;
    private readonly Action _rebuildSettingsOptions;

    public ShellStartupBinder(
        ListBox mainMenuList,
        ListBox notesList,
        ListBox settingsCategoryList,
        ListBox settingsOptionsList,
        IEnumerable<MainMenuEntry> mainMenuEntries,
        IEnumerable<NoteDocument> visibleNotes,
        IEnumerable<string> settingsCategories,
        IEnumerable<string> visibleSettingsOptions,
        Action rebuildSettingsOptions)
    {
        _mainMenuList = mainMenuList;
        _notesList = notesList;
        _settingsCategoryList = settingsCategoryList;
        _settingsOptionsList = settingsOptionsList;
        _mainMenuEntries = mainMenuEntries;
        _visibleNotes = visibleNotes;
        _settingsCategories = settingsCategories;
        _visibleSettingsOptions = visibleSettingsOptions;
        _rebuildSettingsOptions = rebuildSettingsOptions;
    }

    public void PrepareShellUi()
    {
        _mainMenuList.ItemsSource = _mainMenuEntries;
        _notesList.ItemsSource = _visibleNotes;
        _settingsCategoryList.ItemsSource = _settingsCategories;
        _settingsOptionsList.ItemsSource = _visibleSettingsOptions;
        _rebuildSettingsOptions();
    }
}
