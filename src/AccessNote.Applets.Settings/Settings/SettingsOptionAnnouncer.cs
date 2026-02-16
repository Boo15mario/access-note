using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class SettingsOptionAnnouncer
{
    private readonly SettingsViewAdapter _view;
    private readonly Action<string> _announce;

    public SettingsOptionAnnouncer(SettingsViewAdapter view, Action<string> announce)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public void AnnounceCategory(string categoryName)
    {
        _announce($"Settings category. {categoryName}.");
    }

    public void AnnounceReadOnlyOption()
    {
        _announce("This option is read only.");
    }

    public void AnnounceSelectedOption(IReadOnlyList<SettingsOptionEntry> optionEntries, int optionIndex)
    {
        if (optionEntries.Count == 0 || optionIndex < 0 || optionIndex >= optionEntries.Count)
        {
            return;
        }

        var option = optionEntries[optionIndex];
        var value = option.GetValue();
        _view.SetOptionHint(option.Hint);
        _announce($"{option.Label}. {value}. {option.Hint}");
    }
}
