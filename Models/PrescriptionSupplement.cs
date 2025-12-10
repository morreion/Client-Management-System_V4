namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a supplement item within a prescription (Junction Table)
    /// </summary>
    public class PrescriptionSupplement
    {
        public int? Prescription_SupplementsID { get; set; }
        public string? Breakfast { get; set; }
        public string? Lunch { get; set; }
        public string? Dinner { get; set; }
        public string? Bedtime { get; set; }
        public int PrescriptionID { get; set; }
        public int SupplementID { get; set; }

        // Computed
        public string? SupplementName { get; set; }
    }
}
