using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a client in the health management system
    /// </summary>
    public class Client
    {
        public int ClientID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public DateTime? Date_First_Consultation { get; set; }
        public DateTime? Date_Last_Consultation { get; set; }
        public string? Marital_Status { get; set; }
        public int? Children { get; set; }
        public string? Ref { get; set; }
        public string? Alt_Contact { get; set; }

        // Computed property for display
        public string DisplayName => $"{Name} ({Mobile ?? "No phone"})";
        
        // Age calculation
        public int? Age
        {
            get
            {
                if (DOB == null) return null;
                var today = DateTime.Today;
                var age = today.Year - DOB.Value.Year;
                if (DOB.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
