using System;
using System.Text.Json.Serialization;

namespace AccessNote;

/// <summary>
/// Defines a color theme for the Access Note application.
/// </summary>
public sealed class ThemeDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("background")]
    public string Background { get; set; } = "#111111";

    [JsonPropertyName("foreground")]
    public string Foreground { get; set; } = "#F0F0F0";

    [JsonPropertyName("accent")]
    public string Accent { get; set; } = "#0078D4";

    [JsonPropertyName("border")]
    public string Border { get; set; } = "#666666";

    [JsonPropertyName("menuBackground")]
    public string MenuBackground { get; set; } = "#222222";

    [JsonPropertyName("editorBackground")]
    public string EditorBackground { get; set; } = "#1A1A1A";

    [JsonPropertyName("statusBackground")]
    public string StatusBackground { get; set; } = "#1A1A1A";

    [JsonPropertyName("statusBorder")]
    public string StatusBorder { get; set; } = "#5C5C5C";

    [JsonPropertyName("headerForeground")]
    public string HeaderForeground { get; set; } = "#F0F0F0";

    [JsonPropertyName("selectionBackground")]
    public string SelectionBackground { get; set; } = "#0078D4";

    [JsonPropertyName("selectionForeground")]
    public string SelectionForeground { get; set; } = "#FFFFFF";

    public static ThemeDefinition Dark() => new()
    {
        Name = "Dark",
        Background = "#111111",
        Foreground = "#F0F0F0",
        Accent = "#0078D4",
        Border = "#666666",
        MenuBackground = "#222222",
        EditorBackground = "#1A1A1A",
        StatusBackground = "#1A1A1A",
        StatusBorder = "#5C5C5C",
        HeaderForeground = "#F0F0F0",
        SelectionBackground = "#0078D4",
        SelectionForeground = "#FFFFFF"
    };

    public static ThemeDefinition Light() => new()
    {
        Name = "Light",
        Background = "#F5F5F5",
        Foreground = "#1A1A1A",
        Accent = "#0078D4",
        Border = "#CCCCCC",
        MenuBackground = "#FFFFFF",
        EditorBackground = "#FFFFFF",
        StatusBackground = "#E8E8E8",
        StatusBorder = "#CCCCCC",
        HeaderForeground = "#1A1A1A",
        SelectionBackground = "#0078D4",
        SelectionForeground = "#FFFFFF"
    };

    public static ThemeDefinition HighContrast() => new()
    {
        Name = "High Contrast",
        Background = "#000000",
        Foreground = "#FFFFFF",
        Accent = "#FFFF00",
        Border = "#FFFFFF",
        MenuBackground = "#000000",
        EditorBackground = "#000000",
        StatusBackground = "#000000",
        StatusBorder = "#FFFFFF",
        HeaderForeground = "#FFFF00",
        SelectionBackground = "#FFFF00",
        SelectionForeground = "#000000"
    };
}
