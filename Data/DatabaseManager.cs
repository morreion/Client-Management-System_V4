using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Client_Management_System_V4.Data
{
    /// <summary>
    /// Handles database initialization and connection management
    /// </summary>
    public static class DatabaseManager
    {
        private static string? _connectionString;
        private const string DatabaseFileName = "HealthManagement.db";

        /// <summary>
        /// Gets the connection string for the database
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", DatabaseFileName);
                    _connectionString = $"Data Source={dbPath};Version=3;";
                }
                return _connectionString;
            }
        }

        /// <summary>
        /// SYNCHRONOUS database initialization using sqlite3.exe command line
        /// </summary>
        public static void InitializeDatabase(string? sqlScriptPath = null)
        {
            try
            {
                // Default to schema file in bin directory
                if (string.IsNullOrEmpty(sqlScriptPath))
                {
                    sqlScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_mgmt_schema.sql");
                    
                    if (!File.Exists(sqlScriptPath))
                    {
                        throw new FileNotFoundException($"SQL schema file not found at: {sqlScriptPath}");
                    }
                }

                // Create Data directory
                var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                Directory.CreateDirectory(dataDir);
                
                var dbPath = Path.Combine(dataDir, DatabaseFileName);

                // Check if tables already exist
                bool tablesExist = false;
                if (File.Exists(dbPath))
                {
                    try
                    {
                        using var testConn = new SQLiteConnection(ConnectionString);
                        testConn.Open();
                        using var checkCmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Client'", testConn);
                        var result = checkCmd.ExecuteScalar();
                        tablesExist = result != null;
                    }
                    catch
                    {
                        // If we can't check, assume we need to initialize
                        tablesExist = false;
                    }
                }

                // If tables already exist, nothing to do
                if (tablesExist)
                {
                    return;
                }

                // Use System.Data.SQLite to execute the schema directly
                // This avoids issues with parsing and execution order
                using var connection = new SQLiteConnection(ConnectionString);
                connection.Open();

                // Read entire SQL file
                var schema = File.ReadAllText(sqlScriptPath);
                
                // Execute the entire schema as one batch
                using var command = connection.CreateCommand();
                command.CommandText = schema;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a new database connection
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                return connection.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
