using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    /// <summary>
    /// Repository for Supplement data access
    /// </summary>
    public class SupplementRepository : IRepository<Supplement>
    {
        public async Task<IEnumerable<Supplement>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT s.*, d.Name as DistributorName 
                FROM Supplements s 
                LEFT JOIN Distributor d ON s.DistributorID = d.DistributorID 
                ORDER BY s.Name";
            return await connection.QueryAsync<Supplement>(sql);
        }

        public async Task<Supplement?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT s.*, d.Name as DistributorName 
                FROM Supplements s 
                LEFT JOIN Distributor d ON s.DistributorID = d.DistributorID 
                WHERE s.SupplementID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Supplement>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(Supplement entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"INSERT INTO Supplements (Name, Type, Description, Usage, DistributorID) 
                        VALUES (@Name, @Type, @Description, @Usage, @DistributorID);
                        SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(Supplement entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"UPDATE Supplements 
                        SET Name = @Name, 
                            Type = @Type, 
                            Description = @Description,
                            Usage = @Usage,
                            DistributorID = @DistributorID
                        WHERE SupplementID = @SupplementID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Supplements WHERE SupplementID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Supplement>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT s.*, d.Name as DistributorName 
                FROM Supplements s 
                LEFT JOIN Distributor d ON s.DistributorID = d.DistributorID 
                WHERE s.Name LIKE @Search 
                ORDER BY s.Name";
            return await connection.QueryAsync<Supplement>(sql, new { Search = $"{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Supplements");
        }
    }
}
