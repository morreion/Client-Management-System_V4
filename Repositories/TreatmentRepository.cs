using System.Collections.Generic;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class TreatmentRepository : IRepository<Treatment>
    {
        public async Task<IEnumerable<Treatment>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT t.*, c.Name as ClientName 
                FROM Treatment t 
                JOIN Client c ON t.ClientID = c.ClientID 
                ORDER BY t.Treatment_Date DESC";
            return await connection.QueryAsync<Treatment>(sql);
        }

        public async Task<Treatment?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT t.*, c.Name as ClientName 
                FROM Treatment t 
                JOIN Client c ON t.ClientID = c.ClientID 
                WHERE t.TreatmentID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Treatment>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Treatment>> GetByClientIdAsync(int clientId)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT t.*, c.Name as ClientName 
                FROM Treatment t 
                JOIN Client c ON t.ClientID = c.ClientID 
                WHERE t.ClientID = @ClientId
                ORDER BY t.Treatment_Date DESC";
            return await connection.QueryAsync<Treatment>(sql, new { ClientId = clientId });
        }

        public async Task<int> AddAsync(Treatment entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                INSERT INTO Treatment (
                    Treatment_Date, Expectations_of_Treatment, Impression, 
                    Presenting_Symptoms, Rx, ClientID
                ) VALUES (
                    @Treatment_Date, @Expectations_of_Treatment, @Impression, 
                    @Presenting_Symptoms, @Rx, @ClientID
                );
                SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(Treatment entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                UPDATE Treatment 
                SET Treatment_Date = @Treatment_Date,
                    Expectations_of_Treatment = @Expectations_of_Treatment,
                    Impression = @Impression,
                    Presenting_Symptoms = @Presenting_Symptoms,
                    Rx = @Rx,
                    ClientID = @ClientID
                WHERE TreatmentID = @TreatmentID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Treatment WHERE TreatmentID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Treatment>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT t.*, c.Name as ClientName 
                FROM Treatment t 
                JOIN Client c ON t.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                   OR t.Presenting_Symptoms LIKE @Search 
                   OR t.Impression LIKE @Search
                ORDER BY t.Treatment_Date DESC";
            return await connection.QueryAsync<Treatment>(sql, new { Search = $"%{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Treatment");
        }
    }
}
