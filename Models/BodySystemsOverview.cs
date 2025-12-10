using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a comprehensive review of body systems
    /// </summary>
    public class BodySystemsOverview
    {
        public int? Body_Systems_OverviewID { get; set; }
        public DateTime? Assessment_Date { get; set; } = DateTime.Now;
        public string? Immune { get; set; }
        public string? Allergy { get; set; }
        public string? Sleep { get; set; }
        public string? Snore { get; set; }
        public string? Smoke_Alc { get; set; }
        public string? Exercise { get; set; }
        public string? Tongue { get; set; }
        public string? Cravings { get; set; }
        public string? Beverages { get; set; }
        public string? Digestion { get; set; }
        public string? Bowels { get; set; }
        public string? Urination { get; set; }
        public string? Head { get; set; }
        public string? ENT { get; set; }
        public string? Skin_Hair { get; set; }
        public string? Nails { get; set; }
        public string? Mind_Emotional { get; set; }
        public string? Thyroid { get; set; }
        public string? Backache { get; set; }
        public string? Joint_Pain { get; set; }
        public int ClientID { get; set; }

        // Computed
        public string? ClientName { get; set; }
    }
}
