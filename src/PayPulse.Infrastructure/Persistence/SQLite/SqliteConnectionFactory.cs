using System.Data.SQLite;

namespace PayPulse.Infrastructure.Persistence.SQLite
{
    public class SqliteConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string dbPath)
        {
            _connectionString = "Data Source=" + dbPath + ";Version=3;";
        }

        public SQLiteConnection Create()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
