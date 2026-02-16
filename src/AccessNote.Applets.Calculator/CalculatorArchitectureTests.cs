using System.Linq;

namespace AccessNote.Tests;

public class CalculatorArchitectureTests
{
    [Fact]
    public void CalculatorAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(CalculatorModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
