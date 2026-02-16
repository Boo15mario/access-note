using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AccessNote;

public sealed class NoteStorage : INoteStorage
{
    private const string NotesTableSql = """
        CREATE TABLE IF NOT EXISTS notes (
            id TEXT PRIMARY KEY,
            title TEXT NOT NULL,
            content TEXT NOT NULL,
            last_modified_utc TEXT NOT NULL
        );
        """;

    private readonly string _databasePath;

    public NoteStorage(string databasePath)
    {
        _databasePath = databasePath;
    }

    public static string GetDefaultDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "AccessNote");
        return Path.Combine(dir, "access-note.db");
    }

    public IReadOnlyList<NoteDocument> LoadNotes()
    {
        EnsureDatabase();

        var notes = new List<NoteDocument>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, content, last_modified_utc
            FROM notes
            ORDER BY last_modified_utc DESC;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetString(0);
            var title = reader.GetString(1);
            var content = reader.GetString(2);
            var modifiedRaw = reader.GetString(3);

            var modified = TryParseUtc(modifiedRaw);
            notes.Add(NoteDocument.FromPersisted(id, title, content, modified));
        }

        return notes;
    }

    public void SaveNotes(IEnumerable<NoteDocument> notes)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var clear = connection.CreateCommand();
        clear.Transaction = transaction;
        clear.CommandText = "DELETE FROM notes;";
        clear.ExecuteNonQuery();

        foreach (var note in notes)
        {
            var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO notes (id, title, content, last_modified_utc)
                VALUES ($id, $title, $content, $last_modified_utc);
                """;
            insert.Parameters.AddWithValue("$id", note.Id);
            insert.Parameters.AddWithValue("$title", note.PersistedTitle);
            insert.Parameters.AddWithValue("$content", note.PersistedContent);
            insert.Parameters.AddWithValue(
                "$last_modified_utc",
                note.PersistedLastModifiedUtc.ToString("O", CultureInfo.InvariantCulture));
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private void EnsureDatabase()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = CreateConnection();
        connection.Open();

        var create = connection.CreateCommand();
        create.CommandText = NotesTableSql;
        create.ExecuteNonQuery();
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

    private static DateTime TryParseUtc(string value)
    {
        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }
}
