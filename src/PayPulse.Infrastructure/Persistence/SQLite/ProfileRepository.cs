using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.Infrastructure.Persistence.SQLite
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ProfileRepository(SqliteConnectionFactory factory)
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
CREATE TABLE IF NOT EXISTS Profiles (
 ProfileId TEXT PRIMARY KEY,
 Name TEXT,
 ProviderType TEXT,
 ManualToken TEXT,
 IsActive INTEGER,
 LastSyncUtc TEXT,
 PosUserId TEXT,
 PosCashboxId TEXT,
 LoginEmail TEXT,
 LoginPhoneNumber TEXT,
 LoginPassword TEXT,
 TotpSecret TEXT
);";
                cmd.ExecuteNonQuery();
            }
        }

        public IList<Profile> GetAll()
        {
            var list = new List<Profile>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Profiles ORDER BY Name";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(Read(rd));
                    }
                }
            }
            return list;
        }

        public Profile GetById(string profileId)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Profiles WHERE ProfileId=@ProfileId";
                cmd.Parameters.AddWithValue("@ProfileId", profileId);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return Read(rd);
                }
            }
        }

        private Profile Read(SQLiteDataReader rd)
        {
            var p = new Profile();
            p.ProfileId = rd["ProfileId"].ToString();
            p.Name = rd["Name"] as string;
            p.ProviderType = rd["ProviderType"] as string;
            p.ManualToken = rd["ManualToken"] as string;
            p.IsActive = Convert.ToInt32(rd["IsActive"]) != 0;
            string lastSync = rd["LastSyncUtc"] as string;
            p.LastSyncUtc = string.IsNullOrWhiteSpace(lastSync) ? (DateTime?)null : DateTime.Parse(lastSync);
            p.PosUserId = rd["PosUserId"] as string;
            p.PosCashboxId = rd["PosCashboxId"] as string;
            p.LoginEmail = rd["LoginEmail"] as string;
            p.LoginPhoneNumber = rd["LoginPhoneNumber"] as string;
            p.LoginPassword = rd["LoginPassword"] as string;
            p.TotpSecret = rd["TotpSecret"] as string;
            return p;
        }

        public void Insert(Profile profile)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Profiles (ProfileId, Name, ProviderType, ManualToken, IsActive, LastSyncUtc,
 PosUserId, PosCashboxId, LoginEmail, LoginPhoneNumber, LoginPassword, TotpSecret)
VALUES (@ProfileId, @Name, @ProviderType, @ManualToken, @IsActive, @LastSyncUtc,
 @PosUserId, @PosCashboxId, @LoginEmail, @LoginPhoneNumber, @LoginPassword, @TotpSecret);";
                FillParams(cmd, profile);
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Profile profile)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE Profiles SET
 Name=@Name,
 ProviderType=@ProviderType,
 ManualToken=@ManualToken,
 IsActive=@IsActive,
 LastSyncUtc=@LastSyncUtc,
 PosUserId=@PosUserId,
 PosCashboxId=@PosCashboxId,
 LoginEmail=@LoginEmail,
 LoginPhoneNumber=@LoginPhoneNumber,
 LoginPassword=@LoginPassword,
 TotpSecret=@TotpSecret
WHERE ProfileId=@ProfileId;";
                FillParams(cmd, profile);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(string profileId)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Profiles WHERE ProfileId=@ProfileId";
                cmd.Parameters.AddWithValue("@ProfileId", profileId);
                cmd.ExecuteNonQuery();
            }
        }

        private void FillParams(SQLiteCommand cmd, Profile p)
        {
            cmd.Parameters.AddWithValue("@ProfileId", p.ProfileId);
            cmd.Parameters.AddWithValue("@Name", (object)p.Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProviderType", (object)p.ProviderType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ManualToken", (object)p.ManualToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", p.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@LastSyncUtc", p.LastSyncUtc.HasValue ? (object)p.LastSyncUtc.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("@PosUserId", (object)p.PosUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PosCashboxId", (object)p.PosCashboxId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginEmail", (object)p.LoginEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginPhoneNumber", (object)p.LoginPhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginPassword", (object)p.LoginPassword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TotpSecret", (object)p.TotpSecret ?? DBNull.Value);
        }
    }
}
