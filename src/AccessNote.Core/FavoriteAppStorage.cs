using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AccessNote;

public sealed class FavoriteAppStorage
{
    private const string TableSql = """
        CREATE TABLE IF NOT EXISTS favorite_apps (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            path TEXT NOT NULL,
            arguments TEXT,
            sort_order INTEGER
        );
        """;

    private readonly string _databasePath;

    public FavoriteAppStorage(string databasePath)
    {
        _databasePath = databasePath;
    }

    public IReadOnlyList<FavoriteApp> GetAll()
    {
        EnsureDatabase();

        var apps = new List<FavoriteApp>();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, path, arguments
            FROM favorite_apps
            ORDER BY sort_order, id;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var path = reader.GetString(2);
            var arguments = reader.IsDBNull(3) ? null : reader.GetString(3);
            apps.Add(new FavoriteApp(id, name, path, arguments));
        }

        return apps;
    }

    public FavoriteApp Add(string name, string path, string? arguments = null)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var maxOrder = connection.CreateCommand();
        maxOrder.CommandText = "SELECT COALESCE(MAX(sort_order), 0) + 1 FROM favorite_apps;";
        var nextOrder = Convert.ToInt32(maxOrder.ExecuteScalar());

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO favorite_apps (name, path, arguments, sort_order)
            VALUES ($name, $path, $arguments, $sort_order);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$path", path);
        command.Parameters.AddWithValue("$arguments", (object?)arguments ?? DBNull.Value);
        command.Parameters.AddWithValue("$sort_order", nextOrder);

        var id = Convert.ToInt32(command.ExecuteScalar());
        return new FavoriteApp(id, name, path, arguments);
    }

    public void Update(FavoriteApp app)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE favorite_apps
            SET name = $name, path = $path, arguments = $arguments
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", app.Id);
        command.Parameters.AddWithValue("$name", app.Name);
        command.Parameters.AddWithValue("$path", app.Path);
        command.Parameters.AddWithValue("$arguments", (object?)app.Arguments ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM favorite_apps WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Reorder(IReadOnlyList<int> orderedIds)
    {
        ArgumentNullException.ThrowIfNull(orderedIds);
        EnsureDatabase();

        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE favorite_apps SET sort_order = $order WHERE id = $id;";
            command.Parameters.AddWithValue("$order", i);
            command.Parameters.AddWithValue("$id", orderedIds[i]);
            command.ExecuteNonQuery();
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
