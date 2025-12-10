using System;
using System.Collections.Generic;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a prescription record for a client visit
    /// </summary>
    public class Prescription
    {
        public int? PrescriptionID { get; set; }
        public DateTime? Prescription_Date { get; set; } = DateTime.Now;
        public DateTime? Next_Appointment_Date { get; set; }
        public string? Recommendations { get; set; }
        public int ClientID { get; set; }

        // Computed
        public string? ClientName { get; set; }
        
        // Navigation Property: List of supplements for this prescription
        // We will populate this when fetching details or before saving
        public List<PrescriptionSupplement> Supplements { get; set; } = new List<PrescriptionSupplement>();
    }
}
