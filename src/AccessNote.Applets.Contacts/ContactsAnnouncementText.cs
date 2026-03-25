namespace AccessNote;

internal static class ContactsAnnouncementText
{
    public static string FocusContactsList()
    {
        return "Contacts list.";
    }

    public static string FocusContactForm()
    {
        return "Contact form.";
    }

    public static string FocusContactActions()
    {
        return "Contact actions.";
    }

    public static string ContactsShown(int count)
    {
        if (count == 0)
        {
            return "No contacts match your search.";
        }

        return $"{count} contact{(count == 1 ? "" : "s")} shown.";
    }

    public static string SearchApplied(string query, int count)
    {
        if (count == 0)
        {
            return $"Search {query}. No contacts match your search.";
        }

        return $"Search {query}. {count} contact{(count == 1 ? "" : "s")} shown.";
    }

    public static string FilterApplied(string filter, int count)
    {
        if (count == 0)
        {
            return $"Group {filter}. No contacts in this group.";
        }

        return $"Group {filter}. {count} contact{(count == 1 ? "" : "s")} shown.";
    }
}
