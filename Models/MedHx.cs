using System;
using System.Collections.Generic;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a Medical History record for a client visit.
    /// Includes general history notes and a list of specific supplements taken at that time.
    /// </summary>
    public class MedHx
    {
        public int? Med_HxID { get; set; }
        public int ClientID { get; set; }
        public DateTime? Assessment_Date { get; set; } = DateTime.Now;
        
        // Medical History Text Fields
        public string? Blood_Test_Results { get; set; }
        public string? Medication { get; set; }
        public string? Supplements { get; set; } // Free text notes
        public string? Accidents_Previous_Illness { get; set; }
        public string? Menstrual_Notes { get; set; }
        public string? Vaccinations { get; set; }
        public string? Family_Med_Hx { get; set; }
        
        // Note: The schema has a column named "Med_Hx" which conflicts with the Class Name if we aren't careful.
        // In C#, properties can't have the same name as the class.
        // We will map this property to "HistoryNotes" in the model, but map it to "Med_Hx" in Dapper queries.
        public string? HistoryNotes { get; set; } 

        // Computed / UI
        public string? ClientName { get; set; }

        // Navigation Property for the Detail Table
        public List<MedHxSupplement> MedHxSupplements { get; set; } = new List<MedHxSupplement>();
    }
}
