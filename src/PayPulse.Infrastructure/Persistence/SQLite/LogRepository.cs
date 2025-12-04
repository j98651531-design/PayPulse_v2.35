using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.Infrastructure.Persistence.SQLite
{
    public class LogRepository : ILogRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public LogRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Logs (
 LogId TEXT PRIMARY KEY,
 TimestampUtc TEXT,
 Level TEXT,
 Message TEXT,
 ProfileId TEXT,
 Operation TEXT,
 Exception TEXT,
 CorrelationId TEXT
);";
                cmd.ExecuteNonQuery();
            
            // Lightweight migration to add UserId/UserName if missing
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "PRAGMA table_info(Logs);";
                var existingCols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var rd = checkCmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var name = rd["name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            existingCols.Add(name);
                        }
                    }
                }

                if (!existingCols.Contains("UserId"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE Logs ADD COLUMN UserId TEXT NULL;";
                        alter.ExecuteNonQuery();
                    }
                }

                if (!existingCols.Contains("UserName"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE Logs ADD COLUMN UserName TEXT NULL;";
                        alter.ExecuteNonQuery();
                    }
                }
            }
}
        }

        public void Insert(LogEntry log)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Logs (LogId, TimestampUtc, Level, Message, ProfileId, Operation, Exception, CorrelationId)
VALUES (@LogId, @TimestampUtc, @Level, @Message, @ProfileId, @Operation, @Exception, @CorrelationId);";
                cmd.Parameters.AddWithValue("@LogId", log.LogId);
                cmd.Parameters.AddWithValue("@TimestampUtc", log.TimestampUtc.ToString("o"));
                cmd.Parameters.AddWithValue("@Level", log.Level);
                cmd.Parameters.AddWithValue("@Message", log.Message);
                cmd.Parameters.AddWithValue("@ProfileId", (object)log.ProfileId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Operation", (object)log.Operation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Exception", (object)log.Exception ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CorrelationId", (object)log.CorrelationId ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public IList<LogEntry> Get(DateTime fromUtc, DateTime toUtc)
        {
            var list = new List<LogEntry>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT * FROM Logs
WHERE TimestampUtc >= @From AND TimestampUtc <= @To
ORDER BY TimestampUtc DESC;";
                cmd.Parameters.AddWithValue("@From", fromUtc.ToString("o"));
                cmd.Parameters.AddWithValue("@To", toUtc.ToString("o"));

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var l = new LogEntry();
                        l.LogId = rd["LogId"].ToString();
                        l.TimestampUtc = DateTime.Parse(rd["TimestampUtc"].ToString());
                        l.Level = rd["Level"] as string;
                        l.Message = rd["Message"] as string;
                        l.ProfileId = rd["ProfileId"] as string;
                        l.Operation = rd["Operation"] as string;
                        l.Exception = rd["Exception"] as string;
                        l.CorrelationId = rd["CorrelationId"] as string;
                        int idxUserId;
                        try { idxUserId = rd.GetOrdinal("UserId"); l.UserId = rd.IsDBNull(idxUserId) ? null : rd.GetString(idxUserId); }
                        catch (IndexOutOfRangeException) { }

                        int idxUserName;
                        try { idxUserName = rd.GetOrdinal("UserName"); l.UserName = rd.IsDBNull(idxUserName) ? null : rd.GetString(idxUserName); }
                        catch (IndexOutOfRangeException) { }
                        list.Add(l);
                    }
                }
            }
            return list;
        }
    }
}
