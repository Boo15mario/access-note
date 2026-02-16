using System.Linq;

namespace AccessNote.Tests;

public class AppLauncherArchitectureTests
{
    [Fact]
    public void AppLauncherAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(AppLauncherModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
