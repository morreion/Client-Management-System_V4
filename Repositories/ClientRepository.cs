using System

.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    /// <summary>
    /// Repository for Client entity with CRUD operations and search
    /// </summary>
    public class ClientRepository : IRepository<Client>
    {
        private readonly string _connectionString;

        public ClientRepository()
        {
            _connectionString = DatabaseManager.ConnectionString;
            EnsureGenderColumnExists();
        }

        private void EnsureGenderColumnExists()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            
            var checkSql = "PRAGMA table_info(Client)";
            var columns = connection.Query<dynamic>(checkSql);
            var genderColumn = columns.FirstOrDefault(c => c.name == "Gender");

            if (genderColumn == null)
            {
                // Add column if missing (Nullable by default in this new version)
                var alterSql = "ALTER TABLE Client ADD COLUMN Gender INTEGER";
                connection.Execute(alterSql);
            }
            else
            {
                // If column exists, check if it is NOT NULL (notnull = 1)
                // We want it to be nullable. SQLite ALTER COLUMN is limited, so we use Rename-Add-Copy-Drop pattern
                if (genderColumn.notnull == 1)
                {
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        // 1. Rename old column
                        connection.Execute("ALTER TABLE Client RENAME COLUMN Gender TO Gender_Old", transaction: transaction);

                        // 2. Add new nullable column
                        connection.Execute("ALTER TABLE Client ADD COLUMN Gender INTEGER", transaction: transaction);

                        // 3. Copy data
                        connection.Execute("UPDATE Client SET Gender = Gender_Old", transaction: transaction);

                        // 4. Drop old column
                        connection.Execute("ALTER TABLE Client DROP COLUMN Gender_Old", transaction: transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all clients ordered by name
        /// </summary>
        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "SELECT * FROM Client ORDER BY Name";
            return await connection.QueryAsync<Client>(sql);
        }

        /// <summary>
        /// Gets a client by ID
        /// </summary>
        public async Task<Client?> GetByIdAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "SELECT * FROM Client WHERE ClientID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { Id = id });
        }

        /// <summary>
        /// Adds a new client and returns the new ClientID
        /// </summary>
        public async Task<int> AddAsync(Client client)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Client (
                    Name, Address, DOB, Mobile, Email, Occupation, 
                    Date_First_Consultation, Date_Last_Consultation, 
                    Marital_Status, Children, Ref, Alt_Contact, Gender
                )
                VALUES (
                    @Name, @Address, @DOB, @Mobile, @Email, @Occupation,
                    @Date_First_Consultation, @Date_Last_Consultation,
                    @Marital_Status, @Children, @Ref, @Alt_Contact, @Gender
                );
                SELECT last_insert_rowid();";
            
            return await connection.ExecuteScalarAsync<int>(sql, client);
        }

        /// <summary>
        /// Updates an existing client
        /// </summary>
        public async Task<bool> UpdateAsync(Client client)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                UPDATE Client 
                SET 
                    Name = @Name, 
                    Address = @Address, 
                    DOB = @DOB, 
                    Mobile = @Mobile, 
                    Email = @Email, 
                    Occupation = @Occupation,
                    Date_First_Consultation = @Date_First_Consultation,
                    Date_Last_Consultation = @Date_Last_Consultation,
                    Marital_Status = @Marital_Status, 
                    Children = @Children,
                    Ref = @Ref, 
                    Alt_Contact = @Alt_Contact,
                    Gender = @Gender
                WHERE ClientID = @ClientID";
            
            int rowsAffected = await connection.ExecuteAsync(sql, client);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Deletes a client by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "DELETE FROM Client WHERE ClientID = @Id";
            int rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Searches clients by name or email
        /// </summary>
        public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Client 
                WHERE Name LIKE @Search OR Email LIKE @Search
                ORDER BY Name";
            
            return await connection.QueryAsync<Client>(sql, new { Search = $"%{searchTerm}%" });
        }

        /// <summary>
        /// Gets total client count
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            const string sql = "SELECT COUNT(*) FROM Client";
            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }
}
