using System.Collections.Generic;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class BodySystemsOverviewRepository : IRepository<BodySystemsOverview>
    {
        public async Task<IEnumerable<BodySystemsOverview>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT b.*, c.Name as ClientName 
                FROM Body_Systems_Overview b 
                JOIN Client c ON b.ClientID = c.ClientID 
                ORDER BY b.Assessment_Date DESC";
            return await connection.QueryAsync<BodySystemsOverview>(sql);
        }

        public async Task<BodySystemsOverview?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT b.*, c.Name as ClientName 
                FROM Body_Systems_Overview b 
                JOIN Client c ON b.ClientID = c.ClientID 
                WHERE b.Body_Systems_OverviewID = @Id";
            return await connection.QueryFirstOrDefaultAsync<BodySystemsOverview>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(BodySystemsOverview entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                INSERT INTO Body_Systems_Overview (
                    Assessment_Date, Immune, Allergy, Sleep, Snore, Smoke_Alc,
                    Exercise, Tongue, Cravings, Beverages, Digestion, Bowels,
                    Urination, Head, ENT, Skin_Hair, Nails, Mind_Emotional,
                    Thyroid, Backache, Joint_Pain, ClientID
                ) VALUES (
                    @Assessment_Date, @Immune, @Allergy, @Sleep, @Snore, @Smoke_Alc,
                    @Exercise, @Tongue, @Cravings, @Beverages, @Digestion, @Bowels,
                    @Urination, @Head, @ENT, @Skin_Hair, @Nails, @Mind_Emotional,
                    @Thyroid, @Backache, @Joint_Pain, @ClientID
                );
                SELECT last_insert_rowid();";
            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(BodySystemsOverview entity)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                UPDATE Body_Systems_Overview 
                SET Assessment_Date = @Assessment_Date,
                    Immune = @Immune, Allergy = @Allergy, Sleep = @Sleep, 
                    Snore = @Snore, Smoke_Alc = @Smoke_Alc, Exercise = @Exercise,
                    Tongue = @Tongue, Cravings = @Cravings, Beverages = @Beverages, 
                    Digestion = @Digestion, Bowels = @Bowels, Urination = @Urination, 
                    Head = @Head, ENT = @ENT, Skin_Hair = @Skin_Hair, Nails = @Nails, 
                    Mind_Emotional = @Mind_Emotional, Thyroid = @Thyroid, 
                    Backache = @Backache, Joint_Pain = @Joint_Pain, ClientID = @ClientID
                WHERE Body_Systems_OverviewID = @Body_Systems_OverviewID";
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Body_Systems_Overview WHERE Body_Systems_OverviewID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<BodySystemsOverview>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT b.*, c.Name as ClientName 
                FROM Body_Systems_Overview b 
                JOIN Client c ON b.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                ORDER BY b.Assessment_Date DESC";
            return await connection.QueryAsync<BodySystemsOverview>(sql, new { Search = $"{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Body_Systems_Overview");
        }
    }
}
