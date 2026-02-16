using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AccessNote;

/// <summary>
/// Loads built-in and custom themes from disk.
/// </summary>
public static class ThemeLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IReadOnlyList<ThemeDefinition> GetBuiltInThemes()
    {
        return new[]
        {
            ThemeDefinition.Dark(),
            ThemeDefinition.Light(),
            ThemeDefinition.HighContrast()
        };
    }

    /// <summary>
    /// Loads custom themes from *.theme.json files in the given directory.
    /// Returns an empty list if the directory does not exist or contains no valid themes.
    /// </summary>
    public static IReadOnlyList<ThemeDefinition> LoadCustomThemes(string themesDirectory)
    {
        var themes = new List<ThemeDefinition>();
        if (!Directory.Exists(themesDirectory))
        {
            return themes;
        }

        foreach (var file in Directory.GetFiles(themesDirectory, "*.theme.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var theme = JsonSerializer.Deserialize<ThemeDefinition>(json, JsonOptions);
                if (theme != null && !string.IsNullOrWhiteSpace(theme.Name))
                {
                    themes.Add(theme);
                }
            }
            catch (Exception)
            {
                // Skip invalid theme files.
            }
        }

        return themes;
    }

    /// <summary>
    /// Returns all available themes: built-in first, then custom.
    /// </summary>
    public static IReadOnlyList<ThemeDefinition> LoadAllThemes(string themesDirectory)
    {
        var all = new List<ThemeDefinition>(GetBuiltInThemes());
        all.AddRange(LoadCustomThemes(themesDirectory));
        return all;
    }

    /// <summary>
    /// Finds a theme by name (case-insensitive). Falls back to Dark if not found.
    /// </summary>
    public static ThemeDefinition Resolve(IReadOnlyList<ThemeDefinition> themes, string themeName)
    {
        foreach (var theme in themes)
        {
            if (string.Equals(theme.Name, themeName, StringComparison.OrdinalIgnoreCase))
            {
                return theme;
            }
        }

        return ThemeDefinition.Dark();
    }
}
