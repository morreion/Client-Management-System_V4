using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a scanned document/note associated with a client.
    /// Stores PDF files or images as binary data (BLOB).
    /// </summary>
    public class ScannedNote
    {
        /// <summary>
        /// Primary key - auto-incremented ID
        /// </summary>
        public int ScannedNotesID { get; set; }

        /// <summary>
        /// Name/title of the document
        /// </summary>
        public string? Document_Name { get; set; }

        /// <summary>
        /// Date the document was created or scanned
        /// </summary>
        public DateTime? Document_Date { get; set; }

        /// <summary>
        /// Type of document: "PDF", "PNG", "JPG", "JPEG", etc.
        /// </summary>
        public string? Document_Type { get; set; }

        /// <summary>
        /// Description or notes about the document
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Binary data of the scanned document (BLOB)
        /// </summary>
        public byte[]? Scanned_Document { get; set; }

        /// <summary>
        /// Foreign key to Client table
        /// </summary>
        public int ClientID { get; set; }

        #region Display Helpers

        /// <summary>
        /// Returns true if the document is a PDF file
        /// </summary>
        public bool IsPdf => Document_Type?.ToUpperInvariant() == "PDF";

        /// <summary>
        /// Returns true if the document is an image file (not PDF)
        /// </summary>
        public bool IsImage => !IsPdf && !string.IsNullOrEmpty(Document_Type);

        /// <summary>
        /// Display string combining name and date
        /// </summary>
        public string DisplayName => $"{Document_Name ?? "Untitled"} ({Document_Date?.ToString("dd/MM/yyyy") ?? "No date"})";

        #endregion
    }
}
