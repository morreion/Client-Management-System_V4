using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a supplement or herbal remedy
    /// </summary>
    public class Supplement
    {
        public int? SupplementID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public int DistributorID { get; set; }
        
        // Computed property for display
        public string? DistributorName { get; set; }
    }
}
