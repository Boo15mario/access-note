using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class SettingsSelectionSynchronizer
{
    public bool IsCategorySelectionChangeSuppressed { get; private set; }

    public bool IsOptionSelectionChangeSuppressed { get; private set; }

    public void SetCategorySelection(SettingsViewAdapter view, int index, IReadOnlyList<string> categories)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(categories);

        WithCategorySelectionSuppressed(() => view.SetCategorySelection(index, categories));
    }

    public void SetOptionSelection(SettingsViewAdapter view, int index, IReadOnlyList<string> optionRows)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(optionRows);

        WithOptionSelectionSuppressed(() => view.SetOptionSelection(index, optionRows));
    }

    private void WithCategorySelectionSuppressed(Action action)
    {
        IsCategorySelectionChangeSuppressed = true;
        try
        {
            action();
        }
        finally
        {
            IsCategorySelectionChangeSuppressed = false;
        }
    }

    private void WithOptionSelectionSuppressed(Action action)
    {
        IsOptionSelectionChangeSuppressed = true;
        try
        {
            action();
        }
        finally
        {
            IsOptionSelectionChangeSuppressed = false;
        }
    }
}
