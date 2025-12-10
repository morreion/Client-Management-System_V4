using System.Collections.Generic;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class DietRepository : IRepository<Diet>
    {
        public async Task<IEnumerable<Diet>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT d.*, c.Name as ClientName 
                FROM Diet d 
                JOIN Client c ON d.ClientID = c.ClientID 
                ORDER BY d.Diet_Date DESC";
            return await connection.QueryAsync<Diet>(sql);
        }

        public async Task<Diet?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT d.*, c.Name as ClientName 
                FROM Diet d 
                JOIN Client c ON d.ClientID = c.ClientID 
                WHERE d.DietID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Diet>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(Diet entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                INSERT INTO Diet (Diet_Date, Breakfast, Lunch, Dinner, Snacks, ClientID) 
                VALUES (@Diet_Date, @Breakfast, @Lunch, @Dinner, @Snacks, @ClientID);
                SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(Diet entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"UPDATE Diet 
                        SET Diet_Date = @Diet_Date, 
                            Breakfast = @Breakfast, 
                            Lunch = @Lunch,
                            Dinner = @Dinner,
                            Snacks = @Snacks,
                            ClientID = @ClientID
                        WHERE DietID = @DietID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Diet WHERE DietID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Diet>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT d.*, c.Name as ClientName 
                FROM Diet d 
                JOIN Client c ON d.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                   OR d.Breakfast LIKE @Search 
                   OR d.Lunch LIKE @Search
                   OR d.Dinner LIKE @Search
                ORDER BY d.Diet_Date DESC";
            return await connection.QueryAsync<Diet>(sql, new { Search = $"%{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Diet");
        }
    }
}
