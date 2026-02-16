using System.Collections.Generic;
using AccessNote;

namespace AccessNote.Tests;

public sealed class SettingsSelectionSynchronizerTests
{
    [Fact]
    public void SetCategorySelection_UpdatesSelection_AndResetsSuppressionFlag()
    {
        StaTestRunner.Run(
            () =>
            {
                var (adapter, categoryList, _, _, _) = SettingsViewAdapterTestFactory.Create();
                var categories = new List<string> { "General", "Notes Applet", "Accessibility" };
                categoryList.ItemsSource = categories;
                var synchronizer = new SettingsSelectionSynchronizer();

                synchronizer.SetCategorySelection(adapter, index: 2, categories);

                Assert.Equal(2, categoryList.SelectedIndex);
                Assert.False(synchronizer.IsCategorySelectionChangeSuppressed);
            });
    }

    [Fact]
    public void SetOptionSelection_UpdatesSelection_AndResetsSuppressionFlag()
    {
        StaTestRunner.Run(
            () =>
            {
                var (adapter, _, optionsList, _, _) = SettingsViewAdapterTestFactory.Create();
                var optionRows = new List<string> { "Option A: On", "Option B: Off" };
                optionsList.ItemsSource = optionRows;
                var synchronizer = new SettingsSelectionSynchronizer();

                synchronizer.SetOptionSelection(adapter, index: 1, optionRows);

                Assert.Equal(1, optionsList.SelectedIndex);
                Assert.False(synchronizer.IsOptionSelectionChangeSuppressed);
            });
    }
}
