namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a specific supplement entry within a Medical History record.
    /// Links to the main Supplements table.
    /// </summary>
    public class MedHxSupplement
    {
        public int? Med_Hx_SupplementsID { get; set; }
        public int Med_HxID { get; set; }
        public int SupplementID { get; set; }
        
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Notes { get; set; }

        // Computed
        public string? SupplementName { get; set; }
    }
}
