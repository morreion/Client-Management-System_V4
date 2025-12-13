using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class MedHxRepository : IRepository<MedHx>
    {
        // 1. Master List
        public async Task<IEnumerable<MedHx>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            // Map Med_Hx column to HistoryNotes property alias
            var sql = @"
                SELECT 
                    m.Med_HxID, m.ClientID, m.Assessment_Date, 
                    m.Blood_Test_Results, m.Medication, m.Supplements, 
                    m.Accidents_Previous_Illness, m.Menstrual_Notes, 
                    m.Vaccinations, m.Med_Hx AS HistoryNotes, m.Family_Med_Hx,
                    c.Name as ClientName 
                FROM Med_Hx m 
                JOIN Client c ON m.ClientID = c.ClientID 
                ORDER BY m.Assessment_Date DESC";
            return await connection.QueryAsync<MedHx>(sql);
        }

        public async Task<MedHx?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT 
                    m.Med_HxID, m.ClientID, m.Assessment_Date, 
                    m.Blood_Test_Results, m.Medication, m.Supplements, 
                    m.Accidents_Previous_Illness, m.Menstrual_Notes, 
                    m.Vaccinations, m.Med_Hx AS HistoryNotes, m.Family_Med_Hx,
                    c.Name as ClientName 
                FROM Med_Hx m 
                JOIN Client c ON m.ClientID = c.ClientID 
                WHERE m.Med_HxID = @Id";
            return await connection.QueryFirstOrDefaultAsync<MedHx>(sql, new { Id = id });
        }

        public async Task<IEnumerable<MedHx>> GetByClientIdAsync(int clientId)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT 
                    m.Med_HxID, m.ClientID, m.Assessment_Date, 
                    m.Blood_Test_Results, m.Medication, m.Supplements, 
                    m.Accidents_Previous_Illness, m.Menstrual_Notes, 
                    m.Vaccinations, m.Med_Hx AS HistoryNotes, m.Family_Med_Hx,
                    c.Name as ClientName 
                FROM Med_Hx m 
                JOIN Client c ON m.ClientID = c.ClientID 
                WHERE m.ClientID = @ClientId
                ORDER BY m.Assessment_Date DESC";
            return await connection.QueryAsync<MedHx>(sql, new { ClientId = clientId });
        }

        // 2. Details Fetcher
        public async Task<IEnumerable<MedHxSupplement>> GetSupplementsByHxIdAsync(int medHxId)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT ms.*, s.Name as SupplementName 
                FROM Med_Hx_Supplements ms
                JOIN Supplements s ON ms.SupplementID = s.SupplementID
                WHERE ms.Med_HxID = @Id";
            return await connection.QueryAsync<MedHxSupplement>(sql, new { Id = medHxId });
        }

        // 3. Transactional Insert
        public async Task<int> AddWithSupplementsAsync(MedHx medHx, IEnumerable<MedHxSupplement> supplements)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // A. Insert Master
                // Note: @HistoryNotes maps to Med_Hx column
                var sqlMaster = @"
                    INSERT INTO Med_Hx (
                        ClientID, Assessment_Date, Blood_Test_Results, Medication, Supplements,
                        Accidents_Previous_Illness, Menstrual_Notes, Vaccinations, Med_Hx, Family_Med_Hx
                    ) VALUES (
                        @ClientID, @Assessment_Date, @Blood_Test_Results, @Medication, @Supplements,
                        @Accidents_Previous_Illness, @Menstrual_Notes, @Vaccinations, @HistoryNotes, @Family_Med_Hx
                    );
                    SELECT last_insert_rowid();";
                
                var id = await connection.ExecuteScalarAsync<int>(sqlMaster, medHx, transaction);

                // B. Insert Details
                var sqlDetail = @"
                    INSERT INTO Med_Hx_Supplements (Med_HxID, SupplementID, Dosage, Frequency, Notes)
                    VALUES (@Med_HxID, @SupplementID, @Dosage, @Frequency, @Notes)";

                foreach (var sup in supplements)
                {
                    sup.Med_HxID = id;
                    await connection.ExecuteAsync(sqlDetail, sup, transaction);
                }

                transaction.Commit();
                return id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 4. Transactional Update
        public async Task UpdateWithSupplementsAsync(MedHx medHx, IEnumerable<MedHxSupplement> supplements)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // A. Update Master
                var sqlMaster = @"
                    UPDATE Med_Hx SET
                        ClientID = @ClientID,
                        Assessment_Date = @Assessment_Date,
                        Blood_Test_Results = @Blood_Test_Results,
                        Medication = @Medication,
                        Supplements = @Supplements,
                        Accidents_Previous_Illness = @Accidents_Previous_Illness,
                        Menstrual_Notes = @Menstrual_Notes,
                        Vaccinations = @Vaccinations,
                        Med_Hx = @HistoryNotes,
                        Family_Med_Hx = @Family_Med_Hx
                    WHERE Med_HxID = @Med_HxID";
                
                await connection.ExecuteAsync(sqlMaster, medHx, transaction);

                // B. Replace Details (Delete All -> Insert New)
                // This is simpler than syncing logic for this use case
                await connection.ExecuteAsync("DELETE FROM Med_Hx_Supplements WHERE Med_HxID = @Id", new { Id = medHx.Med_HxID }, transaction);

                var sqlDetail = @"
                    INSERT INTO Med_Hx_Supplements (Med_HxID, SupplementID, Dosage, Frequency, Notes)
                    VALUES (@Med_HxID, @SupplementID, @Dosage, @Frequency, @Notes)";

                foreach (var sup in supplements)
                {
                    sup.Med_HxID = medHx.Med_HxID!.Value;
                    await connection.ExecuteAsync(sqlDetail, sup, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            // Cascade delete handles the supplements
            var rows = await connection.ExecuteAsync("DELETE FROM Med_Hx WHERE Med_HxID = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<IEnumerable<MedHx>> SearchAsync(string searchTerm)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT 
                    m.Med_HxID, m.ClientID, m.Assessment_Date, 
                    m.Blood_Test_Results, m.Medication, m.Supplements, 
                    m.Accidents_Previous_Illness, m.Menstrual_Notes, 
                    m.Vaccinations, m.Med_Hx AS HistoryNotes, m.Family_Med_Hx,
                    c.Name as ClientName 
                FROM Med_Hx m 
                JOIN Client c ON m.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                   OR m.Medication LIKE @Search
                   OR m.Accidents_Previous_Illness LIKE @Search
                ORDER BY m.Assessment_Date DESC";
            return await connection.QueryAsync<MedHx>(sql, new { Search = $"%{searchTerm}%" });
        }

        // Interface requirements not fully used but needed
        public async Task<int> AddAsync(MedHx entity) => await AddWithSupplementsAsync(entity, Enumerable.Empty<MedHxSupplement>());
        public async Task<bool> UpdateAsync(MedHx entity) 
        {
             // We generally won't call this directly for UI updates, but for interface compliance:
            using var connection = DatabaseManager.GetConnection();
            return await connection.ExecuteAsync("UPDATE Med_Hx SET Medication=@Medication WHERE Med_HxID=@Id", new {entity.Medication, Id=entity.Med_HxID}) > 0;
        }
        public async Task<int> CountAsync()
        {
             using var connection = DatabaseManager.GetConnection();
             return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Med_Hx");
        }
    }
}
