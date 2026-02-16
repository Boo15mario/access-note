using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AccessNote;

public sealed class SettingsStorage : ISettingsStorage
{
    private const string SettingsTableSql = """
        CREATE TABLE IF NOT EXISTS app_settings (
            key TEXT PRIMARY KEY,
            value TEXT NOT NULL
        );
        """;

    private const string StartScreenKey = "start_screen";
    private const string NotesInitialFocusKey = "notes_initial_focus";
    private const string ConfirmBeforeDeleteNoteKey = "confirm_before_delete_note";
    private const string NotesSortOrderKey = "notes_sort_order";
    private const string AnnounceStatusMessagesKey = "announce_status_messages";
    private const string AnnounceHintsOnScreenOpenKey = "announce_hints_on_screen_open";
    private const string SoundsEnabledKey = "sounds_enabled";
    private const string ThemeNameKey = "theme_name";

    private readonly string _databasePath;

    public SettingsStorage(string databasePath)
    {
        _databasePath = databasePath;
    }

    public AppSettings LoadSettings()
    {
        EnsureDatabase();

        var settings = AppSettings.CreateDefault();
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT key, value
            FROM app_settings;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            values[reader.GetString(0)] = reader.GetString(1);
        }

        if (values.TryGetValue(StartScreenKey, out var startScreenValue) &&
            Enum.TryParse<StartScreenOption>(startScreenValue, ignoreCase: true, out var startScreen))
        {
            settings.StartScreen = startScreen;
        }

        if (values.TryGetValue(NotesInitialFocusKey, out var notesInitialFocusValue) &&
            Enum.TryParse<NotesInitialFocusOption>(notesInitialFocusValue, ignoreCase: true, out var notesInitialFocus))
        {
            settings.NotesInitialFocus = notesInitialFocus;
        }

        if (values.TryGetValue(ConfirmBeforeDeleteNoteKey, out var confirmDeleteValue) &&
            bool.TryParse(confirmDeleteValue, out var confirmDelete))
        {
            settings.ConfirmBeforeDeleteNote = confirmDelete;
        }

        if (values.TryGetValue(NotesSortOrderKey, out var notesSortOrderValue) &&
            Enum.TryParse<NotesSortOrderOption>(notesSortOrderValue, ignoreCase: true, out var notesSortOrder))
        {
            settings.NotesSortOrder = notesSortOrder;
        }

        if (values.TryGetValue(AnnounceStatusMessagesKey, out var announceStatusMessagesValue) &&
            bool.TryParse(announceStatusMessagesValue, out var announceStatusMessages))
        {
            settings.AnnounceStatusMessages = announceStatusMessages;
        }

        if (values.TryGetValue(AnnounceHintsOnScreenOpenKey, out var announceHintsValue) &&
            bool.TryParse(announceHintsValue, out var announceHints))
        {
            settings.AnnounceHintsOnScreenOpen = announceHints;
        }

        if (values.TryGetValue(SoundsEnabledKey, out var soundsEnabledValue) &&
            bool.TryParse(soundsEnabledValue, out var soundsEnabled))
        {
            settings.SoundsEnabled = soundsEnabled;
        }

        if (values.TryGetValue(ThemeNameKey, out var themeNameValue) &&
            !string.IsNullOrWhiteSpace(themeNameValue))
        {
            settings.ThemeName = themeNameValue;
        }

        return settings;
    }

    public void SaveSettings(AppSettings settings)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        UpsertSetting(connection, transaction, StartScreenKey, settings.StartScreen.ToString());
        UpsertSetting(connection, transaction, NotesInitialFocusKey, settings.NotesInitialFocus.ToString());
        UpsertSetting(connection, transaction, ConfirmBeforeDeleteNoteKey, settings.ConfirmBeforeDeleteNote.ToString());
        UpsertSetting(connection, transaction, NotesSortOrderKey, settings.NotesSortOrder.ToString());
        UpsertSetting(connection, transaction, AnnounceStatusMessagesKey, settings.AnnounceStatusMessages.ToString());
        UpsertSetting(connection, transaction, AnnounceHintsOnScreenOpenKey, settings.AnnounceHintsOnScreenOpen.ToString());
        UpsertSetting(connection, transaction, SoundsEnabledKey, settings.SoundsEnabled.ToString());
        UpsertSetting(connection, transaction, ThemeNameKey, settings.ThemeName);

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
        create.CommandText = SettingsTableSql;
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

    private static void UpsertSetting(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string key,
        string value)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO app_settings (key, value)
            VALUES ($key, $value)
            ON CONFLICT(key) DO UPDATE SET value = excluded.value;
            """;
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }
}
