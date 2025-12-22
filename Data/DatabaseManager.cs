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
                    // Use LocalApplicationData (AppData/Local) to ensure we have write permissions in Program Files
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "ClientManagementSystemV4");
                    
                    // Ensure the directory exists
                    if (!Directory.Exists(appDataPath))
                    {
                        Directory.CreateDirectory(appDataPath);
                    }

                    var dbPath = Path.Combine(appDataPath, DatabaseFileName);
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
                // Default to schema file in bin directory (where the .exe is)
                if (string.IsNullOrEmpty(sqlScriptPath))
                {
                    sqlScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_mgmt_schema.sql");
                    
                    if (!File.Exists(sqlScriptPath))
                    {
                        throw new FileNotFoundException($"SQL schema file not found at: {sqlScriptPath}");
                    }
                }

                // Get database path from ConnectionString (already points to AppData)
                var builder = new SQLiteConnectionStringBuilder(ConnectionString);
                var dbPath = builder.DataSource;

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
                        tablesExist = false;
                    }
                }

                // If tables already exist, check for migrations
                if (tablesExist)
                {
                    PerformMigrations();
                    return;
                }

                // Execute the schema directly
                using var connection = new SQLiteConnection(ConnectionString);
                connection.Open();

                // Read entire SQL file
                var schema = File.ReadAllText(sqlScriptPath);
                
                // Execute the entire schema as one batch
                using var command = connection.CreateCommand();
                command.CommandText = schema;
                command.ExecuteNonQuery();

                PerformMigrations();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks for and applies necessary database migrations
        /// </summary>
        private static void PerformMigrations()
        {
            try
            {
                using var connection = new SQLiteConnection(ConnectionString);
                connection.Open();

                // Check if Gender column exists in Client table
                bool genderExists = false;
                using (var checkCmd = new SQLiteCommand("PRAGMA table_info(Client)", connection))
                using (var reader = checkCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader["name"].ToString();
                        if (string.Equals(name, "Gender", StringComparison.OrdinalIgnoreCase))
                        {
                            genderExists = true;
                            break;
                        }
                    }
                }

                if (!genderExists)
                {
                    using var alterCmd = new SQLiteCommand("ALTER TABLE Client ADD COLUMN Gender INTEGER", connection);
                    alterCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log or handle migration error
                Debug.WriteLine($"Migration failed: {ex.Message}");
                // We don't throw here to avoid crashing the app if migration fails, 
                // but functionally it might still fail later if column is missing.
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
