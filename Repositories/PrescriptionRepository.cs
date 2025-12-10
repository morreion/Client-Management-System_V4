using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class PrescriptionRepository : IRepository<Prescription>
    {
        // Standard Get All (Master records only for list)
        public async Task<IEnumerable<Prescription>> GetAllAsync()
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT p.*, c.Name as ClientName 
                FROM Prescription p 
                JOIN Client c ON p.ClientID = c.ClientID 
                ORDER BY p.Prescription_Date DESC";
            return await connection.QueryAsync<Prescription>(sql);
        }

        public async Task<Prescription?> GetByIdAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT p.*, c.Name as ClientName 
                FROM Prescription p 
                JOIN Client c ON p.ClientID = c.ClientID 
                WHERE p.PrescriptionID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Prescription>(sql, new { Id = id });
        }

        // Get Supplements for a specific Prescription
        public async Task<IEnumerable<PrescriptionSupplement>> GetSupplementsByPrescriptionIdAsync(int prescriptionId)
        {
            using var connection = DatabaseManager.GetConnection();
            var sql = @"
                SELECT ps.*, s.Name as SupplementName 
                FROM Prescription_Supplements ps 
                JOIN Supplements s ON ps.SupplementID = s.SupplementID 
                WHERE ps.PrescriptionID = @PrescriptionID";
            return await connection.QueryAsync<PrescriptionSupplement>(sql, new { PrescriptionID = prescriptionId });
        }

        // Transactional Add
        public async Task<int> AddWithSupplementsAsync(Prescription prescription, IEnumerable<PrescriptionSupplement> supplements)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert Master
                var sqlMaster = @"
                    INSERT INTO Prescription (Prescription_Date, Next_Appointment_Date, Recommendations, ClientID) 
                    VALUES (@Prescription_Date, @Next_Appointment_Date, @Recommendations, @ClientID);
                    SELECT last_insert_rowid();";
                
                var id = await connection.ExecuteScalarAsync<int>(sqlMaster, prescription, transaction);

                // 2. Insert Details
                var sqlDetail = @"
                    INSERT INTO Prescription_Supplements (Breakfast, Lunch, Dinner, Bedtime, PrescriptionID, SupplementID) 
                    VALUES (@Breakfast, @Lunch, @Dinner, @Bedtime, @PrescriptionID, @SupplementID)";

                foreach (var s in supplements)
                {
                    s.PrescriptionID = id; // Link to new master ID
                    await connection.ExecuteAsync(sqlDetail, s, transaction);
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

        public async Task UpdateWithSupplementsAsync(Prescription prescription, IEnumerable<PrescriptionSupplement> supplements)
        {
            using var connection = DatabaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Update Master
                var sqlUpdateMaster = @"
                    UPDATE Prescription 
                    SET Prescription_Date = @Prescription_Date, 
                        Next_Appointment_Date = @Next_Appointment_Date, 
                        Recommendations = @Recommendations, 
                        ClientID = @ClientID
                    WHERE PrescriptionID = @PrescriptionID";
                
                await connection.ExecuteAsync(sqlUpdateMaster, prescription, transaction);

                // 2. Delete Existing Supplements
                await connection.ExecuteAsync(
                    "DELETE FROM Prescription_Supplements WHERE PrescriptionID = @PrescriptionID", 
                    new { prescription.PrescriptionID }, transaction);

                // 3. Re-Insert Supplements (simplest update strategy for junction tables)
                var sqlDetail = @"
                    INSERT INTO Prescription_Supplements (Breakfast, Lunch, Dinner, Bedtime, PrescriptionID, SupplementID) 
                    VALUES (@Breakfast, @Lunch, @Dinner, @Bedtime, @PrescriptionID, @SupplementID)";

                foreach (var s in supplements)
                {
                    s.PrescriptionID = prescription.PrescriptionID.Value;
                    await connection.ExecuteAsync(sqlDetail, s, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Required by Interface (but we encourage using the specialized methods above)
        public async Task<int> AddAsync(Prescription entity) => await AddWithSupplementsAsync(entity, new List<PrescriptionSupplement>());
        public async Task<bool> UpdateAsync(Prescription entity) 
        { 
             // This assumes no changes to supplements if called directly. 
             // Ideally use UpdateWithSupplementsAsync from VM.
             using var connection = DatabaseManager.GetConnection();
             return await connection.ExecuteAsync("UPDATE Prescription SET Recommendations=@Recommendations WHERE PrescriptionID=@PrescriptionID", entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = DatabaseManager.GetConnection();
            // Cascade delete handles the supplements automatically if configured, otherwise needs manual delete
            // Our schema has ON DELETE CASCADE
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Prescription WHERE PrescriptionID = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Prescription>> SearchAsync(string searchTerm)
        {
             using var connection = DatabaseManager.GetConnection();
             var sql = @"
                SELECT p.*, c.Name as ClientName 
                FROM Prescription p 
                JOIN Client c ON p.ClientID = c.ClientID 
                WHERE c.Name LIKE @Search 
                   OR p.Recommendations LIKE @Search 
                ORDER BY p.Prescription_Date DESC";
            return await connection.QueryAsync<Prescription>(sql, new { Search = $"%{searchTerm}%" });
        }

        public async Task<int> CountAsync()
        {
             using var connection = DatabaseManager.GetConnection();
             return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Prescription");
        }
    }
}
