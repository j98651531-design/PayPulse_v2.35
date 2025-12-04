using System.Data.OleDb;
using System.IO;

namespace PayPulse.Infrastructure.Persistence.FoxPro
{
    /// <summary>
    /// Simple Visual FoxPro connection factory.
    /// Uses VFPOLEDB provider and a folder path that contains the .dbf files.
    /// </summary>
    public class FoxProConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _folderPath;

        public FoxProConnectionFactory(string folderPath)
        {
            _folderPath = folderPath;
            // Basic Visual FoxPro OLE DB connection string.
            // The actual folder is provided by AppSettings.Databases.PosFoxProFolder.
            _connectionString = "Provider=VFPOLEDB.1;Data Source=" + folderPath + ";Collating Sequence=machine;";
        }

        public OleDbConnection Create()
        {
            if (!Directory.Exists(_folderPath))
            {
                throw new DirectoryNotFoundException("FoxPro POS folder not found: " + _folderPath);
            }

            var conn = new OleDbConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
