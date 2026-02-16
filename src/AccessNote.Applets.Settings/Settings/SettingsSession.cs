using System;

namespace AccessNote;

internal enum SettingsLeaveResult
{
    LeaveAfterSave,
    LeaveAfterDiscard,
    StayAfterSaveFailure,
    StayCanceled
}

internal readonly record struct SettingsLeaveResolution(
    SettingsLeaveResult Result,
    Exception? SaveError = null);

internal sealed class SettingsSession
{
    private readonly ISettingsStorage _storage;

    public SettingsSession(ISettingsStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public AppSettings Current { get; private set; } = AppSettings.CreateDefault();

    public AppSettings Draft { get; private set; } = AppSettings.CreateDefault();

    public bool IsDirty { get; private set; }

    public Exception? LoadOrDefault()
    {
        try
        {
            Current = _storage.LoadSettings();
        }
        catch (Exception ex)
        {
            Current = AppSettings.CreateDefault();
            Draft = Current.Clone();
            IsDirty = false;
            return ex;
        }

        Draft = Current.Clone();
        IsDirty = false;
        return null;
    }

    public void BeginEditing()
    {
        Draft = Current.Clone();
        IsDirty = false;
    }

    public Exception? TrySaveDraft()
    {
        try
        {
            _storage.SaveSettings(Draft);
            Current.ApplyFrom(Draft);
            Draft = Current.Clone();
            IsDirty = false;
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public void ResetDraftToDefaults()
    {
        Draft = AppSettings.CreateDefault();
        RefreshDirtyState();
    }

    public void DiscardDraftChanges()
    {
        Draft = Current.Clone();
        IsDirty = false;
    }

    public void RefreshDirtyState()
    {
        IsDirty = !SettingsOptionCatalog.AreEqual(Draft, Current);
    }

    public SettingsLeaveResolution ResolveLeave(UnsavedChangesChoice choice)
    {
        return choice switch
        {
            UnsavedChangesChoice.Save => ResolveSaveLeave(),
            UnsavedChangesChoice.Discard => ResolveDiscardLeave(),
            _ => new SettingsLeaveResolution(SettingsLeaveResult.StayCanceled)
        };
    }

    private SettingsLeaveResolution ResolveSaveLeave()
    {
        var error = TrySaveDraft();
        if (error != null)
        {
            return new SettingsLeaveResolution(SettingsLeaveResult.StayAfterSaveFailure, error);
        }

        return new SettingsLeaveResolution(SettingsLeaveResult.LeaveAfterSave);
    }

    private SettingsLeaveResolution ResolveDiscardLeave()
    {
        DiscardDraftChanges();
        return new SettingsLeaveResolution(SettingsLeaveResult.LeaveAfterDiscard);
    }
}
