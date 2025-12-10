using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a treatment plan or clinical impression
    /// </summary>
    public class Treatment
    {
        public int? TreatmentID { get; set; }
        public DateTime? Treatment_Date { get; set; } = DateTime.Now;
        public string? Expectations_of_Treatment { get; set; }
        public string? Impression { get; set; }
        public string? Presenting_Symptoms { get; set; }
        public string? Rx { get; set; } // Prescription/Recommendations summary
        public int ClientID { get; set; }

        // Computed Property
        public string? ClientName { get; set; }
    }
}
