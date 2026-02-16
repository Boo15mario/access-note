using System.Windows.Controls;
using System.Windows.Threading;
using AccessNote;

namespace AccessNote.Tests;

internal static class SettingsViewAdapterTestFactory
{
    public static (
        SettingsViewAdapter Adapter,
        ListBox CategoryList,
        ListBox OptionsList,
        TextBlock CategoryTitleText,
        TextBlock OptionHintText) Create()
    {
        var categoryList = new ListBox();
        var optionsList = new ListBox();
        var categoryTitleText = new TextBlock();
        var optionHintText = new TextBlock();
        var saveButton = new Button();
        var resetButton = new Button();
        var backButton = new Button();

        var adapter = new SettingsViewAdapter(
            categoryList: categoryList,
            optionsList: optionsList,
            categoryTitleText: categoryTitleText,
            optionHintText: optionHintText,
            saveButton: saveButton,
            resetButton: resetButton,
            backButton: backButton,
            dispatcher: Dispatcher.CurrentDispatcher);

        return (adapter, categoryList, optionsList, categoryTitleText, optionHintText);
    }
}
