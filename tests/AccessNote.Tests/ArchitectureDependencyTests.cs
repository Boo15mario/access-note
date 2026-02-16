using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AccessNote.Tests;

public class ArchitectureDependencyTests
{
    [Fact]
    public void CoreAssembly_DoesNotDependOnShellHostOrApplets()
    {
        var references = GetReferencedAssemblyNames(typeof(AppSettings).Assembly);

        Assert.DoesNotContain("AccessNote", references);
        Assert.DoesNotContain("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote.Applets.Notes", references);
        Assert.DoesNotContain("AccessNote.Applets.Settings", references);
    }

    [Fact]
    public void ShellAssembly_DoesNotDependOnHostOrApplets()
    {
        var references = GetReferencedAssemblyNames(typeof(ScreenRouter).Assembly);

        Assert.DoesNotContain("AccessNote", references);
        Assert.DoesNotContain("AccessNote.Applets.Notes", references);
        Assert.DoesNotContain("AccessNote.Applets.Settings", references);
    }

    [Fact]
    public void NotesAppletAssembly_DependsOnCoreAndShellButNotHost()
    {
        var references = GetReferencedAssemblyNames(typeof(NotesModule).Assembly);

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
        Assert.DoesNotContain("AccessNote.Applets.Settings", references);
    }

    [Fact]
    public void SettingsAppletAssembly_DependsOnCoreAndShellButNotHost()
    {
        var references = GetReferencedAssemblyNames(typeof(SettingsModule).Assembly);

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
        Assert.DoesNotContain("AccessNote.Applets.Notes", references);
    }

    [Fact]
    public void HostAssembly_DependenciesIncludeCoreShellAndBothApplets()
    {
        var references = GetReferencedAssemblyNames(typeof(MainWindow).Assembly);

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.Contains("AccessNote.Applets.Notes", references);
        Assert.Contains("AccessNote.Applets.Settings", references);
    }

    [Fact]
    public void ShellAssembly_GrantsInternalsOnlyToHostAndTests()
    {
        var grants = GetInternalsVisibleToTargets(typeof(ScreenRouter).Assembly);

        Assert.Contains("AccessNote", grants);
        Assert.Contains("AccessNote.Tests", grants);
        Assert.DoesNotContain("AccessNote.Applets.Notes", grants);
        Assert.DoesNotContain("AccessNote.Applets.Settings", grants);
    }

    [Fact]
    public void ShellAssembly_PublicSurfaceIsLimitedToAppletContracts()
    {
        var publicTypes = typeof(ScreenRouter).Assembly
            .GetTypes()
            .Where(type => type.IsPublic)
            .Select(type => type.Name)
            .OrderBy(name => name)
            .ToArray();

        var expectedPublicTypes = new[]
        {
            nameof(AppletCategory),
            nameof(AppletDescriptor),
            nameof(AppletId),
            nameof(AppletRegistrationContext),
            nameof(IApplet),
            nameof(IAppletRegistration),
            nameof(ShellViewAdapter),
        };

        Assert.Equal(expectedPublicTypes, publicTypes);
    }

    private static IReadOnlyCollection<string> GetReferencedAssemblyNames(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray();
    }

    private static IReadOnlyCollection<string> GetInternalsVisibleToTargets(Assembly assembly)
    {
        return assembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName)
            .ToArray();
    }
}
