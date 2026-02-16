using System.Linq;

namespace AccessNote.Tests;

public class CalendarArchitectureTests
{
    [Fact]
    public void CalendarAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(CalendarModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
