using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class SettingsStateCoordinator
{
    private readonly SettingsSession _settingsSession;
    private readonly NotesSortPolicy _notesSortPolicy;
    private readonly HintAnnouncementPolicy _hintAnnouncementPolicy;
    private readonly ErrorNotifier _errorNotifier;
    private readonly Action<string> _announce;

    public SettingsStateCoordinator(
        SettingsSession settingsSession,
        NotesSortPolicy notesSortPolicy,
        HintAnnouncementPolicy hintAnnouncementPolicy,
        ErrorNotifier errorNotifier,
        Action<string> announce)
    {
        _settingsSession = settingsSession;
        _notesSortPolicy = notesSortPolicy;
        _hintAnnouncementPolicy = hintAnnouncementPolicy;
        _errorNotifier = errorNotifier;
        _announce = announce;
    }

    public void TryLoadSettings()
    {
        var loadError = _settingsSession.LoadOrDefault();
        if (loadError != null)
        {
            _errorNotifier.ShowSettingsLoadError(loadError);
        }
    }

    public IEnumerable<NoteDocument> ApplyNotesSort(IEnumerable<NoteDocument> notes)
    {
        return _notesSortPolicy.Apply(notes, _settingsSession.Current.NotesSortOrder);
    }

    public NoteDocument? GetPreferredNoteSelection(IEnumerable<NoteDocument> notes)
    {
        return _notesSortPolicy.GetPreferredSelection(notes, _settingsSession.Current.NotesSortOrder);
    }

    public void AnnounceHint(string message)
    {
        _hintAnnouncementPolicy.AnnounceIfEnabled(
            _settingsSession.Current.AnnounceHintsOnScreenOpen,
            _announce,
            message);
    }
}
