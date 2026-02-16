using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace AccessNote;

public sealed class ContactStorage
{
    private const string ContactsTableSql = """
        CREATE TABLE IF NOT EXISTS contacts (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            first_name TEXT NOT NULL DEFAULT '',
            last_name TEXT NOT NULL DEFAULT '',
            phone TEXT NOT NULL DEFAULT '',
            email TEXT NOT NULL DEFAULT '',
            address TEXT NOT NULL DEFAULT '',
            notes TEXT NOT NULL DEFAULT '',
            group_id INTEGER
        );
        """;

    private const string GroupsTableSql = """
        CREATE TABLE IF NOT EXISTS contact_groups (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL
        );
        """;

    private readonly string _databasePath;

    public ContactStorage(string databasePath)
    {
        _databasePath = databasePath;
    }

    public IReadOnlyList<Contact> GetAllContacts()
    {
        EnsureDatabase();
        var contacts = new List<Contact>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.id, c.first_name, c.last_name, c.phone, c.email, c.address, c.notes,
                   COALESCE(g.name, '') AS group_name
            FROM contacts c
            LEFT JOIN contact_groups g ON c.group_id = g.id
            ORDER BY c.first_name, c.last_name;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contacts.Add(ReadContact(reader));
        }

        return contacts;
    }

    public IReadOnlyList<Contact> GetContactsByGroup(string groupName)
    {
        EnsureDatabase();
        var contacts = new List<Contact>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.id, c.first_name, c.last_name, c.phone, c.email, c.address, c.notes,
                   COALESCE(g.name, '') AS group_name
            FROM contacts c
            LEFT JOIN contact_groups g ON c.group_id = g.id
            WHERE g.name = $groupName
            ORDER BY c.first_name, c.last_name;
            """;
        command.Parameters.AddWithValue("$groupName", groupName);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contacts.Add(ReadContact(reader));
        }

        return contacts;
    }

    public IReadOnlyList<Contact> SearchContacts(string query)
    {
        EnsureDatabase();
        var contacts = new List<Contact>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        var pattern = $"%{query}%";
        command.CommandText = """
            SELECT c.id, c.first_name, c.last_name, c.phone, c.email, c.address, c.notes,
                   COALESCE(g.name, '') AS group_name
            FROM contacts c
            LEFT JOIN contact_groups g ON c.group_id = g.id
            WHERE c.first_name LIKE $pattern
               OR c.last_name LIKE $pattern
               OR c.phone LIKE $pattern
               OR c.email LIKE $pattern
            ORDER BY c.first_name, c.last_name;
            """;
        command.Parameters.AddWithValue("$pattern", pattern);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contacts.Add(ReadContact(reader));
        }

        return contacts;
    }

    public void AddContact(Contact contact)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        long? groupId = ResolveGroupId(connection, contact.GroupName);

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO contacts (first_name, last_name, phone, email, address, notes, group_id)
            VALUES ($firstName, $lastName, $phone, $email, $address, $notes, $groupId);
            """;
        command.Parameters.AddWithValue("$firstName", contact.FirstName);
        command.Parameters.AddWithValue("$lastName", contact.LastName);
        command.Parameters.AddWithValue("$phone", contact.Phone);
        command.Parameters.AddWithValue("$email", contact.Email);
        command.Parameters.AddWithValue("$address", contact.Address);
        command.Parameters.AddWithValue("$notes", contact.Notes);
        command.Parameters.AddWithValue("$groupId", (object?)groupId ?? DBNull.Value);
        command.ExecuteNonQuery();

        var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        contact.Id = (long)idCmd.ExecuteScalar()!;
    }

    public void UpdateContact(Contact contact)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        long? groupId = ResolveGroupId(connection, contact.GroupName);

        var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE contacts
            SET first_name = $firstName, last_name = $lastName, phone = $phone,
                email = $email, address = $address, notes = $notes, group_id = $groupId
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", contact.Id);
        command.Parameters.AddWithValue("$firstName", contact.FirstName);
        command.Parameters.AddWithValue("$lastName", contact.LastName);
        command.Parameters.AddWithValue("$phone", contact.Phone);
        command.Parameters.AddWithValue("$email", contact.Email);
        command.Parameters.AddWithValue("$address", contact.Address);
        command.Parameters.AddWithValue("$notes", contact.Notes);
        command.Parameters.AddWithValue("$groupId", (object?)groupId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void DeleteContact(long contactId)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM contacts WHERE id = $id;";
        command.Parameters.AddWithValue("$id", contactId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<ContactGroup> GetGroups()
    {
        EnsureDatabase();
        var groups = new List<ContactGroup>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM contact_groups ORDER BY name;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            groups.Add(new ContactGroup
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
            });
        }

        return groups;
    }

    public void AddGroup(ContactGroup group)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO contact_groups (name) VALUES ($name);";
        command.Parameters.AddWithValue("$name", group.Name);
        command.ExecuteNonQuery();

        var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        group.Id = (long)idCmd.ExecuteScalar()!;
    }

    public void DeleteGroup(long groupId)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var clearRefs = connection.CreateCommand();
        clearRefs.Transaction = transaction;
        clearRefs.CommandText = "UPDATE contacts SET group_id = NULL WHERE group_id = $groupId;";
        clearRefs.Parameters.AddWithValue("$groupId", groupId);
        clearRefs.ExecuteNonQuery();

        var delete = connection.CreateCommand();
        delete.Transaction = transaction;
        delete.CommandText = "DELETE FROM contact_groups WHERE id = $groupId;";
        delete.Parameters.AddWithValue("$groupId", groupId);
        delete.ExecuteNonQuery();

        transaction.Commit();
    }

    private void EnsureDatabase()
    {
        var directory = System.IO.Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        using var connection = CreateConnection();
        connection.Open();

        var create1 = connection.CreateCommand();
        create1.CommandText = GroupsTableSql;
        create1.ExecuteNonQuery();

        var create2 = connection.CreateCommand();
        create2.CommandText = ContactsTableSql;
        create2.ExecuteNonQuery();
    }

    private SqliteConnection CreateConnection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default
        };

        return new SqliteConnection(builder.ToString());
    }

    private static long? ResolveGroupId(SqliteConnection connection, string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return null;
        }

        var command = connection.CreateCommand();
        command.CommandText = "SELECT id FROM contact_groups WHERE name = $name;";
        command.Parameters.AddWithValue("$name", groupName);
        var result = command.ExecuteScalar();
        return result is long id ? id : null;
    }

    private static Contact ReadContact(SqliteDataReader reader)
    {
        return new Contact
        {
            Id = reader.GetInt64(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Phone = reader.GetString(3),
            Email = reader.GetString(4),
            Address = reader.GetString(5),
            Notes = reader.GetString(6),
            GroupName = reader.GetString(7),
        };
    }
}
