using AccessNote;
using System.IO;
using System.Windows.Input;

namespace AccessNote.Tests;

public sealed class TrustedAppletRegistrationLoaderTests
{
    [Fact]
    public void Discover_WhenAllowlistFileDoesNotExist_ReturnsEmptyResult()
    {
        var pluginDirectory = CreateTempDirectory();
        try
        {
            var loader = new TrustedAppletRegistrationLoader();
            var allowlistPath = Path.Combine(pluginDirectory, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);

            var result = loader.Discover(pluginDirectory, allowlistPath);

            Assert.Empty(result.Registrations);
            Assert.Empty(result.Warnings);
        }
        finally
        {
            DeleteDirectory(pluginDirectory);
        }
    }

    [Fact]
    public void Discover_WhenAllowlistEntryIsInvalid_SkipsEntryAndReturnsWarning()
    {
        var pluginDirectory = CreateTempDirectory();
        try
        {
            var allowlistPath = Path.Combine(pluginDirectory, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);
            File.WriteAllLines(allowlistPath, new[]
            {
                "../outside.dll",
            });
            var loader = new TrustedAppletRegistrationLoader();

            var result = loader.Discover(pluginDirectory, allowlistPath);

            Assert.Empty(result.Registrations);
            Assert.Single(result.Warnings);
            Assert.Contains("Invalid allowlist entry", result.Warnings[0]);
        }
        finally
        {
            DeleteDirectory(pluginDirectory);
        }
    }

    [Fact]
    public void Discover_WhenAssemblyIsMissing_ReturnsWarningAndContinues()
    {
        var pluginDirectory = CreateTempDirectory();
        try
        {
            var allowlistPath = Path.Combine(pluginDirectory, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);
            File.WriteAllLines(allowlistPath, new[]
            {
                "Missing.Plugin.dll",
            });
            var loader = new TrustedAppletRegistrationLoader();

            var result = loader.Discover(pluginDirectory, allowlistPath);

            Assert.Empty(result.Registrations);
            Assert.Single(result.Warnings);
            Assert.Contains("not found", result.Warnings[0], StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectory(pluginDirectory);
        }
    }

    [Fact]
    public void Discover_WhenAllowlistedAssemblyContainsPublicRegistration_LoadsRegistration()
    {
        var pluginDirectory = CreateTempDirectory();
        try
        {
            var assemblyFileName = "AccessNote.Tests.PluginProbe.dll";
            var targetAssemblyPath = Path.Combine(pluginDirectory, assemblyFileName);
            File.Copy(typeof(TestPluginRegistration).Assembly.Location, targetAssemblyPath, overwrite: true);

            var allowlistPath = Path.Combine(pluginDirectory, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);
            File.WriteAllLines(allowlistPath, new[]
            {
                assemblyFileName,
                assemblyFileName, // Duplicate should be ignored.
            });
            var loader = new TrustedAppletRegistrationLoader();

            var result = loader.Discover(pluginDirectory, allowlistPath);

            Assert.Single(result.Registrations);
            Assert.Empty(result.Warnings);
            Assert.Equal(nameof(TestPluginRegistration), result.Registrations[0].GetType().Name);
        }
        finally
        {
            DeleteDirectory(pluginDirectory);
        }
    }

    [Fact]
    public void Discover_WhenAllowlistedAssemblyHasNoPublicRegistrations_ReturnsWarning()
    {
        var pluginDirectory = CreateTempDirectory();
        try
        {
            var assemblyFileName = "AccessNote.Applets.DateTime.dll";
            var targetAssemblyPath = Path.Combine(pluginDirectory, assemblyFileName);
            File.Copy(typeof(DateTimeScreenView).Assembly.Location, targetAssemblyPath, overwrite: true);

            var allowlistPath = Path.Combine(pluginDirectory, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);
            File.WriteAllLines(allowlistPath, new[]
            {
                assemblyFileName,
            });
            var loader = new TrustedAppletRegistrationLoader();

            var result = loader.Discover(pluginDirectory, allowlistPath);

            Assert.Empty(result.Registrations);
            Assert.Single(result.Warnings);
            Assert.Contains("No exported IAppletRegistration types found", result.Warnings[0]);
        }
        finally
        {
            DeleteDirectory(pluginDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AccessNote.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}

public sealed class TestPluginRegistration : IAppletRegistration
{
    public IApplet Create(AppletRegistrationContext context)
    {
        return new TestPluginApplet();
    }

    private sealed class TestPluginApplet : IApplet
    {
        public AppletDescriptor Descriptor { get; } = new(
            id: AppletId.Calendar,
            label: "Plugin Test",
            screenHintText: "Plugin test.",
            helpText: "Plugin test help.");

        public void Enter()
        {
        }

        public void RestoreFocus()
        {
        }

        public bool CanLeave()
        {
            return true;
        }

        public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
        {
            return false;
        }
    }
}
