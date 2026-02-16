using System.Linq;

namespace AccessNote.Tests;

public class MidiPlayerArchitectureTests
{
    [Fact]
    public void MidiPlayerAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(MidiPlayerModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
