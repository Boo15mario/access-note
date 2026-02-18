namespace AccessNote.Tests;

internal static class HelpTextContractValidator
{
    private const string PlaceholderHelpText = "Help is not available.";

    public static void AssertHelpText(string helpText, string source)
    {
        Assert.False(string.IsNullOrWhiteSpace(helpText), $"Missing help text in {source}.");
        Assert.NotEqual(PlaceholderHelpText, helpText.Trim());
        Assert.Contains("Escape", helpText, StringComparison.OrdinalIgnoreCase);
    }

    public static void AssertHintText(string hintText, string source)
    {
        Assert.False(string.IsNullOrWhiteSpace(hintText), $"Missing hint text in {source}.");
    }
}
