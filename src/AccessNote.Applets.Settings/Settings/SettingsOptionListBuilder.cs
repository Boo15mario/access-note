using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal sealed class SettingsOptionListBuilder
{
    private readonly SettingsSession _session;
    private readonly Action _resetSettingsFromCatalog;

    public SettingsOptionListBuilder(SettingsSession session, Action resetSettingsFromCatalog)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _resetSettingsFromCatalog = resetSettingsFromCatalog ?? throw new ArgumentNullException(nameof(resetSettingsFromCatalog));
    }

    public IReadOnlyList<SettingsOptionEntry> Build(int categoryIndex)
    {
        return SettingsOptionCatalog
            .GetOptionsForCategory(categoryIndex, _session.Draft, _resetSettingsFromCatalog)
            .ToList();
    }

    public static void PopulateVisibleRows(
        IReadOnlyList<SettingsOptionEntry> optionEntries,
        ICollection<string> visibleOptionRows)
    {
        visibleOptionRows.Clear();
        for (var i = 0; i < optionEntries.Count; i++)
        {
            var option = optionEntries[i];
            visibleOptionRows.Add($"{option.Label}: {option.GetValue()}");
        }
    }
}
