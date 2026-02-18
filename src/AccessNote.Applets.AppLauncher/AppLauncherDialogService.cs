using System;
using System.IO;
using System.Windows;

namespace AccessNote;

internal sealed class AppLauncherDialogService : IAppLauncherDialogService
{
    private static readonly string[] LaunchableExtensions = { ".exe", ".lnk", ".bat" };

    private readonly Window _owner;

    public AppLauncherDialogService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public bool TryPromptAddFavorite(AddFavoriteRequest request, out AddFavoriteResult result)
    {
        var addDialog = new AddFavoriteDialog(
            canUseCurrentSelection: request.CanUseCurrentSelection,
            currentSelectionDisplayName: request.CurrentSelectionDisplayName,
            currentSelectionPath: request.CurrentSelectionPath)
        {
            Owner = _owner
        };

        if (addDialog.ShowDialog() != true)
        {
            result = default;
            return false;
        }

        if (addDialog.SelectedSource == AddFavoriteSource.CurrentSelection)
        {
            if (!request.CanUseCurrentSelection || string.IsNullOrWhiteSpace(request.CurrentSelectionPath))
            {
                result = default;
                return false;
            }

            result = new AddFavoriteResult(
                Source: AddFavoriteSource.CurrentSelection,
                Path: request.CurrentSelectionPath,
                DisplayName: ResolveDisplayName(request.CurrentSelectionDisplayName, request.CurrentSelectionPath));
            return true;
        }

        var picker = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Application",
            Filter = "Applications (*.exe;*.lnk;*.bat)|*.exe;*.lnk;*.bat|All Files (*.*)|*.*",
            InitialDirectory = ResolveInitialDirectory(
                preferredDirectory: request.InitialBrowseDirectory,
                selectedPath: request.CurrentSelectionPath)
        };

        if (picker.ShowDialog(_owner) != true)
        {
            result = default;
            return false;
        }

        var selectedPath = picker.FileName;
        result = new AddFavoriteResult(
            Source: AddFavoriteSource.Browse,
            Path: selectedPath,
            DisplayName: ResolveDisplayName(string.Empty, selectedPath));
        return true;
    }

    private static string ResolveInitialDirectory(string preferredDirectory, string selectedPath)
    {
        if (Path.IsPathRooted(selectedPath))
        {
            var selectedDirectory = Path.GetDirectoryName(selectedPath);
            if (!string.IsNullOrWhiteSpace(selectedDirectory) && Directory.Exists(selectedDirectory))
            {
                return selectedDirectory;
            }
        }

        if (!string.IsNullOrWhiteSpace(preferredDirectory) && Directory.Exists(preferredDirectory))
        {
            return preferredDirectory;
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    }

    private static string ResolveDisplayName(string displayName, string path)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        var extension = Path.GetExtension(path);
        if (!string.IsNullOrWhiteSpace(extension) && Array.IndexOf(LaunchableExtensions, extension.ToLowerInvariant()) >= 0)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        return Path.GetFileName(path);
    }
}
