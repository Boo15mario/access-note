namespace AccessNote;

/// <summary>
/// Contract for playing application sound effects.
/// </summary>
public interface ISoundService
{
    void PlayStartup();
    void PlayVolumeChange();
    void PlaySound(string name);
}
