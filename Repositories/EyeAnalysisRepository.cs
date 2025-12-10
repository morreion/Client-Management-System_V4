using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class EyeAnalysisRepository : IRepository<EyeAnalysis>
    {
        // 1. Standard Master CRUD
        public async Task<IEnumerable<EyeAnalysis>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT e.*, c.Name as ClientName 
                FROM Eye_Analysis e 
                JOIN Client c ON e.ClientID = c.ClientID 
                ORDER BY e.Analysis_Date DESC";
            return await connection.QueryAsync<EyeAnalysis>(sql);
        }

        public async Task<EyeAnalysis?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT e.*, c.Name as ClientName 
                FROM Eye_Analysis e 
                JOIN Client c ON e.ClientID = c.ClientID 
                WHERE e.Eye_AnalysisID = @Id";
            return await connection.QueryFirstOrDefaultAsync<EyeAnalysis>(sql, new { Id = id });
        }

        // 2. Lookup Data Methods
        public async Task<IEnumerable<ScleraPriorityType>> GetAllPriorityTypesAsync()
        {
            await EnsureSeedDataAsync();
            using var connection = DatabaseManager.GetConnection();
            return await connection.QueryAsync<ScleraPriorityType>("SELECT * FROM Sclera_Priority_Types ORDER BY Priority_Name");
        }

        private async Task EnsureSeedDataAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Sclera_Priority_Types");
            if (count == 0)
            {
                var priorities = new[]
                {
                    "Adrenal Fatigue",
                    "Bowel Toxicity",
                    "Candida",
                    "Heavy Metals",
                    "Kidney Stress",
                    "Liver Stress",
                    "Lymphatic Congestion",
                    "Nerve Rings (Stress)",
                    "Parasites",
                    "Sinus Congestion",
                    "Skin Issues",
                    "Sugar Sensitivity",
                    "Thyroid Stress",
                    "Venous Congestion"
                };

                foreach (var name in priorities)
                {
                    await connection.ExecuteAsync("INSERT INTO Sclera_Priority_Types (Priority_Name) VALUES (@Name)", new { Name = name });
                }
            }
        }

        public async Task<IEnumerable<int>> GetSelectedPriorityIdsAsync(int eyeAnalysisId)
        {
             using var connection = DatabaseManager.GetConnection();
             var sql = "SELECT ScleraPriorityTypeID FROM Eye_Analysis_Sclera_Priorities WHERE Eye_AnalysisID = @Id";
             return await connection.QueryAsync<int>(sql, new { Id = eyeAnalysisId });
        }
        
        // 3. Sclera Priority Management (CRUD)
        public async Task AddPriorityTypeAsync(ScleraPriorityType priority)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = "INSERT INTO Sclera_Priority_Types (Priority_Name, Description) VALUES (@Priority_Name, @Description)";
            await connection.ExecuteAsync(sql, priority);
        }

        public async Task UpdatePriorityTypeAsync(ScleraPriorityType priority)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = "UPDATE Sclera_Priority_Types SET Priority_Name = @Priority_Name, Description = @Description WHERE ScleraPriorityTypeID = @ScleraPriorityTypeID";
            await connection.ExecuteAsync(sql, priority);
        }

        public async Task DeletePriorityTypeAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = "DELETE FROM Sclera_Priority_Types WHERE ScleraPriorityTypeID = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // 3. Image Methods
        public async Task<IEnumerable<EyeScan>> GetScansForAnalysisAsync(int eyeAnalysisId)
        {
             using var connection = DatabaseManager.GetConnection();
             return await connection.QueryAsync<EyeScan>("SELECT * FROM Eye_Scan WHERE Eye_AnalysisID = @Id", new { Id = eyeAnalysisId });
        }

        // 4. Transactional Add
        public async Task<int> AddWithDetailsAsync(EyeAnalysis analysis, IEnumerable<ScleraPriorityType> selectedPriorities, IEnumerable<EyeScan> scans)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // A. Insert Master
                var sqlMaster = @"
                    INSERT INTO Eye_Analysis (
                        Analysis_Date, Iris_Colour, Texture, Type, Pupil, Stomach, S_I_T, ANW,
                        Bowel, Nox, Nerve_Rings, Scurf, Radii, Psora, Organs, Urine, Meridian_Scan, ClientID
                    ) VALUES (
                        @Analysis_Date, @Iris_Colour, @Texture, @Type, @Pupil, @Stomach, @S_I_T, @ANW,
                        @Bowel, @Nox, @Nerve_Rings, @Scurf, @Radii, @Psora, @Organs, @Urine, @Meridian_Scan, @ClientID
                    );
                    SELECT last_insert_rowid();";
                
                var id = await connection.ExecuteScalarAsync<int>(sqlMaster, analysis, transaction);

                // B. Insert Priorities (Junction)
                var sqlPriority = "INSERT INTO Eye_Analysis_Sclera_Priorities (Eye_AnalysisID, ScleraPriorityTypeID) VALUES (@EyeAnalysisID, @ScleraPriorityTypeID)";
                foreach (var p in selectedPriorities)
                {
                    if (p.IsSelected)
                    {
                        await connection.ExecuteAsync(sqlPriority, new { EyeAnalysisID = id, ScleraPriorityTypeID = p.ScleraPriorityTypeID }, transaction);
                    }
                }

                // C. Insert Scans (Images)
                var sqlScan = "INSERT INTO Eye_Scan (Eye_Scan, Scan_Date, Eye_Side, Eye_AnalysisID) VALUES (@Eye_Scan, @Scan_Date, @Eye_Side, @Eye_AnalysisID)";
                foreach (var scan in scans)
                {
                    scan.Eye_AnalysisID = id;
                    await connection.ExecuteAsync(sqlScan, scan, transaction);
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

        public async Task UpdateWithDetailsAsync(EyeAnalysis analysis, IEnumerable<ScleraPriorityType> selectedPriorities, IEnumerable<EyeScan> newScans)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // A. Update Master
                var sqlUpdate = @"
                    UPDATE Eye_Analysis SET
                        Analysis_Date = @Analysis_Date, Iris_Colour = @Iris_Colour, Texture = @Texture, Type = @Type,
                        Pupil = @Pupil, Stomach = @Stomach, S_I_T = @S_I_T, ANW = @ANW, Bowel = @Bowel, Nox = @Nox,
                        Nerve_Rings = @Nerve_Rings, Scurf = @Scurf, Radii = @Radii, Psora = @Psora, Organs = @Organs,
                        Urine = @Urine, Meridian_Scan = @Meridian_Scan, ClientID = @ClientID
                    WHERE Eye_AnalysisID = @Eye_AnalysisID";
                await connection.ExecuteAsync(sqlUpdate, analysis, transaction);

                // B. Sync Priorities (Delete all, re-insert selected)
                await connection.ExecuteAsync("DELETE FROM Eye_Analysis_Sclera_Priorities WHERE Eye_AnalysisID = @Id", new { Id = analysis.Eye_AnalysisID }, transaction);
                
                var sqlPriority = "INSERT INTO Eye_Analysis_Sclera_Priorities (Eye_AnalysisID, ScleraPriorityTypeID) VALUES (@EyeAnalysisID, @ScleraPriorityTypeID)";
                foreach (var p in selectedPriorities)
                {
                    if (p.IsSelected)
                    {
                         await connection.ExecuteAsync(sqlPriority, new { EyeAnalysisID = analysis.Eye_AnalysisID, ScleraPriorityTypeID = p.ScleraPriorityTypeID }, transaction);
                    }
                }

                // C. Add NEW Scans (We generally append scans, deleting is a separate action or simple append here)
                // For simplicity in this edit, we assume 'newScans' are just new ones to add
                var sqlScan = "INSERT INTO Eye_Scan (Eye_Scan, Scan_Date, Eye_Side, Eye_AnalysisID) VALUES (@Eye_Scan, @Scan_Date, @Eye_Side, @Eye_AnalysisID)";
                foreach (var scan in newScans)
                {
                     // Ensure ID is set
                     scan.Eye_AnalysisID = analysis.Eye_AnalysisID.Value;
                     // Only insert if it doesn't look like it's already there (rudimentary check or just always add new)
                     if (scan.Eye_ScanID == null)
                     {
                        await connection.ExecuteAsync(sqlScan, scan, transaction);
                     }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // IRepository Requirements
        public async Task<int> AddAsync(EyeAnalysis entity) => await AddWithDetailsAsync(entity, Enumerable.Empty<ScleraPriorityType>(), Enumerable.Empty<EyeScan>());
        public async Task<bool> UpdateAsync(EyeAnalysis entity) 
        { 
             // Should verify call sites, but default to safe update
             using var connection = DatabaseManager.GetConnection();
             return await connection.ExecuteAsync("UPDATE Eye_Analysis SET Texture=@Texture WHERE Eye_AnalysisID=@Id", new {entity.Texture, Id=entity.Eye_AnalysisID}) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
             using var connection = DatabaseManager.GetConnection();
             var rows = await connection.ExecuteAsync("DELETE FROM Eye_Analysis WHERE Eye_AnalysisID = @Id", new { Id = id });
             return rows > 0;
        }

        public async Task<IEnumerable<EyeAnalysis>> SearchAsync(string searchTerm)
        {
             using var connection = DatabaseManager.GetConnection();
             var sql = @"
                SELECT e.*, c.Name as ClientName 
                FROM Eye_Analysis e 
                JOIN Client c ON e.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                   OR e.Iris_Colour LIKE @Search
                ORDER BY e.Analysis_Date DESC";
            return await connection.QueryAsync<EyeAnalysis>(sql, new { Search = $"%{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
             using var connection = DatabaseManager.GetConnection();
             return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Eye_Analysis");
        }
    }
}
