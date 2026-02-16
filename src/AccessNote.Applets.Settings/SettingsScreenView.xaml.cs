using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AccessNote;

public partial class SettingsScreenView : UserControl
{
    public SettingsScreenView()
    {
        InitializeComponent();
    }

    internal ObservableCollection<string> VisibleSettingsOptions { get; } = new();

    internal ListBox SettingsCategoryListControl => SettingsCategoryList;
    internal ListBox SettingsOptionsListControl => SettingsOptionsList;
    internal TextBlock SettingsCategoryTitleTextControl => SettingsCategoryTitleText;
    internal TextBlock SettingsOptionHintTextControl => SettingsOptionHintText;
    internal Button SettingsSaveButtonControl => SettingsSaveButton;
    internal Button SettingsResetButtonControl => SettingsResetButton;
    internal Button SettingsBackButtonControl => SettingsBackButton;

    internal Action? CategorySelectionChanged { get; set; }
    internal Action? OptionSelectionChanged { get; set; }
    internal Action? SaveClick { get; set; }
    internal Action? ResetClick { get; set; }
    internal Action? BackClick { get; set; }

    private void OnSettingsCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CategorySelectionChanged?.Invoke();
    }

    private void OnSettingsOptionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        OptionSelectionChanged?.Invoke();
    }

    private void OnSettingsSaveClick(object sender, RoutedEventArgs e)
    {
        SaveClick?.Invoke();
    }

    private void OnSettingsResetClick(object sender, RoutedEventArgs e)
    {
        ResetClick?.Invoke();
    }

    private void OnSettingsBackClick(object sender, RoutedEventArgs e)
    {
        BackClick?.Invoke();
    }
}
