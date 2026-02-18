using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccessNote;

internal sealed class AppLauncherModule
{
    private static readonly string[] LaunchableExtensions = { ".exe", ".lnk", ".bat" };

    private readonly FavoriteAppStorage _storage;
    private readonly IAppLauncherDialogService _dialogs;
    private readonly Action<string> _announce;
    private readonly IReadOnlyList<IAppBrowseSource> _browseSources;
    private readonly AppBrowseCatalogBuilder _browseCatalogBuilder;
    private readonly AppBrowseNavigator _browseNavigator;
    private readonly IAppLaunchService _launchService;

    private TextBlock? _modeText;
    private ListBox? _appList;
    private TextBlock? _detailName;
    private TextBlock? _detailPath;
    private TextBlock? _detailArguments;
    private Button? _addButton;
    private Button? _removeButton;
    private Button? _launchButton;

    private bool _isBrowseMode;
    private List<FavoriteApp> _favorites = new();
    private List<AppBrowseNode> _allBrowseEntries = new();
    private List<AppBrowseNode> _browseEntries = new();
    private AppBrowseNode? _cachedBrowseRoot;
    private bool _browseRefreshInFlight;
    private string _browseFilterQuery = string.Empty;

    public AppLauncherModule(
        FavoriteAppStorage storage,
        IAppLauncherDialogService dialogs,
        Action<string> announce,
        IReadOnlyList<IAppBrowseSource>? browseSources = null,
        AppBrowseCatalogBuilder? browseCatalogBuilder = null,
        AppBrowseNavigator? browseNavigator = null,
        IAppLaunchService? launchService = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _browseSources = browseSources ?? AppBrowseSourceFactory.CreateDefault();
        _browseCatalogBuilder = browseCatalogBuilder ?? new AppBrowseCatalogBuilder();
        _browseNavigator = browseNavigator ?? new AppBrowseNavigator();
        _launchService = launchService ?? new AppLauncherProcessLauncher();
    }

    public void Enter(
        TextBlock modeText,
        ListBox appList,
        TextBlock detailName,
        TextBlock detailPath,
        TextBlock detailArguments,
        Button addButton,
        Button removeButton,
        Button launchButton)
    {
        DetachHandlers();

        _modeText = modeText;
        _appList = appList;
        _detailName = detailName;
        _detailPath = detailPath;
        _detailArguments = detailArguments;
        _addButton = addButton;
        _removeButton = removeButton;
        _launchButton = launchButton;

        _isBrowseMode = false;
        _browseFilterQuery = string.Empty;

        AttachHandlers();
        LoadFavorites();
        UpdateModeDisplay();

        var count = _isBrowseMode ? _browseEntries.Count : _favorites.Count;
        _announce(AppLauncherAnnouncementText.EnterScreen(
            isBrowseMode: _isBrowseMode,
            count: count));
    }

    public void RestoreFocus()
    {
        _appList?.Focus();
    }

    public bool CanLeave()
    {
        DetachHandlers();
        return true;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.Tab && modifiers == ModifierKeys.None)
        {
            ToggleMode();
            return true;
        }

        if (modifiers == ModifierKeys.Control && key == Key.N)
        {
            AddFavorite();
            return true;
        }

        if (_isBrowseMode)
        {
            return HandleBrowseInput(key, modifiers);
        }

        return HandleFavoritesInput(key, modifiers);
    }

    public void RemoveFavorite()
    {
        if (_isBrowseMode) return;
        if (_appList == null) return;

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count)
        {
            _announce(AppLauncherAnnouncementText.NoFavoriteSelected());
            return;
        }

        var favorite = _favorites[index];
        _storage.Delete(favorite.Id);
        LoadFavorites();

        if (_appList.Items.Count > 0)
        {
            _appList.SelectedIndex = Math.Min(index, _appList.Items.Count - 1);
        }

        _announce(AppLauncherAnnouncementText.RemovedFromFavorites(favorite.DisplayName));
    }

    private bool HandleFavoritesInput(Key key, ModifierKeys modifiers)
    {
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
        if (key == Key.Insert && modifiers == ModifierKeys.None)
        {
            AddCurrentBrowseSelectionToFavorites();
            return true;
        }

        if (TryHandleBrowseFilterInput(key, modifiers))
        {
            return true;
        }

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
            EnterBrowseMode();
        }
        else
        {
            _browseFilterQuery = string.Empty;
            LoadFavorites();
        }

        UpdateModeDisplay();

        var count = _isBrowseMode ? _browseEntries.Count : _favorites.Count;
        _announce(AppLauncherAnnouncementText.ModeChanged(
            isBrowseMode: _isBrowseMode,
            count: count));
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

        if (_isBrowseMode)
        {
            return;
        }

        RefreshAppList(_favorites.Select(favorite => favorite.DisplayName).ToList());
    }

    private void EnterBrowseMode()
    {
        _browseFilterQuery = string.Empty;

        if (_cachedBrowseRoot == null)
        {
            RefreshBrowseCatalog();
        }
        else
        {
            _browseNavigator.Load(_cachedBrowseRoot);
        }

        LoadBrowseEntries();
        StartBrowseCatalogRefreshInBackground();
    }

    private void RefreshBrowseCatalog()
    {
        var browseRoot = _browseCatalogBuilder.Build(_browseSources);
        _cachedBrowseRoot = browseRoot;
        _browseNavigator.Load(browseRoot);
    }

    private void LoadBrowseEntries()
    {
        _allBrowseEntries = new List<AppBrowseNode>(_browseNavigator.CurrentEntries);
        ApplyBrowseFilter(announce: false);
    }

    private static string FormatBrowseEntry(AppBrowseNode entry)
    {
        return entry.IsFolder
            ? "[" + entry.Name + "]"
            : entry.Name;
    }

    private bool TryHandleBrowseFilterInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.Escape && modifiers == ModifierKeys.None && !string.IsNullOrWhiteSpace(_browseFilterQuery))
        {
            _browseFilterQuery = string.Empty;
            ApplyBrowseFilter(announce: true);
            return true;
        }

        if (key == Key.Back && modifiers == ModifierKeys.None && !string.IsNullOrWhiteSpace(_browseFilterQuery))
        {
            _browseFilterQuery = _browseFilterQuery[..^1];
            ApplyBrowseFilter(announce: true);
            return true;
        }

        if (!TryGetFilterCharacter(key, modifiers, out var character))
        {
            return false;
        }

        _browseFilterQuery += character;
        ApplyBrowseFilter(announce: true);
        return true;
    }

    private void ApplyBrowseFilter(bool announce)
    {
        if (!_isBrowseMode)
        {
            return;
        }

        _browseEntries = string.IsNullOrWhiteSpace(_browseFilterQuery)
            ? new List<AppBrowseNode>(_allBrowseEntries)
            : _allBrowseEntries
                .Where(entry => entry.Name.Contains(_browseFilterQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

        RefreshAppList(_browseEntries.Select(FormatBrowseEntry).ToList());

        if (announce)
        {
            _announce(AppLauncherAnnouncementText.FilterApplied(_browseFilterQuery, _browseEntries.Count));
        }
    }

    private static bool TryGetFilterCharacter(Key key, ModifierKeys modifiers, out char character)
    {
        character = '\0';

        if ((modifiers & ~ModifierKeys.Shift) != ModifierKeys.None)
        {
            return false;
        }

        if (key >= Key.A && key <= Key.Z)
        {
            var offset = key - Key.A;
            character = (char)('a' + offset);
            return true;
        }

        if (key >= Key.D0 && key <= Key.D9)
        {
            var offset = key - Key.D0;
            character = (char)('0' + offset);
            return true;
        }

        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            var offset = key - Key.NumPad0;
            character = (char)('0' + offset);
            return true;
        }

        switch (key)
        {
            case Key.Space:
                character = ' ';
                return true;
            case Key.OemMinus:
            case Key.Subtract:
                character = '-';
                return true;
            case Key.OemPeriod:
            case Key.Decimal:
                character = '.';
                return true;
            default:
                return false;
        }
    }

    private void RefreshAppList(IList<string> items)
    {
        if (_appList == null) return;

        _appList.ItemsSource = null;
        _appList.ItemsSource = items;

        if (_appList.Items.Count > 0)
        {
            _appList.SelectedIndex = 0;
            UpdateDetails();
            return;
        }

        ClearDetails();
    }

    private void StartBrowseCatalogRefreshInBackground()
    {
        if (_browseRefreshInFlight)
        {
            return;
        }

        var dispatcher = _appList?.Dispatcher;
        if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return;
        }

        _browseRefreshInFlight = true;
        _ = Task.Run(() =>
            {
                try
                {
                    return _browseCatalogBuilder.Build(_browseSources);
                }
                catch
                {
                    return null;
                }
            })
            .ContinueWith(
                task =>
                {
                    if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                    {
                        _browseRefreshInFlight = false;
                        return;
                    }

                    dispatcher.BeginInvoke(new Action(() => ApplyBrowseCatalogRefresh(task.Result)));
                },
                TaskScheduler.Default);
    }

    private void ApplyBrowseCatalogRefresh(AppBrowseNode? refreshedRoot)
    {
        _browseRefreshInFlight = false;
        if (refreshedRoot == null)
        {
            return;
        }

        _cachedBrowseRoot = refreshedRoot;
        if (!_isBrowseMode)
        {
            return;
        }

        var previousPath = _browseNavigator.GetCurrentPathDisplay();
        var previousSelectionName = GetSelectedBrowseEntryName();
        _browseNavigator.Load(refreshedRoot);
        _browseNavigator.RestorePath(previousPath);
        LoadBrowseEntries();
        RestoreBrowseSelectionByName(previousSelectionName);
    }

    private string GetSelectedBrowseEntryName()
    {
        if (_appList == null)
        {
            return string.Empty;
        }

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _browseEntries.Count)
        {
            return string.Empty;
        }

        return _browseEntries[index].Name;
    }

    private void RestoreBrowseSelectionByName(string preferredName)
    {
        if (_appList == null || string.IsNullOrWhiteSpace(preferredName))
        {
            return;
        }

        for (var index = 0; index < _browseEntries.Count; index++)
        {
            if (!string.Equals(_browseEntries[index].Name, preferredName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _appList.SelectedIndex = index;
            return;
        }
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
            var favorite = _favorites[index];
            SetDetails(favorite.DisplayName, favorite.Path, favorite.Arguments ?? string.Empty);
            return;
        }

        if (_isBrowseMode && index < _browseEntries.Count)
        {
            var entry = _browseEntries[index];
            if (entry.IsFolder)
            {
                var currentPath = _browseNavigator.GetCurrentPathDisplay();
                var nextPath = string.Equals(currentPath, "Browse", StringComparison.OrdinalIgnoreCase)
                    ? entry.Name
                    : currentPath + "\\" + entry.Name;
                SetDetails(entry.Name, nextPath, "Folder");
                return;
            }

            SetDetails(entry.Name, entry.DetailPath, BuildBrowseDetailArguments(entry));
            return;
        }

        ClearDetails();
    }

    private void SetDetails(string name, string path, string arguments)
    {
        if (_detailName != null) _detailName.Text = name;
        if (_detailPath != null) _detailPath.Text = path;
        if (_detailArguments != null) _detailArguments.Text = arguments;
    }

    private static string BuildBrowseDetailArguments(AppBrowseNode entry)
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(entry.DetailArguments))
        {
            segments.Add(entry.DetailArguments);
        }

        if (!string.IsNullOrWhiteSpace(entry.SourceLabel))
        {
            segments.Add("Source: " + entry.SourceLabel);
        }

        var confidenceText = entry.TitleMetadata.Confidence switch
        {
            TitleConfidence.Verified => "verified",
            TitleConfidence.Inferred => "inferred",
            TitleConfidence.Fallback => "fallback",
            _ => string.Empty,
        };
        if (!string.IsNullOrWhiteSpace(confidenceText))
        {
            var titleDetail = string.IsNullOrWhiteSpace(entry.TitleMetadata.Provenance)
                ? confidenceText
                : $"{confidenceText} ({entry.TitleMetadata.Provenance})";
            segments.Add("Title: " + titleDetail);
        }

        return string.Join(" | ", segments);
    }

    private void ClearDetails()
    {
        SetDetails(string.Empty, string.Empty, string.Empty);
    }

    private void AddFavorite()
    {
        var request = BuildAddFavoriteRequest();
        if (!_dialogs.TryPromptAddFavorite(request, out var result))
        {
            return;
        }

        if (result.Source == AddFavoriteSource.CurrentSelection && !request.CanUseCurrentSelection)
        {
            _announce(AppLauncherAnnouncementText.CurrentSelectionUnavailable());
            return;
        }

        if (!AppLauncherFavoriteLaunchCodec.TryParse(result.Path.Trim(), out var launchSpec))
        {
            _announce(AppLauncherAnnouncementText.UnsupportedFavoriteSelection());
            return;
        }

        if (launchSpec.TargetType == LaunchTargetType.DirectPath)
        {
            if (string.IsNullOrWhiteSpace(launchSpec.Target) || !File.Exists(launchSpec.Target))
            {
                _announce(AppLauncherAnnouncementText.InvalidFavoritePath());
                return;
            }

            if (!IsLaunchableFilePath(launchSpec.Target))
            {
                _announce(AppLauncherAnnouncementText.UnsupportedFavoriteSelection());
                return;
            }
        }

        var launchIdentity = AppLauncherFavoriteLaunchCodec.GetIdentityKey(launchSpec);
        var duplicate = _favorites.FirstOrDefault(favorite => FavoriteMatchesIdentity(favorite, launchIdentity));
        if (duplicate != null)
        {
            _announce(AppLauncherAnnouncementText.AlreadyInFavorites(duplicate.DisplayName));
            return;
        }

        var favoriteName = ResolveDisplayName(result.DisplayName, launchSpec);
        var storedTarget = AppLauncherFavoriteLaunchCodec.Encode(launchSpec);
        var savedFavorite = _storage.Add(favoriteName, storedTarget);
        LoadFavorites();

        if (!_isBrowseMode)
        {
            SelectFavoriteById(savedFavorite.Id);
        }

        _announce(AppLauncherAnnouncementText.AddedToFavorites(savedFavorite.DisplayName));
    }

    private void AddCurrentBrowseSelectionToFavorites()
    {
        if (!TryGetCurrentBrowseFavoriteCandidate(out var launchToken, out var displayName))
        {
            _announce(AppLauncherAnnouncementText.CurrentSelectionUnavailable());
            return;
        }

        if (!AppLauncherFavoriteLaunchCodec.TryParse(launchToken, out var launchSpec))
        {
            _announce(AppLauncherAnnouncementText.UnsupportedFavoriteSelection());
            return;
        }

        if (launchSpec.TargetType == LaunchTargetType.DirectPath)
        {
            if (string.IsNullOrWhiteSpace(launchSpec.Target) || !File.Exists(launchSpec.Target))
            {
                _announce(AppLauncherAnnouncementText.InvalidFavoritePath());
                return;
            }

            if (!IsLaunchableFilePath(launchSpec.Target))
            {
                _announce(AppLauncherAnnouncementText.UnsupportedFavoriteSelection());
                return;
            }
        }

        var launchIdentity = AppLauncherFavoriteLaunchCodec.GetIdentityKey(launchSpec);
        var duplicate = _favorites.FirstOrDefault(favorite => FavoriteMatchesIdentity(favorite, launchIdentity));
        if (duplicate != null)
        {
            _announce(AppLauncherAnnouncementText.AlreadyInFavorites(duplicate.DisplayName));
            return;
        }

        var favoriteName = ResolveDisplayName(displayName, launchSpec);
        var savedFavorite = _storage.Add(favoriteName, launchToken);
        LoadFavorites();
        _announce(AppLauncherAnnouncementText.AddedToFavorites(savedFavorite.DisplayName));
    }

    private static bool FavoriteMatchesIdentity(FavoriteApp favorite, string identity)
    {
        if (!AppLauncherFavoriteLaunchCodec.TryParse(favorite.Path, out var favoriteSpec))
        {
            return false;
        }

        var favoriteIdentity = AppLauncherFavoriteLaunchCodec.GetIdentityKey(favoriteSpec);
        return string.Equals(favoriteIdentity, identity, StringComparison.OrdinalIgnoreCase);
    }

    private void RenameFavorite()
    {
        if (_appList == null) return;

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count)
        {
            _announce(AppLauncherAnnouncementText.NoFavoriteSelected());
            return;
        }

        var favorite = _favorites[index];
        var dialog = new RenameAppDialog(favorite.Name);
        if (dialog.ShowDialog() == true)
        {
            favorite.Name = dialog.AppName;
            _storage.Update(favorite);
            LoadFavorites();
            if (index < _appList.Items.Count)
            {
                _appList.SelectedIndex = index;
            }

            _announce("Renamed to " + dialog.AppName);
        }
    }

    private void LaunchSelectedFavorite()
    {
        if (_appList == null) return;

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _favorites.Count)
        {
            _announce(AppLauncherAnnouncementText.NoFavoriteSelected());
            return;
        }

        var favorite = _favorites[index];
        if (!AppLauncherFavoriteLaunchCodec.TryParse(favorite.Path, out var launchSpec))
        {
            _announce(AppLauncherAnnouncementText.UnsupportedFavoriteSelection());
            return;
        }

        if (launchSpec.TargetType == LaunchTargetType.DirectPath && !string.IsNullOrWhiteSpace(favorite.Arguments))
        {
            launchSpec = launchSpec with { Arguments = favorite.Arguments }; 
        }

        LaunchEntry(launchSpec, favorite.DisplayName);
    }

    private void ActivateBrowseSelection()
    {
        if (_appList == null) return;

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _browseEntries.Count)
        {
            return;
        }

        if (_browseNavigator.TryEnterFolder(index, out var folderName))
        {
            _browseFilterQuery = string.Empty;
            LoadBrowseEntries();
            _announce(AppLauncherAnnouncementText.DirectoryChanged(
                directoryName: folderName,
                count: _browseEntries.Count));
            return;
        }

        var entry = _browseEntries[index];
        if (!entry.LaunchSpec.HasValue)
        {
            return;
        }

        LaunchEntry(entry.LaunchSpec.Value, entry.Name);
    }

    private void NavigateUp()
    {
        if (!_browseNavigator.TryNavigateUp(out var folderName))
        {
            return;
        }

        _browseFilterQuery = string.Empty;
        LoadBrowseEntries();
        _announce(AppLauncherAnnouncementText.DirectoryChanged(
            directoryName: folderName,
            count: _browseEntries.Count));
    }

    private void LaunchEntry(LaunchSpec launchSpec, string displayName)
    {
        if (_launchService.TryLaunch(launchSpec, out var errorMessage))
        {
            _announce(AppLauncherAnnouncementText.Launched(displayName));
            return;
        }

        _announce(AppLauncherAnnouncementText.LaunchFailed(errorMessage));
    }

    private AddFavoriteRequest BuildAddFavoriteRequest()
    {
        if (TryGetCurrentBrowseFavoriteCandidate(out var launchToken, out var displayName))
        {
            return new AddFavoriteRequest(
                CanUseCurrentSelection: true,
                CurrentSelectionPath: launchToken,
                CurrentSelectionDisplayName: displayName,
                InitialBrowseDirectory: GetDefaultBrowseDirectory());
        }

        return new AddFavoriteRequest(
            CanUseCurrentSelection: false,
            CurrentSelectionPath: string.Empty,
            CurrentSelectionDisplayName: string.Empty,
            InitialBrowseDirectory: GetDefaultBrowseDirectory());
    }

    private bool TryGetCurrentBrowseFavoriteCandidate(out string launchToken, out string displayName)
    {
        launchToken = string.Empty;
        displayName = string.Empty;

        if (!_isBrowseMode || _appList == null)
        {
            return false;
        }

        var index = _appList.SelectedIndex;
        if (index < 0 || index >= _browseEntries.Count)
        {
            return false;
        }

        var entry = _browseEntries[index];
        if (entry.IsFolder || !entry.LaunchSpec.HasValue)
        {
            return false;
        }

        launchToken = AppLauncherFavoriteLaunchCodec.Encode(entry.LaunchSpec.Value);
        displayName = entry.Name;
        return true;
    }

    private static bool IsLaunchableFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Directory.Exists(path))
        {
            return false;
        }

        var extension = Path.GetExtension(path);
        return LaunchableExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveDisplayName(string displayName, LaunchSpec launchSpec)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        if (launchSpec.TargetType == LaunchTargetType.DirectPath)
        {
            return Path.GetFileNameWithoutExtension(launchSpec.Target);
        }

        if (launchSpec.TargetType == LaunchTargetType.ShellApp)
        {
            return launchSpec.Target;
        }

        return launchSpec.Target;
    }

    private void SelectFavoriteById(int id)
    {
        if (_appList == null)
        {
            return;
        }

        var index = _favorites.FindIndex(favorite => favorite.Id == id);
        if (index >= 0 && index < _appList.Items.Count)
        {
            _appList.SelectedIndex = index;
        }
    }

    private void AttachHandlers()
    {
        if (_appList != null)
        {
            _appList.SelectionChanged += OnSelectionChanged;
        }

        if (_addButton != null)
        {
            _addButton.Click += OnAddButtonClick;
        }

        if (_removeButton != null)
        {
            _removeButton.Click += OnRemoveButtonClick;
        }

        if (_launchButton != null)
        {
            _launchButton.Click += OnLaunchButtonClick;
        }
    }

    private void DetachHandlers()
    {
        if (_appList != null)
        {
            _appList.SelectionChanged -= OnSelectionChanged;
        }

        if (_addButton != null)
        {
            _addButton.Click -= OnAddButtonClick;
        }

        if (_removeButton != null)
        {
            _removeButton.Click -= OnRemoveButtonClick;
        }

        if (_launchButton != null)
        {
            _launchButton.Click -= OnLaunchButtonClick;
        }
    }

    private void OnAddButtonClick(object sender, RoutedEventArgs e)
    {
        AddFavorite();
    }

    private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
    {
        RemoveFavorite();
    }

    private void OnLaunchButtonClick(object sender, RoutedEventArgs e)
    {
        if (_isBrowseMode)
        {
            ActivateBrowseSelection();
            return;
        }

        LaunchSelectedFavorite();
    }

    private static string GetDefaultBrowseDirectory()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (Directory.Exists(desktop))
        {
            return desktop;
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    }
}
