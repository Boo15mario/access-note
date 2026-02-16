using System;
using System.Windows;
using System.Windows.Media;

namespace AccessNote;

/// <summary>
/// Applies a ThemeDefinition to the WPF application resource dictionary at runtime.
/// </summary>
internal static class ThemeManager
{
    public static void Apply(ThemeDefinition theme, ResourceDictionary resources)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(resources);

        SetBrush(resources, "ThemeBackground", theme.Background);
        SetBrush(resources, "ThemeForeground", theme.Foreground);
        SetBrush(resources, "ThemeAccent", theme.Accent);
        SetBrush(resources, "ThemeBorder", theme.Border);
        SetBrush(resources, "ThemeMenuBackground", theme.MenuBackground);
        SetBrush(resources, "ThemeEditorBackground", theme.EditorBackground);
        SetBrush(resources, "ThemeStatusBackground", theme.StatusBackground);
        SetBrush(resources, "ThemeStatusBorder", theme.StatusBorder);
        SetBrush(resources, "ThemeHeaderForeground", theme.HeaderForeground);
        SetBrush(resources, "ThemeSelectionBackground", theme.SelectionBackground);
        SetBrush(resources, "ThemeSelectionForeground", theme.SelectionForeground);
    }

    private static void SetBrush(ResourceDictionary resources, string key, string colorHex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[key] = brush;
        }
        catch (FormatException)
        {
            // Ignore invalid color values; keep existing resource.
        }
    }
}
