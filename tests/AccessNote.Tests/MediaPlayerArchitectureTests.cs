using System.Linq;

namespace AccessNote.Tests;

public class MediaPlayerArchitectureTests
{
    [Fact]
    public void MediaPlayerAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(MediaPlayerModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
