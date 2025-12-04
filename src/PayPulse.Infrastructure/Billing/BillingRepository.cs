using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PayPulse.Core.Billing;
using PayPulse.Infrastructure.Data;

namespace PayPulse.Infrastructure.Billing
{
    public class BillingRepository : IBillingRepository
    {
        private readonly string _dbPath;

        public BillingRepository(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) throw new ArgumentNullException(nameof(dbPath));
            _dbPath = dbPath;
        }

        private SQLiteConnection CreateConnection()
        {
            var connectionString = "Data Source=" + _dbPath + ";Version=3;";
            return new SQLiteConnection(connectionString);
        }

        public BillingTariffs GetTariffs()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT PricePerTransfer, PricePerAddToPos, PricePerCustomer, Currency
FROM BillingConfig
LIMIT 1;
";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new BillingTariffs
                            {
                                PricePerTransfer = Convert.ToDecimal(reader["PricePerTransfer"]),
                                PricePerAddToPos = Convert.ToDecimal(reader["PricePerAddToPos"]),
                                PricePerCustomer = Convert.ToDecimal(reader["PricePerCustomer"]),
                                Currency = reader["Currency"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public void SaveTariffs(BillingTariffs tariffs)
        {
            if (tariffs == null) throw new ArgumentNullException(nameof(tariffs));

            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
DELETE FROM BillingConfig;

INSERT INTO BillingConfig
(Id, PricePerTransfer, PricePerAddToPos, PricePerCustomer, Currency)
VALUES (1, @PricePerTransfer, @PricePerAddToPos, @PricePerCustomer, @Currency);
";
                    cmd.Parameters.AddWithValue("@PricePerTransfer", tariffs.PricePerTransfer);
                    cmd.Parameters.AddWithValue("@PricePerAddToPos", tariffs.PricePerAddToPos);
                    cmd.Parameters.AddWithValue("@PricePerCustomer", tariffs.PricePerCustomer);
                    cmd.Parameters.AddWithValue("@Currency", tariffs.Currency ?? string.Empty);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertBillingEvent(BillingEvent billingEvent)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO BillingEvent (EventType, ProfileId, Provider, TransferId, CreatedAtUtc)
VALUES (@EventType, @ProfileId, @Provider, @TransferId, @CreatedAtUtc);
";

                    cmd.Parameters.AddWithValue("@EventType", billingEvent.EventType);
                    cmd.Parameters.AddWithValue("@ProfileId", billingEvent.ProfileId ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Provider", billingEvent.Provider ?? string.Empty);
                    cmd.Parameters.AddWithValue("@TransferId", billingEvent.TransferId ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CreatedAtUtc", billingEvent.CreatedAtUtc.ToString("o"));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public BillingPeriodSummary GetPeriodSummary(string periodKey, BillingTariffs tariffs)
        {
            if (tariffs == null) throw new ArgumentNullException(nameof(tariffs));

            var summary = new BillingPeriodSummary
            {
                PeriodKey = periodKey,
                Currency = tariffs.Currency,
                PricePerTransfer = tariffs.PricePerTransfer,
                PricePerAddToPos = tariffs.PricePerAddToPos,
                PricePerCustomer = tariffs.PricePerCustomer
            };

            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT EventType, COUNT(*) AS Cnt
FROM BillingEvent
WHERE substr(CreatedAtUtc, 1, 7) = @PeriodKey
GROUP BY EventType;
";

                    cmd.Parameters.AddWithValue("@PeriodKey", periodKey);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var type = reader["EventType"].ToString();
                            var count = Convert.ToInt32(reader["Cnt"]);

                            switch (type)
                            {
                                case "Transfer":
                                    summary.TransferCount = count;
                                    break;
                                case "AddToPos":
                                    summary.AddToPosCount = count;
                                    break;
                                case "Customer":
                                    summary.CustomerCount = count;
                                    break;
                            }
                        }
                    }
                }
            }

            return summary;
        }

        public BillingPeriod InsertClosedPeriod(BillingPeriod period)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO BillingPeriod
(PeriodKey, FromDateUtc, ToDateUtc, TotalTransfers, TotalAddToPos, TotalCustomers, Amount, Currency, IsClosed, CreatedAtUtc)
VALUES
(@PeriodKey, @FromDateUtc, @ToDateUtc, @TotalTransfers, @TotalAddToPos, @TotalCustomers, @Amount, @Currency, @IsClosed, @CreatedAtUtc);

SELECT last_insert_rowid();
";

                    cmd.Parameters.AddWithValue("@PeriodKey", period.PeriodKey);
                    cmd.Parameters.AddWithValue("@FromDateUtc", period.FromDateUtc.ToString("o"));
                    cmd.Parameters.AddWithValue("@ToDateUtc", period.ToDateUtc.ToString("o"));
                    cmd.Parameters.AddWithValue("@TotalTransfers", period.TotalTransfers);
                    cmd.Parameters.AddWithValue("@TotalAddToPos", period.TotalAddToPos);
                    cmd.Parameters.AddWithValue("@TotalCustomers", period.TotalCustomers);
                    cmd.Parameters.AddWithValue("@Amount", period.Amount);
                    cmd.Parameters.AddWithValue("@Currency", period.Currency ?? string.Empty);
                    cmd.Parameters.AddWithValue("@IsClosed", period.IsClosed ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CreatedAtUtc", period.CreatedAtUtc.ToString("o"));

                    var id = cmd.ExecuteScalar();
                    period.Id = Convert.ToInt32(id);
                }
            }

            return period;
        }

        public bool PeriodExists(string periodKey)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT COUNT(1)
FROM BillingPeriod
WHERE PeriodKey = @PeriodKey;
";

                    cmd.Parameters.AddWithValue("@PeriodKey", periodKey);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public IList<BillingEvent> GetEvents(DateTime fromUtc, DateTime toUtc, string eventType, string profileId, string provider)
        {
            var list = new List<BillingEvent>();

            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    var sql = @"
SELECT Id, EventType, ProfileId, Provider, TransferId, CreatedAtUtc
FROM BillingEvent
WHERE CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
";

                    if (!string.IsNullOrEmpty(eventType))
                        sql += " AND EventType = @EventType";

                    if (!string.IsNullOrEmpty(profileId))
                        sql += " AND ProfileId = @ProfileId";

                    if (!string.IsNullOrEmpty(provider))
                        sql += " AND Provider = @Provider";

                    sql += " ORDER BY CreatedAtUtc DESC";

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@FromUtc", fromUtc.ToString("o"));
                    cmd.Parameters.AddWithValue("@ToUtc", toUtc.ToString("o"));

                    if (!string.IsNullOrEmpty(eventType))
                        cmd.Parameters.AddWithValue("@EventType", eventType);

                    if (!string.IsNullOrEmpty(profileId))
                        cmd.Parameters.AddWithValue("@ProfileId", profileId);

                    if (!string.IsNullOrEmpty(provider))
                        cmd.Parameters.AddWithValue("@Provider", provider);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var evt = new BillingEvent
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EventType = reader["EventType"].ToString(),
                                ProfileId = reader["ProfileId"].ToString(),
                                Provider = reader["Provider"].ToString(),
                                TransferId = reader["TransferId"].ToString(),
                                CreatedAtUtc = DateTime.Parse(reader["CreatedAtUtc"].ToString())
                            };
                            list.Add(evt);
                        }
                    }
                }
            }

            return list;
        }

        public IList<BillingPeriod> GetClosedPeriods()
        {
            var list = new List<BillingPeriod>();

            using (var connection = CreateConnection())
            {
                connection.Open();
                BillingSqliteSchemaExtensions.CreateBillingTables(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT Id, PeriodKey, FromDateUtc, ToDateUtc, TotalTransfers, TotalAddToPos, TotalCustomers,
       Amount, Currency, IsClosed, CreatedAtUtc
FROM BillingPeriod
ORDER BY PeriodKey DESC;
";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var period = new BillingPeriod
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                PeriodKey = reader["PeriodKey"].ToString(),
                                FromDateUtc = DateTime.Parse(reader["FromDateUtc"].ToString()),
                                ToDateUtc = DateTime.Parse(reader["ToDateUtc"].ToString()),
                                TotalTransfers = Convert.ToInt32(reader["TotalTransfers"]),
                                TotalAddToPos = Convert.ToInt32(reader["TotalAddToPos"]),
                                TotalCustomers = Convert.ToInt32(reader["TotalCustomers"]),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                Currency = reader["Currency"].ToString(),
                                IsClosed = Convert.ToInt32(reader["IsClosed"]) == 1,
                                CreatedAtUtc = DateTime.Parse(reader["CreatedAtUtc"].ToString())
                            };
                            list.Add(period);
                        }
                    }
                }
            }

            return list;
        }
    }
}