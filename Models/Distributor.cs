using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a supplement distributor/supplier
    /// </summary>
    public class Distributor
    {
        public int? DistributorID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Work_Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
    }
}
