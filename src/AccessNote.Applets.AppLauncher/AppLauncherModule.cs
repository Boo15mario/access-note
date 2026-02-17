using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccessNote;

internal sealed class AppLauncherModule
{
    private static readonly string[] LaunchableExtensions = { ".exe", ".lnk", ".bat" };

    private readonly FavoriteAppStorage _storage;
    private readonly Action<string> _announce;

    private TextBlock? _modeText;
    private ListBox? _appList;
    private TextBlock? _detailName;
    private TextBlock? _detailPath;
    private TextBlock? _detailArguments;

    private bool _isBrowseMode;
    private string _currentDirectory = string.Empty;
    private List<FavoriteApp> _favorites = new();
    private List<string> _browseEntries = new();

    public AppLauncherModule(FavoriteAppStorage storage, Action<string> announce)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public void Enter(
        TextBlock modeText,
        ListBox appList,
        TextBlock detailName,
        TextBlock detailPath,
        TextBlock detailArguments)
    {
        _modeText = modeText;
        _appList = appList;
        _detailName = detailName;
        _detailPath = detailPath;
        _detailArguments = detailArguments;

        _isBrowseMode = false;
        _currentDirectory = GetStartDirectory();

        _appList.SelectionChanged += OnSelectionChanged;
        LoadFavorites();
        UpdateModeDisplay();

        var mode = _isBrowseMode ? "Browse" : "Favorites";
        var count = _isBrowseMode ? _browseEntries.Count : _favorites.Count;
        _announce($"App Launcher. {mode} mode. {count} items.");
    }

    public void RestoreFocus()
    {
        _appList?.Focus();
    }

    public bool CanLeave()
    {
        if (_appList != null)
        {
            _appList.SelectionChanged -= OnSelectionChanged;
        }
        return true;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.Tab && modifiers == ModifierKeys.None)
        {
            ToggleMode();
            return true;
        }

        if (_isBrowseMode)
        {
            return HandleBrowseInput(key, modifiers);
        }

        return HandleFavoritesInput(key, modifiers);
    }

    private bool HandleFavoritesInput(Key key, ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.Control && key == Key.N)
        {
            AddFavorite();
            return true;
        }

        if (key == Key.F2 && modifiers == ModifierKeys.None)
        {
            RenameFavorite();
            return true;
        }

        if (key == Key.Delete && modifiers == ModifierKeys.None)
        {
            RemoveFavorite();
            return true;
        }

        if ((key == Key.Enter || key == Key.Return) && modifiers == ModifierKeys.None)
        {
            LaunchSelectedFavorite();
            return true;
        }

        return false;
    }

    private bool HandleBrowseInput(Key key, ModifierKeys modifiers)
    {
        if ((key == Key.Enter || key == Key.Return) && modifiers == ModifierKeys.None)
        {
            ActivateBrowseSelection();
            return true;
        }

        if (key == Key.Back && modifiers == ModifierKeys.None)
        {
            NavigateUp();
            return true;
        }

        return false;
    }

    private void ToggleMode()
    {
        _isBrowseMode = !_isBrowseMode;
        if (_isBrowseMode)
        {
            LoadBrowseEntries();
        }
        else
        {
            LoadFavorites();
        }
        UpdateModeDisplay();

        var mode = _isBrowseMode ? "Browse" : "Favorites";
        var count = _isBrowseMode ? _browseEntries.Count : _favorites.Count;
        _announce($"{mode} mode. {count} items.");
    }

    private void UpdateModeDisplay()
    {
        if (_modeText != null)
        {
            _modeText.Text = _isBrowseMode ? "Browse" : "Favorites";
        }
    }

    private void LoadFavorites()
    {
        _favorites = new List<FavoriteApp>(_storage.GetAll());
        RefreshAppList(_favorites.Select(f => f.DisplayName).ToList());
    }

    private void LoadBrowseEntries()
    {
        _browseEntries = new List<string>();

        try
        {
            if (!Directory.Exists(_currentDirectory))
            {
                _currentDirectory = GetStartDirectory();
            }

            var directories = Directory.GetDirectories(_currentDirectory)
                .Select(d => Path.GetFileName(d))
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase);

            foreach (var dir in directories)
            {
                _browseEntries.Add("[" + dir + "]");
            }

            var files = Directory.GetFiles(_currentDirectory)
                .Where(f => LaunchableExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => Path.GetFileName(f))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                _browseEntries.Add(file);
            }
        }
        catch
        {
            // If we can't read directory, show empty list
        }

        RefreshAppList(_browseEntries);
    }

    private void RefreshAppList(IList<string> items)
    {
        if (_appList == null) return;

        _appList.Items.Clear();
        foreach (var item in items)
        {
            _appList.Items.Add(item);
        }

        if (_appList.Items.Count > 0)
        {
            _appList.SelectedIndex = 0;
        }

        ClearDetails();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        if (_appList == null) return;

        var index = _appList.SelectedIndex;
        if (index < 0)
        {
            ClearDetails();
            return;
        }

        if (!_isBrowseMode && index < _favorites.Count)
        {
            var fav = _favorites[index];
            SetDetails(fav.DisplayName, fav.Path, fav.Arguments ?? string.Empty);
        }
        else if (_isBrowseMode && index < _browseEntries.Count)
        {
            var entry = _browseEntries[index];
            var isDir = entry.StartsWith("[") && entry.EndsWith("]");
            var name = isDir ? entry.Trim('[', ']') : entry;
            var fullPath = Path.Combine(_currentDirectory, name);
            SetDetails(name, fullPath, isDir ? "Directory" : string.Empty);
        }
    }

    private void SetDetails(string name, string path, string arguments)
    {
        if (_detailName != null) _detailName.Text = name;
        if (_detailPath != null) _detailPath.Text = path;
        if (_detailArguments != null) _detailArguments.Text = arguments;
    }

    private void ClearDetails()
    {
        SetDetails(string.Empty, string.Empty, string.Empty);
    }

    private void AddFavorite()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Application",
            Filter = "Applications (*.exe;*.lnk;*.bat)|*.exe;*.lnk;*.bat|All Files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };

        if (dialog.ShowDialog() == true)
        {
            var filePath = dialog.FileName;
            var name = Path.GetFileNameWithoutExtension(filePath);
            var app = _storage.Add(name, filePath);
            LoadFavorites();
            _announce($"Added {app.DisplayName}");
        }
    }

    private void RenameFavorite()
    {
        if (_appList == null) return;
        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count) return;

        var fav = _favorites[index];
        var dialog = new RenameAppDialog(fav.Name);
        if (dialog.ShowDialog() == true)
        {
            fav.Name = dialog.AppName;
            _storage.Update(fav);
            LoadFavorites();
            if (index < _appList.Items.Count)
            {
                _appList.SelectedIndex = index;
            }
            _announce($"Renamed to {dialog.AppName}");
        }
    }

    public void RemoveFavorite()
    {
        if (_isBrowseMode) return;
        if (_appList == null) return;
        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count) return;

        var fav = _favorites[index];
        _storage.Delete(fav.Id);
        LoadFavorites();
        _announce($"Removed {fav.DisplayName}");
    }

    private void LaunchSelectedFavorite()
    {
        if (_appList == null) return;
        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count) return;

        var fav = _favorites[index];
        LaunchProcess(fav.Path, fav.Arguments);
    }

    private void ActivateBrowseSelection()
    {
        if (_appList == null) return;
        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _browseEntries.Count) return;

        var entry = _browseEntries[index];
        var isDir = entry.StartsWith("[") && entry.EndsWith("]");

        if (isDir)
        {
            var dirName = entry.Trim('[', ']');
            _currentDirectory = Path.Combine(_currentDirectory, dirName);
            LoadBrowseEntries();
        }
        else
        {
            var fullPath = Path.Combine(_currentDirectory, entry);
            LaunchProcess(fullPath, null);
        }
    }

    private void NavigateUp()
    {
        var parent = Directory.GetParent(_currentDirectory);
        if (parent != null)
        {
            _currentDirectory = parent.FullName;
            LoadBrowseEntries();
            _announce($"{Path.GetFileName(_currentDirectory)}. {_browseEntries.Count} items.");
        }
    }

    private void LaunchProcess(string path, string? arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            _announce($"Launched {Path.GetFileNameWithoutExtension(path)}");
        }
        catch (Exception ex)
        {
            _announce($"Failed to launch: {ex.Message}");
        }
    }

    private static string GetStartDirectory()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        return Directory.Exists(desktop)
            ? desktop
            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    }
}
