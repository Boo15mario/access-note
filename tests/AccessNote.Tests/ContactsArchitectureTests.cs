using System.Linq;

namespace AccessNote.Tests;

public class ContactsArchitectureTests
{
    [Fact]
    public void ContactsAppletAssembly_DoesNotDependOnHost()
    {
        var references = typeof(ContactsModule)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("AccessNote.Core", references);
        Assert.Contains("AccessNote.Shell", references);
        Assert.DoesNotContain("AccessNote", references);
    }
}
