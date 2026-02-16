using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class NotesModule
{
    private readonly NotesSession _session;
    private readonly NotesViewAdapter _view;
    private readonly NotesViewStateCoordinator _state;
    private readonly NotesFocusCoordinator _focus;
    private readonly NotesActionsCoordinator _actions;
    private readonly NotesFlowCoordinator _flow;
    private readonly Func<IEnumerable<NoteDocument>, NoteDocument?> _preferredSelection;
    private readonly Action<string> _announce;
    private readonly Action<Exception> _handlePersistError;
    private readonly Dispatcher _dispatcher;

    public NotesModule(
        INotesDialogService dialogs,
        NotesSession session,
        Func<IEnumerable<NoteDocument>, IEnumerable<NoteDocument>> applySort,
        Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelection,
        Func<bool> shouldConfirmDelete,
        IList<NoteDocument> visibleNotes,
        ListBox notesList,
        TextBox searchBox,
        TextBox editorTextBox,
        TextBlock editorTitleText,
        FrameworkElement statusRegion,
        FrameworkElement statusText,
        Dispatcher dispatcher,
        Func<string> getStatusText,
        Action showMainMenu,
        Action<Exception> handlePersistError,
        Action<string> announce)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _preferredSelection = preferredSelection ?? throw new ArgumentNullException(nameof(preferredSelection));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _handlePersistError = handlePersistError ?? throw new ArgumentNullException(nameof(handlePersistError));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        _view = new NotesViewAdapter(
            notesList,
            searchBox,
            editorTextBox,
            editorTitleText,
            statusRegion,
            statusText);
        _state = new NotesViewStateCoordinator(
            _session,
            _view,
            visibleNotes,
            applySort,
            announce);
        _focus = new NotesFocusCoordinator(
            _view,
            _state,
            refreshVisibleNotes: () => _state.RefreshVisibleNotes(announceCount: false),
            ensureCanLeaveActiveNote: () => _actions!.EnsureCanLeaveActiveNote(),
            showMainMenu: showMainMenu,
            getStatusText: getStatusText,
            announce: announce);
        _actions = new NotesActionsCoordinator(
            dialogs: dialogs,
            session: _session,
            state: _state,
            preferredSelection: _preferredSelection,
            shouldConfirmDelete: shouldConfirmDelete,
            refreshVisibleNotes: () => _state.RefreshVisibleNotes(announceCount: false),
            focusEditor: _focus.FocusEditor,
            focusNotesList: _focus.FocusNotesList,
            restoreNotesFocus: _focus.RestoreNotesFocus,
            tryPersistNotes: TryPersistNotesInternal,
            handlePersistError: handlePersistError,
            announce: announce);
        _flow = new NotesFlowCoordinator(
            focusList: _focus.FocusNotesList,
            focusEditor: _focus.FocusEditor,
            createNewNote: _actions.CreateNewNote,
            saveActiveNote: () => { _actions.SaveActiveNote(announce: true); },
            focusSearch: _focus.FocusSearchBox,
            cycleFocus: _focus.CycleNotesFocus,
            renameActiveNote: _actions.RenameActiveNote,
            deleteActiveNote: _actions.DeleteActiveNote,
            handleEscape: _focus.HandleEscape);
    }

    public Exception? LoadPersistedNotes()
    {
        return _session.Load(_preferredSelection);
    }

    public bool TryPersistNotes()
    {
        return TryPersistNotesInternal();
    }

    public void EnterWorkspace(NotesInitialFocusOption initialFocus)
    {
        if (_session.EnsureAtLeastOneNote())
        {
            _state.SetSearchText(string.Empty);
            _state.RefreshVisibleNotes(announceCount: false);
            _state.LoadEditorFromActiveNote();
            _dispatcher.BeginInvoke(_focus.FocusEditor, DispatcherPriority.Input);
            return;
        }

        _state.SetSearchText(string.Empty);
        _state.RefreshVisibleNotes(announceCount: false);
        _session.EnsureActiveNoteIsValid(_preferredSelection);
        _state.SelectActiveNoteInList();
        _state.LoadEditorFromActiveNote();
        _dispatcher.BeginInvoke(
            () =>
            {
                if (initialFocus == NotesInitialFocusOption.Editor)
                {
                    _focus.FocusEditor();
                    return;
                }

                _focus.FocusNotesList();
            },
            DispatcherPriority.Input);
    }

    public void HandleSearchTextChanged(bool isNotesScreen)
    {
        if (!isNotesScreen || _state.IsSearchTextChangeSuppressed)
        {
            return;
        }

        _state.RefreshVisibleNotes(announceCount: true);
    }

    public void HandleSelectionChanged(bool isNotesScreen)
    {
        if (!isNotesScreen || _state.IsSelectionChangeSuppressed)
        {
            return;
        }

        var selected = _view.GetSelectedNote();
        if (selected == null || selected == _session.ActiveNote)
        {
            return;
        }

        if (!_actions.EnsureCanLeaveActiveNote())
        {
            _state.RevertSelectionToActiveNote();
            return;
        }

        _session.SetActiveNote(selected);
        _state.LoadEditorFromActiveNote();
        _announce($"{selected.Title} selected.");
    }

    public void HandleEditorTextChanged(bool isNotesScreen)
    {
        if (!isNotesScreen || _state.IsEditorTextChangeSuppressed)
        {
            return;
        }

        _state.SyncEditorTextToActiveNote();
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        return _flow.HandleInput(key, modifiers, _focus.IsFocusInListRegion());
    }

    public bool EnsureCanLeaveActiveNote()
    {
        return _actions.EnsureCanLeaveActiveNote();
    }

    public void RestoreFocus()
    {
        _focus.RestoreNotesFocus();
    }

    private bool TryPersistNotesInternal()
    {
        var error = _session.Persist();
        if (error == null)
        {
            return true;
        }

        _handlePersistError(error);
        return false;
    }
}
