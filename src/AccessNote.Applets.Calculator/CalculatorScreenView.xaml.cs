using System.Windows.Controls;

namespace AccessNote;

public partial class CalculatorScreenView : UserControl
{
    public CalculatorScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock ModeTextControl => ModeText;
    internal TextBox ExpressionBoxControl => ExpressionBox;
    internal TextBlock ResultTextControl => ResultText;
    internal ListBox HistoryListControl => HistoryList;
}
