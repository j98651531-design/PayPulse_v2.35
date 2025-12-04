using System.Data.SQLite;

namespace PayPulse.Infrastructure.Data
{
    public interface ISqliteConnectionFactory
    {
        SQLiteConnection CreateConnection();
    }

    public class SqliteConnectionFactory : ISqliteConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string dbPath)
        {
            _connectionString = "Data Source=" + dbPath + ";Version=3;";
        }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}