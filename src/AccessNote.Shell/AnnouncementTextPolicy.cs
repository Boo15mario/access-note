using System.Text;

namespace AccessNote;

internal static class AnnouncementTextPolicy
{
    public static string Normalize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Ready.";
        }

        return CollapseWhitespace(message.Trim());
    }

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            previousWasWhitespace = false;
            builder.Append(character);
        }

        return builder.ToString();
    }
}
