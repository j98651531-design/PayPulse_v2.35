using System.Data.SQLite;

namespace PayPulse.Infrastructure.Data
{
    public static class BillingSqliteSchemaExtensions
    {
        public static void CreateBillingTables(SQLiteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS BillingConfig (
    Id                INTEGER PRIMARY KEY,
    PricePerTransfer  REAL      NOT NULL,
    PricePerAddToPos  REAL      NOT NULL,
    PricePerCustomer  REAL      NOT NULL,
    Currency          TEXT      NOT NULL
);

CREATE TABLE IF NOT EXISTS BillingEvent (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    EventType        TEXT    NOT NULL,
    ProfileId        TEXT    NOT NULL,
    Provider         TEXT    NOT NULL,
    TransferId       TEXT,
    CreatedAtUtc     TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS BillingPeriod (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    PeriodKey         TEXT    NOT NULL,
    FromDateUtc       TEXT    NOT NULL,
    ToDateUtc         TEXT    NOT NULL,
    TotalTransfers    INTEGER NOT NULL,
    TotalAddToPos     INTEGER NOT NULL,
    TotalCustomers    INTEGER NOT NULL,
    Amount            REAL    NOT NULL,
    Currency          TEXT    NOT NULL,
    IsClosed          INTEGER NOT NULL,
    CreatedAtUtc      TEXT    NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_BillingPeriod_PeriodKey
ON BillingPeriod(PeriodKey);
";
                cmd.ExecuteNonQuery();
            }
        }
    }
}