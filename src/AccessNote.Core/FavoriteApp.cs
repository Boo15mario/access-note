namespace AccessNote;

public sealed class FavoriteApp
{
    public FavoriteApp(int id, string name, string path, string? arguments = null)
    {
        Id = id;
        Name = name;
        Path = path;
        Arguments = arguments;
    }

    public int Id { get; }

    public string Name { get; set; }

    public string Path { get; }

    public string? Arguments { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? System.IO.Path.GetFileNameWithoutExtension(Path) : Name;

    public override string ToString() => DisplayName;
}
