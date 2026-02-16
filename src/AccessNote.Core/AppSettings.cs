using System;

namespace AccessNote;

public enum StartScreenOption
{
    MainMenu,
    Notes
}

public enum NotesInitialFocusOption
{
    List,
    Editor
}

public enum NotesSortOrderOption
{
    LastModifiedNewest,
    LastModifiedOldest,
    TitleAscending
}

public sealed class AppSettings
{
    public StartScreenOption StartScreen { get; set; }

    public NotesInitialFocusOption NotesInitialFocus { get; set; }

    public bool ConfirmBeforeDeleteNote { get; set; }

    public NotesSortOrderOption NotesSortOrder { get; set; }

    public bool AnnounceStatusMessages { get; set; }

    public bool AnnounceHintsOnScreenOpen { get; set; }

    public AppSettings Clone()
    {
        return new AppSettings
        {
            StartScreen = StartScreen,
            NotesInitialFocus = NotesInitialFocus,
            ConfirmBeforeDeleteNote = ConfirmBeforeDeleteNote,
            NotesSortOrder = NotesSortOrder,
            AnnounceStatusMessages = AnnounceStatusMessages,
            AnnounceHintsOnScreenOpen = AnnounceHintsOnScreenOpen
        };
    }

    public void ApplyFrom(AppSettings source)
    {
        ArgumentNullException.ThrowIfNull(source);
        StartScreen = source.StartScreen;
        NotesInitialFocus = source.NotesInitialFocus;
        ConfirmBeforeDeleteNote = source.ConfirmBeforeDeleteNote;
        NotesSortOrder = source.NotesSortOrder;
        AnnounceStatusMessages = source.AnnounceStatusMessages;
        AnnounceHintsOnScreenOpen = source.AnnounceHintsOnScreenOpen;
    }

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            StartScreen = StartScreenOption.MainMenu,
            NotesInitialFocus = NotesInitialFocusOption.List,
            ConfirmBeforeDeleteNote = true,
            NotesSortOrder = NotesSortOrderOption.LastModifiedNewest,
            AnnounceStatusMessages = true,
            AnnounceHintsOnScreenOpen = true
        };
    }
}
