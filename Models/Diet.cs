using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents dietary information and meal plans for a client visit
    /// </summary>
    public class Diet
    {
        public int? DietID { get; set; }
        public DateTime? Diet_Date { get; set; } = DateTime.Now;
        public string? Breakfast { get; set; }
        public string? Lunch { get; set; }
        public string? Dinner { get; set; }
        public string? Snacks { get; set; }
        public int ClientID { get; set; }

        // Computed Property for Display
        public string? ClientName { get; set; }
    }
}
