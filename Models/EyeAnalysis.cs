using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents an eye analysis / iridology session
    /// </summary>
    public class EyeAnalysis
    {
        public int? Eye_AnalysisID { get; set; }
        public DateTime? Analysis_Date { get; set; } = DateTime.Now;
        public string? Iris_Colour { get; set; }
        public string? Texture { get; set; }
        public string? Type { get; set; }
        public string? Pupil { get; set; }
        public string? Stomach { get; set; }
        public string? S_I_T { get; set; }
        public string? ANW { get; set; }
        public string? Bowel { get; set; }
        public string? Nox { get; set; }
        public string? Nerve_Rings { get; set; }
        public string? Scurf { get; set; }
        public string? Radii { get; set; }
        public string? Psora { get; set; }
        public string? Organs { get; set; }
        public string? Urine { get; set; }
        public string? Meridian_Scan { get; set; }
        public int ClientID { get; set; }

        public string? ClientName { get; set; }
    }
}
