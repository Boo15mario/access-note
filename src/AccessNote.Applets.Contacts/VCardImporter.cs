using System;
using System.Collections.Generic;
using System.IO;

namespace AccessNote;

internal static class VCardImporter
{
    public static IReadOnlyList<Contact> Import(string filePath)
    {
        var contacts = new List<Contact>();
        var lines = File.ReadAllLines(filePath);
        Contact? current = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.Equals("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase))
            {
                current = new Contact();
                continue;
            }

            if (line.Equals("END:VCARD", StringComparison.OrdinalIgnoreCase))
            {
                if (current != null)
                {
                    contacts.Add(current);
                    current = null;
                }
                continue;
            }

            if (current == null) continue;

            var colonIndex = line.IndexOf(':');
            if (colonIndex < 0) continue;

            var propertyPart = line[..colonIndex];
            var valuePart = line[(colonIndex + 1)..];

            // Strip parameters (e.g., TEL;TYPE=WORK:123)
            var propertyName = propertyPart.Split(';')[0].ToUpperInvariant();

            switch (propertyName)
            {
                case "N":
                    // N:LastName;FirstName;MiddleName;Prefix;Suffix
                    var parts = valuePart.Split(';');
                    if (parts.Length >= 1) current.LastName = parts[0];
                    if (parts.Length >= 2) current.FirstName = parts[1];
                    break;

                case "FN":
                    // Only use FN if N wasn't already parsed
                    if (string.IsNullOrEmpty(current.FirstName) && string.IsNullOrEmpty(current.LastName))
                    {
                        var fnParts = valuePart.Split(' ', 2);
                        current.FirstName = fnParts[0];
                        if (fnParts.Length > 1) current.LastName = fnParts[1];
                    }
                    break;

                case "TEL":
                    if (string.IsNullOrEmpty(current.Phone))
                        current.Phone = valuePart;
                    break;

                case "EMAIL":
                    if (string.IsNullOrEmpty(current.Email))
                        current.Email = valuePart;
                    break;

                case "ADR":
                    // ADR:;;Street;City;State;Zip;Country
                    if (string.IsNullOrEmpty(current.Address))
                    {
                        var adrParts = valuePart.Split(';');
                        var nonEmpty = new List<string>();
                        foreach (var p in adrParts)
                        {
                            if (!string.IsNullOrWhiteSpace(p)) nonEmpty.Add(p.Trim());
                        }
                        current.Address = string.Join(", ", nonEmpty);
                    }
                    break;

                case "NOTE":
                    if (string.IsNullOrEmpty(current.Notes))
                        current.Notes = valuePart.Replace("\\n", Environment.NewLine);
                    break;
            }
        }

        return contacts;
    }
}
