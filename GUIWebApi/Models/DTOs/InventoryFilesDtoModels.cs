namespace GUIWebApi.Models.DTOs
{
    public class InventoryFileReadDto
    {
        public int InventoryFileId { get; set; }
        public string? ContentHash { get; set; } // SHA256 af indholdet
        public string? PhysicalPath { get; set; } // Hvor den bor på serveren
        public long FileSize { get; set; }
        public string? Url { get; set; }
    }

    public class InventoryFileDto : InventoryFileReadDto
    {
        public List<UserFileReadDto>? UserFiles { get; set; }
    }
}
