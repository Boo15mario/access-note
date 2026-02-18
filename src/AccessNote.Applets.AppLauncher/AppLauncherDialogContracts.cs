namespace AccessNote;

internal enum AddFavoriteSource
{
    CurrentSelection,
    Browse
}

internal readonly record struct AddFavoriteRequest(
    bool CanUseCurrentSelection,
    string CurrentSelectionPath,
    string CurrentSelectionDisplayName,
    string InitialBrowseDirectory);

internal readonly record struct AddFavoriteResult(
    AddFavoriteSource Source,
    string Path,
    string DisplayName);

internal interface IAppLauncherDialogService
{
    bool TryPromptAddFavorite(AddFavoriteRequest request, out AddFavoriteResult result);
}
