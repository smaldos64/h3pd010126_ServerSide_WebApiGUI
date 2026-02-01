namespace GUIWebApi.Models.DTOs
{
    public class FileInventoryReadDto
    {
        public int FileInventoryId { get; set; }
        public string? ContentHash { get; set; } // SHA256 af indholdet
        public string? PhysicalPath { get; set; } // Hvor den bor på serveren
        public long FileSize { get; set; }
    }
}
