using System.Collections.Generic;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class AnthropometricsRepository : IRepository<Anthropometrics>
    {
        public async Task<IEnumerable<Anthropometrics>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT a.*, c.Name as ClientName 
                FROM Anthropometrics a 
                JOIN Client c ON a.ClientID = c.ClientID 
                ORDER BY a.Assessment_Date DESC";
            return await connection.QueryAsync<Anthropometrics>(sql);
        }

        public async Task<Anthropometrics?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT a.*, c.Name as ClientName 
                FROM Anthropometrics a 
                JOIN Client c ON a.ClientID = c.ClientID 
                WHERE a.AnthropometricsID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Anthropometrics>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Anthropometrics>> GetByClientIdAsync(int clientId)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT a.*, c.Name as ClientName 
                FROM Anthropometrics a 
                JOIN Client c ON a.ClientID = c.ClientID 
                WHERE a.ClientID = @ClientId
                ORDER BY a.Assessment_Date DESC";
            return await connection.QueryAsync<Anthropometrics>(sql, new { ClientId = clientId });
        }

        public async Task<int> AddAsync(Anthropometrics entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                INSERT INTO Anthropometrics (
                    Assessment_Date, BP, Pulse, SpO2_Percent, PWA, 
                    Temp, Weight, Height, Zinc_Status, NOX_Status, ClientID
                ) VALUES (
                    @Assessment_Date, @BP, @Pulse, @SpO2_Percent, @PWA, 
                    @Temp, @Weight, @Height, @Zinc_Status, @NOX_Status, @ClientID
                );
                SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(Anthropometrics entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                UPDATE Anthropometrics 
                SET Assessment_Date = @Assessment_Date,
                    BP = @BP,
                    Pulse = @Pulse,
                    SpO2_Percent = @SpO2_Percent,
                    PWA = @PWA,
                    Temp = @Temp,
                    Weight = @Weight,
                    Height = @Height,
                    Zinc_Status = @Zinc_Status,
                    NOX_Status = @NOX_Status,
                    ClientID = @ClientID
                WHERE AnthropometricsID = @AnthropometricsID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Anthropometrics WHERE AnthropometricsID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Anthropometrics>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT a.*, c.Name as ClientName 
                FROM Anthropometrics a 
                JOIN Client c ON a.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                ORDER BY a.Assessment_Date DESC";
            return await connection.QueryAsync<Anthropometrics>(sql, new { Search = $"{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Anthropometrics");
        }
    }
}
