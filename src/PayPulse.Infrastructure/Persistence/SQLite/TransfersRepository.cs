using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Infrastructure.Persistence.SQLite
{
    public class TransfersRepository : ITransfersRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public TransfersRepository(SqliteConnectionFactory factory)
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
                CREATE TABLE IF NOT EXISTS Transfers (
                     TransactionId TEXT PRIMARY KEY,
                     ReservationCode TEXT,
                     Date TEXT,
                     SenderName TEXT,
                     TellerName TEXT,
                     AgentName TEXT,
                     SentAmount REAL,
                     FeeAmount REAL,
                     ExtraAmount REAL,
                     PosAmount REAL,
                     Currency TEXT,
                     AgentId TEXT,
                     SenderIdType TEXT,
                     SenderIdNumber TEXT,
                     SenderPhoneNumber TEXT,
                     Type TEXT,
                     Status TEXT,
                     SenderNationality TEXT,
                     PosCustomerTotalAmount REAL,
                     PosCustomerId TEXT,
                     IsNewCustomer INTEGER,
                     IsAddedToPos INTEGER,
                     ErrorMessage TEXT,
                     CreatedAtUtc TEXT,
                     UpdatedAtUtc TEXT
                );";
                cmd.ExecuteNonQuery();
            }
        }

        public void UpsertTransfer(Transfer t)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Transfers (
                     TransactionId, ReservationCode, Date, SenderName, TellerName, AgentName,
                     SentAmount, FeeAmount, ExtraAmount, PosAmount, Currency, AgentId,
                     SenderIdType, SenderIdNumber, SenderPhoneNumber, Type, Status,
                     SenderNationality, PosCustomerTotalAmount, PosCustomerId,
                     IsNewCustomer, IsAddedToPos, ErrorMessage, CreatedAtUtc, UpdatedAtUtc
                    ) VALUES (
                     @TransactionId, @ReservationCode, @Date, @SenderName, @TellerName, @AgentName,
                     @SentAmount, @FeeAmount, @ExtraAmount, @PosAmount, @Currency, @AgentId,
                     @SenderIdType, @SenderIdNumber, @SenderPhoneNumber, @Type, @Status,
                     @SenderNationality, @PosCustomerTotalAmount, @PosCustomerId,
                     @IsNewCustomer, @IsAddedToPos, @ErrorMessage, @CreatedAtUtc, @UpdatedAtUtc
                    )
                    ON CONFLICT(TransactionId) DO UPDATE SET
                     ReservationCode=@ReservationCode,
                     Date=@Date,
                     SenderName=@SenderName,
                     TellerName=@TellerName,
                     AgentName=@AgentName,
                     SentAmount=@SentAmount,
                     FeeAmount=@FeeAmount,
                     ExtraAmount=@ExtraAmount,
                     PosAmount=@PosAmount,
                     Currency=@Currency,
                     AgentId=@AgentId,
                     SenderIdType=@SenderIdType,
                     SenderIdNumber=@SenderIdNumber,
                     SenderPhoneNumber=@SenderPhoneNumber,
                     Type=@Type,
                     Status=@Status,
                     SenderNationality=@SenderNationality,
                     PosCustomerTotalAmount=@PosCustomerTotalAmount,
                     PosCustomerId=@PosCustomerId,
                     IsNewCustomer=@IsNewCustomer,
                     IsAddedToPos=@IsAddedToPos,
                     ErrorMessage=@ErrorMessage,
                     UpdatedAtUtc=@UpdatedAtUtc;
                    ";
                cmd.Parameters.AddWithValue("@TransactionId", t.TransactionId);
                cmd.Parameters.AddWithValue("@ReservationCode", (object)t.ReservationCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", t.Date.ToUniversalTime().ToString("o"));
                cmd.Parameters.AddWithValue("@SenderName", (object)t.SenderName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TellerName", (object)t.TellerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AgentName", (object)t.AgentName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SentAmount", t.SentAmount);
                cmd.Parameters.AddWithValue("@FeeAmount", t.FeeAmount);
                cmd.Parameters.AddWithValue("@ExtraAmount", t.ExtraAmount);
                cmd.Parameters.AddWithValue("@PosAmount", t.PosAmount);
                cmd.Parameters.AddWithValue("@Currency", (object)t.Currency ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AgentId", (object)t.AgentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SenderIdType", (object)t.SenderIdType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SenderIdNumber", (object)t.SenderIdNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SenderPhoneNumber", (object)t.SenderPhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Type", (object)t.Type ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)t.Status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SenderNationality", (object)t.SenderNationality ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PosCustomerTotalAmount", t.PosCustomerTotalAmount);
                cmd.Parameters.AddWithValue("@PosCustomerId", (object)t.PosCustomerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsNewCustomer", t.IsNewCustomer ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsAddedToPos", t.IsAddedToPos ? 1 : 0);
                cmd.Parameters.AddWithValue("@ErrorMessage", (object)t.ErrorMessage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAtUtc", t.CreatedAtUtc.ToUniversalTime().ToString("o"));
                cmd.Parameters.AddWithValue("@UpdatedAtUtc", t.UpdatedAtUtc.HasValue ? (object)t.UpdatedAtUtc.Value.ToUniversalTime().ToString("o") : DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public bool TransferExists(string transactionId)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(1)
                    FROM Transfers
                    WHERE TransactionId = @TransactionId;
                ";
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                var countObj = cmd.ExecuteScalar();
                var count = Convert.ToInt32(countObj);
                return count > 0;
            }
        }


        public IList<Transfer> GetTransfers(DateRange range, string agentId = null)
        {
            var list = new List<Transfer>();

            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT * FROM Transfers
                    WHERE Date >= @Start AND Date <= @End
                    " + (string.IsNullOrWhiteSpace(agentId) ? "" : " AND AgentId=@AgentId") + @"
                    ORDER BY Date DESC;";
                cmd.Parameters.AddWithValue("@Start", range.Start.ToUniversalTime().ToString("o"));
                cmd.Parameters.AddWithValue("@End", range.End.ToUniversalTime().ToString("o"));
                if (!string.IsNullOrWhiteSpace(agentId))
                {
                    cmd.Parameters.AddWithValue("@AgentId", agentId);
                }

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var t = new Transfer();
                        t.TransactionId = rd["TransactionId"].ToString();
                        t.ReservationCode = rd["ReservationCode"] as string;
                        t.Date = DateTime.Parse(rd["Date"].ToString()).ToLocalTime();
                        t.SenderName = rd["SenderName"] as string;
                        t.TellerName = rd["TellerName"] as string;
                        t.AgentName = rd["AgentName"] as string;
                        t.SentAmount = Convert.ToDecimal(rd["SentAmount"]);
                        t.FeeAmount = Convert.ToDecimal(rd["FeeAmount"]);
                        t.ExtraAmount = Convert.ToDecimal(rd["ExtraAmount"]);
                        t.PosAmount = Convert.ToDecimal(rd["PosAmount"]);
                        t.Currency = rd["Currency"] as string;
                        t.AgentId = rd["AgentId"] as string;
                        t.SenderIdType = rd["SenderIdType"] as string;
                        t.SenderIdNumber = rd["SenderIdNumber"] as string;
                        t.SenderPhoneNumber = rd["SenderPhoneNumber"] as string;
                        t.Type = rd["Type"] as string;
                        t.Status = rd["Status"] as string;
                        t.SenderNationality = rd["SenderNationality"] as string;
                        t.PosCustomerTotalAmount = Convert.ToDecimal(rd["PosCustomerTotalAmount"]);
                        t.PosCustomerId = rd["PosCustomerId"] as string;
                        t.IsNewCustomer = Convert.ToInt32(rd["IsNewCustomer"]) != 0;
                        t.IsAddedToPos = Convert.ToInt32(rd["IsAddedToPos"]) != 0;
                        t.ErrorMessage = rd["ErrorMessage"] as string;
                        t.CreatedAtUtc = DateTime.Parse(rd["CreatedAtUtc"].ToString());
                        string upd = rd["UpdatedAtUtc"] as string;
                        t.UpdatedAtUtc = string.IsNullOrWhiteSpace(upd) ? (DateTime?)null : DateTime.Parse(upd);
                        list.Add(t);
                    }
                }
            }

            return list;
        }

        public void UpdateTransferCustomer(string transactionId, string posCustomerId, bool isNewCustomer, string errorMessage)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE Transfers
SET PosCustomerId=@PosCustomerId,
    IsNewCustomer=@IsNewCustomer,
    ErrorMessage=@ErrorMessage,
    UpdatedAtUtc=@UpdatedAtUtc
WHERE TransactionId=@TransactionId;";
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@PosCustomerId", (object)posCustomerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsNewCustomer", isNewCustomer ? 1 : 0);
                cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAtUtc", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkAsAddedToPos(string transactionId)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE Transfers
SET IsAddedToPos=1, UpdatedAtUtc=@UpdatedAtUtc
WHERE TransactionId=@TransactionId;";
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@UpdatedAtUtc", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
        }

        public IList<Transfer> GetPendingForPos()
        {
            var list = new List<Transfer>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Transfers WHERE IsAddedToPos=0;";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var t = new Transfer();
                        t.TransactionId = rd["TransactionId"].ToString();
                        t.PosCustomerId = rd["PosCustomerId"] as string;
                        t.Currency = rd["Currency"] as string;
                        t.SentAmount = Convert.ToDecimal(rd["SentAmount"]);
                        t.FeeAmount = Convert.ToDecimal(rd["FeeAmount"]);
                        t.ExtraAmount = Convert.ToDecimal(rd["ExtraAmount"]);
                        t.PosAmount = Convert.ToDecimal(rd["PosAmount"]);
                        list.Add(t);
                    }
                }
            }
            return list;
        }
    }
}
