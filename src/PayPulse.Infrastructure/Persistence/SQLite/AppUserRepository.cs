using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.Infrastructure.Persistence.SQLite
{
    /// <summary>
    /// SQLite-backed repository for application users (AppUsers table).
    /// Stores BCrypt-hashed passwords and audit timestamps.
    /// </summary>
    public class AppUserRepository : IAppUserRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public AppUserRepository(SqliteConnectionFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            EnsureSchema();
            EnsureSeedAdmin();
        }

        private void EnsureSchema()
        {
            using (var conn = _factory.Create())
            {
                // Base schema
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS AppUsers (
    UserId           TEXT NOT NULL PRIMARY KEY,
    UserName         TEXT NOT NULL UNIQUE,
    DisplayName      TEXT NULL,
    Password         TEXT NOT NULL,
    Role             TEXT NOT NULL,
    IsActive         INTEGER NOT NULL,
    CreatedAt        TEXT NOT NULL,
    UpdatedAt        TEXT NOT NULL,
    LastLoginAt      TEXT NULL,
    CreatedByUserId  TEXT NULL,
    UpdatedByUserId  TEXT NULL,
    MustChangePassword INTEGER NOT NULL
);";
                    cmd.ExecuteNonQuery();
                }

                // Lightweight migration for existing DBs (adds missing columns).
                var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(AppUsers);";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader["name"]?.ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                existingColumns.Add(name);
                            }
                        }
                    }
                }

                string[] requiredNotNullTextColumns = { "CreatedAt", "UpdatedAt" };
                foreach (var col in requiredNotNullTextColumns)
                {
                    if (!existingColumns.Contains(col))
                    {
                        using (var alter = conn.CreateCommand())
                        {
                            alter.CommandText =
                                $"ALTER TABLE AppUsers ADD COLUMN {col} TEXT NOT NULL DEFAULT (datetime('now'));";
                            alter.ExecuteNonQuery();
                        }
                    }
                }

                if (!existingColumns.Contains("LastLoginAt"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE AppUsers ADD COLUMN LastLoginAt TEXT NULL;";
                        alter.ExecuteNonQuery();
                    }
                }

                if (!existingColumns.Contains("CreatedByUserId"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE AppUsers ADD COLUMN CreatedByUserId TEXT NULL;";
                        alter.ExecuteNonQuery();
                    }
                }

                if (!existingColumns.Contains("UpdatedByUserId"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE AppUsers ADD COLUMN UpdatedByUserId TEXT NULL;";
                        alter.ExecuteNonQuery();
                    }
                }

                if (!existingColumns.Contains("MustChangePassword"))
                {
                    using (var alter = conn.CreateCommand())
                    {
                        alter.CommandText = "ALTER TABLE AppUsers ADD COLUMN MustChangePassword INTEGER NOT NULL DEFAULT 0;";
                        alter.ExecuteNonQuery();
                    }
                }
            }
        }

        private void EnsureSeedAdmin()
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM AppUsers WHERE Role = 'Admin';";
                var count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                if (count > 0)
                {
                    return;
                }
            }

            var admin = new AppUser
            {
                UserId = Guid.NewGuid().ToString("D"),
                UserName = "admin",
                DisplayName = "Administrator",
                Password = "admin", // will be hashed in Insert
                Role = AppUserRole.Admin,
                IsActive = true,
                CreatedByUserId = null,
                UpdatedByUserId = null,
                MustChangePassword = true
            };

            Insert(admin);
        }

        public AppUser GetByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT UserId, UserName, DisplayName, Password, Role, IsActive,
       CreatedAt, UpdatedAt, LastLoginAt, CreatedByUserId, UpdatedByUserId, MustChangePassword
FROM AppUsers
WHERE UserName = @userName;";
                cmd.Parameters.AddWithValue("@userName", userName);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return Map(reader);
                }
            }
        }

        public IList<AppUser> GetAll()
        {
            var list = new List<AppUser>();

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT UserId, UserName, DisplayName, Password, Role, IsActive,
       CreatedAt, UpdatedAt, LastLoginAt, CreatedByUserId, UpdatedByUserId, MustChangePassword
FROM AppUsers
ORDER BY UserName;";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Map(reader));
                    }
                }
            }

            return list;
        }

        public void Insert(AppUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var now = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(user.UserId))
            {
                user.UserId = Guid.NewGuid().ToString("D");
            }

            var passwordToStore = NormalizePasswordForStorage(user.Password);

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO AppUsers
    (UserId, UserName, DisplayName, Password, Role, IsActive,
     CreatedAt, UpdatedAt, LastLoginAt, CreatedByUserId, UpdatedByUserId, MustChangePassword)
VALUES
    (@id, @userName, @displayName, @password, @role, @isActive,
     @createdAt, @updatedAt, @lastLoginAt, @createdBy, @updatedBy, @mustChange);";

                cmd.Parameters.AddWithValue("@id", user.UserId);
                cmd.Parameters.AddWithValue("@userName", user.UserName);
                cmd.Parameters.AddWithValue("@displayName", (object)user.DisplayName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@password", passwordToStore);
                cmd.Parameters.AddWithValue("@role", user.Role.ToString());
                cmd.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@createdAt", now.ToString("o"));
                cmd.Parameters.AddWithValue("@updatedAt", now.ToString("o"));
                cmd.Parameters.AddWithValue("@lastLoginAt", (object)user.LastLoginAt?.ToString("o") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@createdBy", (object)user.CreatedByUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@updatedBy", (object)user.UpdatedByUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@mustChange", user.MustChangePassword ? 1 : 0);

                cmd.ExecuteNonQuery();
            }

            user.Password = passwordToStore;
            user.CreatedAt = now;
            user.UpdatedAt = now;
        }

        public void Update(AppUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var now = DateTime.UtcNow;
            var passwordToStore = NormalizePasswordForStorage(user.Password);

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE AppUsers
SET UserName          = @userName,
    DisplayName       = @displayName,
    Password          = @password,
    Role              = @role,
    IsActive          = @isActive,
    UpdatedAt         = @updatedAt,
    UpdatedByUserId   = @updatedBy,
    MustChangePassword = @mustChange
WHERE UserId          = @id;";

                cmd.Parameters.AddWithValue("@id", user.UserId);
                cmd.Parameters.AddWithValue("@userName", user.UserName);
                cmd.Parameters.AddWithValue("@displayName", (object)user.DisplayName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@password", passwordToStore);
                cmd.Parameters.AddWithValue("@role", user.Role.ToString());
                cmd.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@updatedAt", now.ToString("o"));
                cmd.Parameters.AddWithValue("@updatedBy", (object)user.UpdatedByUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@mustChange", user.MustChangePassword ? 1 : 0);

                cmd.ExecuteNonQuery();
            }

            user.Password = passwordToStore;
            user.UpdatedAt = now;
        }

        public void Delete(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM AppUsers WHERE UserId = @id;";
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLastLogin(string userId, DateTime lastLoginUtc)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE AppUsers
SET LastLoginAt = @lastLoginAt,
    UpdatedAt   = @updatedAt
WHERE UserId    = @id;";

                var ts = lastLoginUtc.ToString("o");
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@lastLoginAt", ts);
                cmd.Parameters.AddWithValue("@updatedAt", ts);

                cmd.ExecuteNonQuery();
            }
        }

        public void UpdatePassword(string userId, string newPassword, bool clearMustChangePassword)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException("New password cannot be empty.", nameof(newPassword));
            }

            var now = DateTime.UtcNow;
            var passwordToStore = NormalizePasswordForStorage(newPassword);

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE AppUsers
SET Password          = @password,
    UpdatedAt         = @updatedAt,
    MustChangePassword = @mustChange
WHERE UserId          = @id;";

                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@password", passwordToStore);
                cmd.Parameters.AddWithValue("@updatedAt", now.ToString("o"));
                cmd.Parameters.AddWithValue("@mustChange", clearMustChangePassword ? 0 : 1);

                cmd.ExecuteNonQuery();
            }
        }

        private static AppUser Map(SQLiteDataReader reader)
        {
            // Column order must match the SELECT statements above
            var user = new AppUser
            {
                UserId = reader.GetString(0),
                UserName = reader.GetString(1),
                DisplayName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Password = reader.GetString(3),
                Role = Enum.TryParse<AppUserRole>(reader.GetString(4), out var role) ? role : AppUserRole.User,
                IsActive = reader.GetInt32(5) != 0
            };

            // CreatedAt
            if (!reader.IsDBNull(6))
            {
                if (DateTime.TryParse(reader.GetString(6), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var created))
                {
                    user.CreatedAt = created;
                }
            }

            // UpdatedAt
            if (!reader.IsDBNull(7))
            {
                if (DateTime.TryParse(reader.GetString(7), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var updated))
                {
                    user.UpdatedAt = updated;
                }
            }

            // LastLoginAt
            if (!reader.IsDBNull(8))
            {
                if (DateTime.TryParse(reader.GetString(8), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var lastLogin))
                {
                    user.LastLoginAt = lastLogin;
                }
            }

            // CreatedByUserId
            if (!reader.IsDBNull(9))
            {
                user.CreatedByUserId = reader.GetString(9);
            }

            // UpdatedByUserId
            if (!reader.IsDBNull(10))
            {
                user.UpdatedByUserId = reader.GetString(10);
            }

            // MustChangePassword
            if (!reader.IsDBNull(11))
            {
                user.MustChangePassword = reader.GetInt32(11) != 0;
            }

            return user;
        }

        private static string NormalizePasswordForStorage(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }

            // If it already looks like a BCrypt hash (starts with $2), keep as-is
            if (password.StartsWith("$2"))
            {
                return password;
            }

            // Otherwise hash it
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
