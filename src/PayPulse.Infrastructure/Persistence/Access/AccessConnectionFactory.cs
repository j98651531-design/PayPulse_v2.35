using System.Data.OleDb;
using System.IO;

namespace PayPulse.Infrastructure.Persistence.Access
{
    public class AccessConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _posDbPath;

        public AccessConnectionFactory(string posDbPath)
        {
            _posDbPath = posDbPath;
            _connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + posDbPath + ";Persist Security Info=False;";
        }

        public OleDbConnection Create()
        {
            if (!File.Exists(_posDbPath))
            {
                throw new FileNotFoundException("POS database file not found: " + _posDbPath);
            }

            var conn = new OleDbConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
