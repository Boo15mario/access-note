using System.Collections.Generic;

namespace AccessNote;

public interface INoteStorage
{
    IReadOnlyList<NoteDocument> LoadNotes();

    void SaveNotes(IEnumerable<NoteDocument> notes);
}

public interface ISettingsStorage
{
    AppSettings LoadSettings();

    void SaveSettings(AppSettings settings);
}
