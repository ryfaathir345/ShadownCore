using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using WinTweakStudio.Models;

namespace WinTweakStudio.Data
{
    public class DatabaseInitializer
    {
        private static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_data.db");
        private static string ConnectionString => $"Data Source={DbPath};";

        public static void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var createTablesCmd = connection.CreateCommand();
            createTablesCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS RestorePoints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CreatedAt TEXT NOT NULL,
                    Label TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS TweakLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RestorePointId INTEGER NOT NULL,
                    TweakName TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    TargetPath TEXT NOT NULL,
                    OldValue TEXT NOT NULL,
                    NewValue TEXT NOT NULL,
                    AppliedAt TEXT NOT NULL,
                    IsReverted INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY(RestorePointId) REFERENCES RestorePoints(Id) ON DELETE CASCADE
                );
            ";
            createTablesCmd.ExecuteNonQuery();

            EnsureTweakDefinitionsTable(connection);
            EnsureInitialRestorePoint(connection);
            DbSeeder.Seed(connection);
        }

        private static void EnsureTweakDefinitionsTable(SqliteConnection connection)
        {
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='TweakDefinitions';";
            var tableExists = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

            if (tableExists)
            {
                var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA table_info(TweakDefinitions);";
                using var reader = pragmaCmd.ExecuteReader();
                bool hasRequiresSecurityWarning = false;
                while (reader.Read())
                {
                    var colName = reader.GetString(1);
                    if (string.Equals(colName, "RequiresSecurityWarning", StringComparison.OrdinalIgnoreCase))
                    {
                        hasRequiresSecurityWarning = true;
                        break;
                    }
                }

                if (!hasRequiresSecurityWarning)
                {
                    var dropCmd = connection.CreateCommand();
                    dropCmd.CommandText = "DROP TABLE TweakDefinitions;";
                    dropCmd.ExecuteNonQuery();
                }
            }

            var createCmd = connection.CreateCommand();
            createCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TweakDefinitions (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    SubCategory TEXT,
                    RiskLevel TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    TargetPath TEXT NOT NULL,
                    ValueName TEXT NOT NULL DEFAULT '',
                    DefaultValue TEXT NOT NULL DEFAULT '',
                    RecommendedValue TEXT NOT NULL DEFAULT '',
                    RequiresSecurityWarning INTEGER NOT NULL DEFAULT 0
                );
            ";
            createCmd.ExecuteNonQuery();
        }

        private static void EnsureInitialRestorePoint(SqliteConnection connection)
        {
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM RestorePoints;";
            var count = Convert.ToInt64(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO RestorePoints (CreatedAt, Label, IsActive)
                    VALUES (@CreatedAt, @Label, 1);
                ";
                insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertCmd.Parameters.AddWithValue("@Label", "Initial System Baseline");
                insertCmd.ExecuteNonQuery();
            }
        }

        public static long GetOrCreateActiveRestorePointId()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id FROM RestorePoints WHERE IsActive = 1 ORDER BY Id DESC LIMIT 1;";
            var result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt64(result);
            }

            var createCmd = connection.CreateCommand();
            createCmd.CommandText = @"
                INSERT INTO RestorePoints (CreatedAt, Label, IsActive)
                VALUES (@CreatedAt, @Label, 1);
                SELECT last_insert_rowid();
            ";
            createCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            createCmd.Parameters.AddWithValue("@Label", $"Restore Point - {DateTime.Now:MMM dd, HH:mm}");
            return Convert.ToInt64(createCmd.ExecuteScalar());
        }

        public static long CreateRestorePoint(string label)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var createCmd = connection.CreateCommand();
            createCmd.CommandText = @"
                INSERT INTO RestorePoints (CreatedAt, Label, IsActive)
                VALUES (@CreatedAt, @Label, 1);
                SELECT last_insert_rowid();
            ";
            createCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            createCmd.Parameters.AddWithValue("@Label", label);
            return Convert.ToInt64(createCmd.ExecuteScalar());
        }

        public static void LogTweakApplicationAtomic(TweakLog log)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO TweakLogs 
                    (RestorePointId, TweakName, Category, Type, TargetPath, OldValue, NewValue, AppliedAt, IsReverted)
                    VALUES 
                    (@RestorePointId, @TweakName, @Category, @Type, @TargetPath, @OldValue, @NewValue, @AppliedAt, @IsReverted);
                ";
                cmd.Parameters.AddWithValue("@RestorePointId", log.RestorePointId);
                cmd.Parameters.AddWithValue("@TweakName", log.TweakName);
                cmd.Parameters.AddWithValue("@Category", log.Category);
                cmd.Parameters.AddWithValue("@Type", log.Type);
                cmd.Parameters.AddWithValue("@TargetPath", log.TargetPath);
                cmd.Parameters.AddWithValue("@OldValue", log.OldValue);
                cmd.Parameters.AddWithValue("@NewValue", log.NewValue);
                cmd.Parameters.AddWithValue("@AppliedAt", log.AppliedAt);
                cmd.Parameters.AddWithValue("@IsReverted", log.IsReverted ? 1 : 0);

                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static List<TweakLog> GetAllTweakLogs()
        {
            var logs = new List<TweakLog>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, RestorePointId, TweakName, Category, Type, TargetPath, OldValue, NewValue, AppliedAt, IsReverted FROM TweakLogs ORDER BY Id DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new TweakLog
                {
                    Id = reader.GetInt64(0),
                    RestorePointId = reader.GetInt64(1),
                    TweakName = reader.GetString(2),
                    Category = reader.GetString(3),
                    Type = reader.GetString(4),
                    TargetPath = reader.GetString(5),
                    OldValue = reader.GetString(6),
                    NewValue = reader.GetString(7),
                    AppliedAt = reader.GetString(8),
                    IsReverted = reader.GetInt32(9) == 1
                });
            }

            return logs;
        }

        public static List<RestorePoint> GetAllRestorePoints()
        {
            var points = new List<RestorePoint>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, CreatedAt, Label, IsActive FROM RestorePoints ORDER BY Id DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                points.Add(new RestorePoint
                {
                    Id = reader.GetInt64(0),
                    CreatedAt = reader.GetString(1),
                    Label = reader.GetString(2),
                    IsActive = reader.GetInt32(3) == 1
                });
            }

            return points;
        }

        public static void MarkLogReverted(long logId, bool isReverted = true)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE TweakLogs SET IsReverted = @IsReverted WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@IsReverted", isReverted ? 1 : 0);
            cmd.Parameters.AddWithValue("@Id", logId);
            cmd.ExecuteNonQuery();
        }

        public static List<TweakDefinition> GetTweakDefinitionsByCategory(TweakCategory category)
        {
            var list = new List<TweakDefinition>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Description, Category, SubCategory, RiskLevel, Type, TargetPath, ValueName, DefaultValue, RecommendedValue, RequiresSecurityWarning FROM TweakDefinitions WHERE Category = @Category;";
            cmd.Parameters.AddWithValue("@Category", category.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TweakDefinition
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Category = Enum.TryParse<TweakCategory>(reader.GetString(3), out var cat) ? cat : category,
                    SubCategory = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    RiskLevel = Enum.TryParse<RiskLevel>(reader.GetString(5), out var risk) ? risk : RiskLevel.Safe,
                    Type = Enum.TryParse<TweakType>(reader.GetString(6), out var tType) ? tType : TweakType.Registry,
                    TargetPath = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    ValueName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    DefaultValue = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    RecommendedValue = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    RequiresSecurityWarning = !reader.IsDBNull(11) && reader.GetInt32(11) == 1
                });
            }

            return list;
        }
    }
}
