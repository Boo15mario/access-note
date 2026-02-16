using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccessNote;

internal sealed class CalculatorModule
{
    private const int MaxHistory = 20;

    private TextBlock? _modeText;
    private TextBox? _expressionBox;
    private TextBlock? _resultText;
    private ListBox? _historyList;

    private string _expression = string.Empty;
    private bool _isScientific;
    private readonly List<string> _history = new();
    private int _historyIndex = -1;

    public void Enter(TextBlock modeText, TextBox expressionBox, TextBlock resultText, ListBox historyList)
    {
        _modeText = modeText;
        _expressionBox = expressionBox;
        _resultText = resultText;
        _historyList = historyList;

        _expression = string.Empty;
        UpdateDisplay();
    }

    public void RestoreFocus()
    {
        _expressionBox?.Focus();
    }

    public bool CanLeave()
    {
        return true;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.Control && key == Key.M)
        {
            ToggleMode();
            return true;
        }

        if (key == Key.Enter || key == Key.Return)
        {
            Evaluate();
            return true;
        }

        if (key == Key.Back)
        {
            if (_expression.Length > 0)
            {
                _expression = _expression[..^1];
                UpdateDisplay();
            }
            return true;
        }

        if (key == Key.Escape)
        {
            if (_expression.Length > 0)
            {
                _expression = string.Empty;
                if (_resultText != null) _resultText.Text = string.Empty;
                UpdateDisplay();
                return true;
            }
            return false;
        }

        if (key == Key.Up && string.IsNullOrEmpty(_expression))
        {
            NavigateHistory(-1);
            return true;
        }

        if (key == Key.Down && string.IsNullOrEmpty(_expression))
        {
            NavigateHistory(1);
            return true;
        }

        if (_isScientific && modifiers == ModifierKeys.None)
        {
            var handled = HandleScientificKey(key);
            if (handled) return true;
        }

        var ch = KeyToChar(key, modifiers);
        if (ch != null)
        {
            _expression += ch;
            _historyIndex = -1;
            UpdateDisplay();
            return true;
        }

        return false;
    }

    private bool HandleScientificKey(Key key)
    {
        string? func = key switch
        {
            Key.S => "sin(",
            Key.C => "cos(",
            Key.T => "tan(",
            Key.L => "log(",
            Key.N => "ln(",
            Key.P => "pow(",
            _ => null,
        };

        if (func != null)
        {
            _expression += func;
            _historyIndex = -1;
            UpdateDisplay();
            return true;
        }

        if (key == Key.OemExclaim || (key == Key.D1 && Keyboard.IsKeyDown(Key.LeftShift) == false && _expression.Length > 0 && char.IsDigit(_expression[^1])))
        {
            // '!' for factorial is handled in Evaluate via expression parsing
        }

        return false;
    }

    private void ToggleMode()
    {
        _isScientific = !_isScientific;
        if (_modeText != null)
        {
            _modeText.Text = _isScientific ? "Scientific" : "Basic";
        }
    }

    private void Evaluate()
    {
        if (string.IsNullOrWhiteSpace(_expression))
            return;

        try
        {
            var result = EvaluateExpression(_expression);
            var entry = $"{_expression} = {result}";

            _history.Insert(0, entry);
            if (_history.Count > MaxHistory)
                _history.RemoveAt(_history.Count - 1);

            UpdateHistoryList();

            if (_resultText != null)
                _resultText.Text = result;

            _expression = string.Empty;
            _historyIndex = -1;
            UpdateDisplay();
        }
        catch
        {
            if (_resultText != null)
                _resultText.Text = "Error";
        }
    }

    private string EvaluateExpression(string expr)
    {
        var processed = ProcessScientificFunctions(expr);
        processed = ProcessSqrt(processed);
        processed = ProcessFactorial(processed);

        using var table = new DataTable();
        var result = table.Compute(processed, null);
        var value = Convert.ToDouble(result, CultureInfo.InvariantCulture);
        return value.ToString("G", CultureInfo.CurrentCulture);
    }

    private static string ProcessScientificFunctions(string expr)
    {
        var result = expr;
        result = ProcessFunction(result, "sin", Math.Sin);
        result = ProcessFunction(result, "cos", Math.Cos);
        result = ProcessFunction(result, "tan", Math.Tan);
        result = ProcessFunction(result, "log", Math.Log10);
        result = ProcessFunction(result, "ln", Math.Log);
        result = ProcessPow(result);
        return result;
    }

    private static string ProcessFunction(string expr, string funcName, Func<double, double> func)
    {
        while (true)
        {
            var idx = expr.IndexOf(funcName + "(", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) break;

            var start = idx + funcName.Length + 1;
            var depth = 1;
            var end = start;
            while (end < expr.Length && depth > 0)
            {
                if (expr[end] == '(') depth++;
                else if (expr[end] == ')') depth--;
                end++;
            }

            var inner = expr[start..(end - 1)];
            var innerVal = EvaluateSimple(inner);
            var result = func(innerVal);
            expr = string.Concat(expr.AsSpan(0, idx), result.ToString("G", CultureInfo.InvariantCulture), expr.AsSpan(end));
        }
        return expr;
    }

    private static string ProcessPow(string expr)
    {
        while (true)
        {
            var idx = expr.IndexOf("pow(", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) break;

            var start = idx + 4;
            var depth = 1;
            var end = start;
            while (end < expr.Length && depth > 0)
            {
                if (expr[end] == '(') depth++;
                else if (expr[end] == ')') depth--;
                end++;
            }

            var inner = expr[start..(end - 1)];
            var commaIdx = inner.IndexOf(',');
            if (commaIdx < 0) break;

            var baseVal = EvaluateSimple(inner[..commaIdx].Trim());
            var expVal = EvaluateSimple(inner[(commaIdx + 1)..].Trim());
            var result = Math.Pow(baseVal, expVal);
            expr = string.Concat(expr.AsSpan(0, idx), result.ToString("G", CultureInfo.InvariantCulture), expr.AsSpan(end));
        }
        return expr;
    }

    private static string ProcessSqrt(string expr)
    {
        return ProcessFunction(expr, "sqrt", Math.Sqrt);
    }

    private static string ProcessFactorial(string expr)
    {
        while (true)
        {
            var idx = expr.IndexOf('!');
            if (idx <= 0) break;

            var numStart = idx - 1;
            while (numStart > 0 && (char.IsDigit(expr[numStart - 1]) || expr[numStart - 1] == '.'))
                numStart--;

            var numStr = expr[numStart..idx];
            if (!int.TryParse(numStr, out var n) || n < 0)
                break;

            long factorial = 1;
            for (int i = 2; i <= n; i++)
                factorial *= i;

            expr = string.Concat(expr.AsSpan(0, numStart), factorial.ToString(CultureInfo.InvariantCulture), expr.AsSpan(idx + 1));
        }
        return expr;
    }

    private static double EvaluateSimple(string expr)
    {
        using var table = new DataTable();
        var result = table.Compute(expr, null);
        return Convert.ToDouble(result, CultureInfo.InvariantCulture);
    }

    private void NavigateHistory(int direction)
    {
        if (_history.Count == 0) return;

        _historyIndex += direction;
        if (_historyIndex < 0) _historyIndex = 0;
        if (_historyIndex >= _history.Count) _historyIndex = _history.Count - 1;

        if (_historyList != null)
        {
            _historyList.SelectedIndex = _historyIndex;
            _historyList.ScrollIntoView(_historyList.SelectedItem);
        }
    }

    private void UpdateHistoryList()
    {
        if (_historyList == null) return;
        _historyList.Items.Clear();
        foreach (var entry in _history)
        {
            _historyList.Items.Add(entry);
        }
    }

    private void UpdateDisplay()
    {
        if (_expressionBox != null)
        {
            _expressionBox.Text = _expression;
        }
    }

    private static char? KeyToChar(Key key, ModifierKeys modifiers)
    {
        var shift = (modifiers & ModifierKeys.Shift) != 0;

        return key switch
        {
            Key.D0 or Key.NumPad0 when !shift => '0',
            Key.D1 or Key.NumPad1 when !shift => '1',
            Key.D2 or Key.NumPad2 when !shift => '2',
            Key.D3 or Key.NumPad3 when !shift => '3',
            Key.D4 or Key.NumPad4 when !shift => '4',
            Key.D5 when !shift => '5',
            Key.NumPad5 => '5',
            Key.D5 when shift => '%',
            Key.D6 or Key.NumPad6 when !shift => '6',
            Key.D7 or Key.NumPad7 when !shift => '7',
            Key.D8 when !shift => '8',
            Key.NumPad8 => '8',
            Key.D8 when shift => '*',
            Key.D9 when !shift => '9',
            Key.NumPad9 => '9',
            Key.D9 when shift => '(',
            Key.D0 when shift => ')',
            Key.OemPlus when shift => '+',
            Key.OemMinus when !shift => '-',
            Key.Add => '+',
            Key.Subtract => '-',
            Key.Multiply => '*',
            Key.Divide => '/',
            Key.OemPeriod or Key.Decimal => '.',
            Key.D1 when shift => '!',
            Key.OemComma => ',',
            _ => null,
        };
    }
}
