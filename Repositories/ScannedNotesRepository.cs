using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    /// <summary>
    /// Repository for ScannedNote entity with CRUD operations and search.
    /// Handles binary document data (PDFs and images) stored as BLOBs.
    /// </summary>
    public class ScannedNotesRepository : IRepository<ScannedNote>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of ScannedNotesRepository
        /// </summary>
        public ScannedNotesRepository()
        {
            _connectionString = DatabaseManager.ConnectionString;
        }

        /// <summary>
        /// Gets all scanned notes ordered by document date (newest first)
        /// </summary>
        public async Task<IEnumerable<ScannedNote>> GetAllAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
                       Description, Scanned_Document, ClientID 
                FROM Scanned_Notes 
                ORDER BY Document_Date DESC";
            return await connection.QueryAsync<ScannedNote>(sql);
        }

        /// <summary>
        /// Gets a scanned note by ID
        /// </summary>
        public async Task<ScannedNote?> GetByIdAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
                       Description, Scanned_Document, ClientID 
                FROM Scanned_Notes 
                WHERE ScannedNotesID = @Id";
            return await connection.QueryFirstOrDefaultAsync<ScannedNote>(sql, new { Id = id });
        }

        /// <summary>
        /// Gets all scanned notes for a specific client
        /// </summary>
        /// <param name="clientId">The client's ID</param>
        /// <returns>Collection of scanned notes for the client</returns>
        public async Task<IEnumerable<ScannedNote>> GetByClientIdAsync(int clientId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
                       Description, Scanned_Document, ClientID 
                FROM Scanned_Notes 
                WHERE ClientID = @ClientId 
                ORDER BY Document_Date DESC";
            return await connection.QueryAsync<ScannedNote>(sql, new { ClientId = clientId });
        }

        /// <summary>
        /// Adds a new scanned note and returns the new ScannedNotesID
        /// </summary>
        public async Task<int> AddAsync(ScannedNote note)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Scanned_Notes (
                    Document_Name, Document_Date, Document_Type, 
                    Description, Scanned_Document, ClientID
                )
                VALUES (
                    @Document_Name, @Document_Date, @Document_Type, 
                    @Description, @Scanned_Document, @ClientID
                );
                SELECT last_insert_rowid();";
            
            return await connection.ExecuteScalarAsync<int>(sql, note);
        }

        /// <summary>
        /// Updates an existing scanned note
        /// </summary>
        public async Task<bool> UpdateAsync(ScannedNote note)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                UPDATE Scanned_Notes 
                SET 
                    Document_Name = @Document_Name, 
                    Document_Date = @Document_Date, 
                    Document_Type = @Document_Type,
                    Description = @Description,
                    Scanned_Document = @Scanned_Document,
                    ClientID = @ClientID
                WHERE ScannedNotesID = @ScannedNotesID";
            
            int rowsAffected = await connection.ExecuteAsync(sql, note);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Deletes a scanned note by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "DELETE FROM Scanned_Notes WHERE ScannedNotesID = @Id";
            int rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Searches scanned notes by document name
        /// </summary>
        /// <param name="searchTerm">Search term to match against document name</param>
        /// <returns>Collection of matching scanned notes</returns>
        public async Task<IEnumerable<ScannedNote>> SearchAsync(string searchTerm)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
                       Description, Scanned_Document, ClientID 
                FROM Scanned_Notes 
                WHERE Document_Name LIKE @Search
                ORDER BY Document_Date DESC";
            
            return await connection.QueryAsync<ScannedNote>(sql, new { Search = $"{searchTerm}%" });
        }

        /// <summary>
        /// Searches scanned notes by document name for a specific client
        /// </summary>
        /// <param name="clientId">The client's ID</param>
        /// <param name="searchTerm">Search term to match against document name</param>
        /// <returns>Collection of matching scanned notes for the client</returns>
        public async Task<IEnumerable<ScannedNote>> SearchByClientAsync(int clientId, string searchTerm)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
                       Description, Scanned_Document, ClientID 
                FROM Scanned_Notes 
                WHERE ClientID = @ClientId AND Document_Name LIKE @Search
                ORDER BY Document_Date DESC";
            
            return await connection.QueryAsync<ScannedNote>(sql, 
                new { ClientId = clientId, Search = $"{searchTerm}%" });
        }

        /// <summary>
        /// Gets total count of scanned notes
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "SELECT COUNT(*) FROM Scanned_Notes";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        /// <summary>
        /// Gets count of scanned notes for a specific client
        /// </summary>
        public async Task<int> GetCountByClientAsync(int clientId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "SELECT COUNT(*) FROM Scanned_Notes WHERE ClientID = @ClientId";
            return await connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId });
        }
    }
}
