using System.ComponentModel;
using AccessNote;

namespace AccessNote.Tests;

public sealed class ExitFlowCoordinatorTests
{
    [Fact]
    public void HandleClosing_CancelsCloseAndShowsPrompt_WhenCloseNotYetAllowed()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = true,
            ShowExitConfirmationDialogResult = false
        };
        var coordinator = new ExitFlowCoordinator(host);
        var args = new CancelEventArgs();

        coordinator.HandleClosing(args);

        Assert.True(args.Cancel);
        Assert.Equal(1, host.ShowExitConfirmationDialogCalls);
    }

    [Fact]
    public void ShowExitPrompt_WhenCannotLeave_DoesNothing()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = false
        };
        var coordinator = new ExitFlowCoordinator(host);

        coordinator.ShowExitPrompt();

        Assert.Equal(1, host.CanLeaveActiveScreenCalls);
        Assert.Equal(0, host.ShowExitConfirmationDialogCalls);
        Assert.Equal(0, host.CloseWindowCalls);
        Assert.Equal(0, host.RestoreFocusCalls);
        Assert.Empty(host.Announcements);
    }

    [Fact]
    public void ShowExitPrompt_WhenUserCancels_RestoresFocusAndAnnounces()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = true,
            ShowExitConfirmationDialogResult = false
        };
        var coordinator = new ExitFlowCoordinator(host);

        coordinator.ShowExitPrompt();

        Assert.Equal(1, host.RestoreFocusCalls);
        Assert.Equal(new[] { "Exit canceled." }, host.Announcements);
        Assert.Equal(0, host.CloseWindowCalls);
    }

    [Fact]
    public void ShowExitPrompt_WhenConfirmedAndPersistSucceeds_ClosesWindow()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = true,
            ShowExitConfirmationDialogResult = true,
            TryPersistNotesResult = true
        };
        var coordinator = new ExitFlowCoordinator(host);

        coordinator.ShowExitPrompt();

        Assert.Equal(1, host.TryPersistNotesCalls);
        Assert.Equal(1, host.CloseWindowCalls);
        Assert.Equal(0, host.RestoreFocusCalls);
        Assert.Empty(host.Announcements);
    }

    [Fact]
    public void ShowExitPrompt_WhenConfirmedAndPersistFails_RestoresFocus()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = true,
            ShowExitConfirmationDialogResult = true,
            TryPersistNotesResult = false
        };
        var coordinator = new ExitFlowCoordinator(host);

        coordinator.ShowExitPrompt();

        Assert.Equal(1, host.TryPersistNotesCalls);
        Assert.Equal(0, host.CloseWindowCalls);
        Assert.Equal(1, host.RestoreFocusCalls);
        Assert.Empty(host.Announcements);
    }

    [Fact]
    public void HandleClosing_DoesNotCancelAfterSuccessfulExitFlow()
    {
        var host = new FakeExitHost
        {
            CanLeaveActiveScreenResult = true,
            ShowExitConfirmationDialogResult = true,
            TryPersistNotesResult = true
        };
        var coordinator = new ExitFlowCoordinator(host);

        coordinator.ShowExitPrompt();
        var args = new CancelEventArgs();
        coordinator.HandleClosing(args);

        Assert.False(args.Cancel);
    }

    private sealed class FakeExitHost : IExitHost
    {
        public bool CanLeaveActiveScreenResult { get; set; } = true;
        public bool TryPersistNotesResult { get; set; } = true;
        public bool? ShowExitConfirmationDialogResult { get; set; }

        public int CanLeaveActiveScreenCalls { get; private set; }
        public int TryPersistNotesCalls { get; private set; }
        public int RestoreFocusCalls { get; private set; }
        public int ShowExitConfirmationDialogCalls { get; private set; }
        public int CloseWindowCalls { get; private set; }
        public List<string> Announcements { get; } = new();

        public bool CanLeaveActiveScreen()
        {
            CanLeaveActiveScreenCalls++;
            return CanLeaveActiveScreenResult;
        }

        public bool TryPersistNotes()
        {
            TryPersistNotesCalls++;
            return TryPersistNotesResult;
        }

        public void RestoreFocusForActiveScreen()
        {
            RestoreFocusCalls++;
        }

        public void Announce(string message)
        {
            Announcements.Add(message);
        }

        public bool? ShowExitConfirmationDialog()
        {
            ShowExitConfirmationDialogCalls++;
            return ShowExitConfirmationDialogResult;
        }

        public void CloseWindow()
        {
            CloseWindowCalls++;
        }
    }
}
