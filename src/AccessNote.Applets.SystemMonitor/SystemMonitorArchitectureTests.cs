using System.Linq;

namespace AccessNote.Tests;

public class SystemMonitorArchitectureTests
{
    [Fact]
    public void SystemMonitorAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(SystemMonitorModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
