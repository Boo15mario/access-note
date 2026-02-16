using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AccessNote;

public sealed class CalendarEventStorage : ICalendarEventStorage
{
    private const string TableSql = """
        CREATE TABLE IF NOT EXISTS calendar_events (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            date TEXT NOT NULL,
            time TEXT,
            description TEXT
        );
        """;

    private readonly string _databasePath;

    public CalendarEventStorage(string databasePath)
    {
        _databasePath = databasePath;
    }

    public IReadOnlyList<CalendarEvent> GetEventsForDate(DateTime date)
    {
        EnsureDatabase();

        var events = new List<CalendarEvent>();
        var dateKey = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, date, time, description
            FROM calendar_events
            WHERE date = $date
            ORDER BY time IS NULL, time, title;
            """;
        command.Parameters.AddWithValue("$date", dateKey);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(ReadEvent(reader));
        }

        return events;
    }

    public IReadOnlyList<CalendarEvent> GetEventsForMonth(int year, int month)
    {
        EnsureDatabase();

        var events = new List<CalendarEvent>();
        var prefix = $"{year:D4}-{month:D2}";

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, date, time, description
            FROM calendar_events
            WHERE date LIKE $prefix || '%'
            ORDER BY date, time IS NULL, time, title;
            """;
        command.Parameters.AddWithValue("$prefix", prefix);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(ReadEvent(reader));
        }

        return events;
    }

    public void AddEvent(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO calendar_events (title, date, time, description)
            VALUES ($title, $date, $time, $description);
            """;
        command.Parameters.AddWithValue("$title", calendarEvent.Title);
        command.Parameters.AddWithValue("$date", calendarEvent.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$time", calendarEvent.Time.HasValue
            ? calendarEvent.Time.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
            : (object)DBNull.Value);
        command.Parameters.AddWithValue("$description", (object?)calendarEvent.Description ?? DBNull.Value);
        command.ExecuteNonQuery();

        calendarEvent.Id = (int)(long)new SqliteCommand("SELECT last_insert_rowid();", connection).ExecuteScalar()!;
    }

    public void UpdateEvent(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE calendar_events
            SET title = $title, date = $date, time = $time, description = $description
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", calendarEvent.Id);
        command.Parameters.AddWithValue("$title", calendarEvent.Title);
        command.Parameters.AddWithValue("$date", calendarEvent.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$time", calendarEvent.Time.HasValue
            ? calendarEvent.Time.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
            : (object)DBNull.Value);
        command.Parameters.AddWithValue("$description", (object?)calendarEvent.Description ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void DeleteEvent(int id)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM calendar_events WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static CalendarEvent ReadEvent(SqliteDataReader reader)
    {
        var evt = new CalendarEvent
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Date = DateTime.ParseExact(reader.GetString(2), "yyyy-MM-dd", CultureInfo.InvariantCulture),
        };

        if (!reader.IsDBNull(3))
        {
            var timeStr = reader.GetString(3);
            if (TimeSpan.TryParseExact(timeStr, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                evt.Time = time;
            }
        }

        if (!reader.IsDBNull(4))
        {
            evt.Description = reader.GetString(4);
        }

        return evt;
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
        create.CommandText = TableSql;
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
}
