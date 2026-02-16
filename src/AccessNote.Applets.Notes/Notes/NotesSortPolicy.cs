using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal sealed class NotesSortPolicy
{
    public IEnumerable<NoteDocument> Apply(IEnumerable<NoteDocument> notes, NotesSortOrderOption sortOrder)
    {
        return sortOrder switch
        {
            NotesSortOrderOption.LastModifiedOldest => notes.OrderBy(note => note.LastModifiedUtc),
            NotesSortOrderOption.TitleAscending => notes.OrderBy(note => note.Title, StringComparer.OrdinalIgnoreCase),
            _ => notes.OrderByDescending(note => note.LastModifiedUtc),
        };
    }

    public NoteDocument? GetPreferredSelection(IEnumerable<NoteDocument> notes, NotesSortOrderOption sortOrder)
    {
        return Apply(notes, sortOrder).FirstOrDefault();
    }
}
