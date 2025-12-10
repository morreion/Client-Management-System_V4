using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    /// <summary>
    /// Repository for Distributor data access
    /// </summary>
    public class DistributorRepository : IRepository<Distributor>
    {
        public async Task<IEnumerable<Distributor>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.QueryAsync<Distributor>(
                "SELECT * FROM Distributor ORDER BY Name");
        }

        public async Task<Distributor?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Distributor>(
                "SELECT * FROM Distributor WHERE DistributorID = @Id",
                new { Id = id });
        }

        public async Task<int> AddAsync(Distributor entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"INSERT INTO Distributor (Name, Address, Work_Phone, Mobile, Email, Website) 
                        VALUES (@Name, @Address, @Work_Phone, @Mobile, @Email, @Website);
                        SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(Distributor entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"UPDATE Distributor 
                        SET Name = @Name, 
                            Address = @Address, 
                            Work_Phone = @Work_Phone,
                            Mobile = @Mobile,
                            Email = @Email,
                            Website = @Website 
                        WHERE DistributorID = @DistributorID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Distributor WHERE DistributorID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Search distributors by name, email, or phone
        /// </summary>
        public async Task<IEnumerable<Distributor>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.QueryAsync<Distributor>(
                @"SELECT * FROM Distributor 
                  WHERE Name LIKE @Search 
                     OR Email LIKE @Search 
                     OR Work_Phone LIKE @Search
                     OR Mobile LIKE @Search
                     OR Website LIKE @Search
                  ORDER BY Name",
                new { Search = $"%{searchTerm}%" });
        }

        /// <summary>
        /// Get count of distributors
        /// </summary>
        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Distributor");
        }
    }
}
