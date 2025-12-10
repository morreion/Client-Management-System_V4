using System;

namespace Client_Management_System_V4.Models
{
    public class EyeScan
    {
        public int? Eye_ScanID { get; set; }
        public byte[]? Eye_Scan { get; set; } // BLOB data
        public DateTime? Scan_Date { get; set; }
        public string? Eye_Side { get; set; } // 'Left' or 'Right'
        public int Eye_AnalysisID { get; set; }
    }
}
