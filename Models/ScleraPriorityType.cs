namespace Client_Management_System_V4.Models
{
    public class ScleraPriorityType
    {
        public int ScleraPriorityTypeID { get; set; }
        public string Priority_Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Helper for UI selection
        public bool IsSelected { get; set; }
    }
}
