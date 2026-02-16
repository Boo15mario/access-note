using System.Linq;

namespace AccessNote.Tests;

public class DateTimeArchitectureTests
{
    [Fact]
    public void DateTimeAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(DateTimeModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
