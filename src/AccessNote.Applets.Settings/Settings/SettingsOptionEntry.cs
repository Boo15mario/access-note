using System;

namespace AccessNote;

internal sealed class SettingsOptionEntry
{
    public required string Label { get; init; }

    public required string Hint { get; init; }

    public required Func<string> GetValue { get; init; }

    public Action<int>? ChangeBy { get; init; }
}
