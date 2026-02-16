using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AccessNote;

internal static class VCardExporter
{
    public static void Export(IEnumerable<Contact> contacts, string filePath)
    {
        var sb = new StringBuilder();

        foreach (var contact in contacts)
        {
            sb.AppendLine("BEGIN:VCARD");
            sb.AppendLine("VERSION:3.0");
            sb.Append("N:").Append(Escape(contact.LastName)).Append(';').AppendLine(Escape(contact.FirstName));
            sb.Append("FN:").AppendLine(Escape(contact.DisplayName));

            if (!string.IsNullOrEmpty(contact.Phone))
                sb.Append("TEL:").AppendLine(Escape(contact.Phone));

            if (!string.IsNullOrEmpty(contact.Email))
                sb.Append("EMAIL:").AppendLine(Escape(contact.Email));

            if (!string.IsNullOrEmpty(contact.Address))
                sb.Append("ADR:;;").Append(Escape(contact.Address)).AppendLine(";;;;");

            if (!string.IsNullOrEmpty(contact.Notes))
                sb.Append("NOTE:").AppendLine(Escape(contact.Notes));

            sb.AppendLine("END:VCARD");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
    }
}
