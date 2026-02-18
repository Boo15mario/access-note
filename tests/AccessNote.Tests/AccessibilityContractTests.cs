using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AccessNote.Tests;

public sealed class AccessibilityContractTests
{
    [Fact]
    public void AccessibilityContractDocument_Exists_WithRequiredSections()
    {
        var repoRoot = FindRepositoryRoot();
        var contractPath = Path.Combine(repoRoot, "docs", "plans", "2026-02-18-accessibility-contract-v1.md");

        Assert.True(File.Exists(contractPath), $"Missing accessibility contract doc: {contractPath}");

        var markdown = File.ReadAllText(contractPath);
        Assert.Contains("# Accessibility Contract v1", markdown);
        Assert.Contains("## Core Rules", markdown);
        Assert.Contains("### 3) Single-Call Announcement Rule", markdown);
        Assert.Contains("## F1 Help Contract", markdown);
    }

    [Fact]
    public void AppletDescriptors_DoNotUsePlaceholderHelpText()
    {
        var repoRoot = FindRepositoryRoot();
        var appletFiles = Directory
            .EnumerateFiles(Path.Combine(repoRoot, "src"), "*Applet.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains("AccessNote.Applets.", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(appletFiles);

        foreach (var file in appletFiles)
        {
            var content = File.ReadAllText(file);
            var helpTextMatch = Regex.Match(content, "helpText:\\s*\"([^\"]+)\"");
            Assert.True(helpTextMatch.Success, $"Could not find helpText descriptor in {file}.");
            HelpTextContractValidator.AssertHelpText(helpTextMatch.Groups[1].Value, file);

            var hintTextMatch = Regex.Match(content, "screenHintText:\\s*\"([^\"]+)\"");
            Assert.True(hintTextMatch.Success, $"Could not find screenHintText descriptor in {file}.");
            HelpTextContractValidator.AssertHintText(hintTextMatch.Groups[1].Value, file);
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var hasPlan = File.Exists(Path.Combine(current.FullName, "plan.md"));
            var hasSource = Directory.Exists(Path.Combine(current.FullName, "src"));
            if (hasPlan && hasSource)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found from test base directory.");
    }
}
