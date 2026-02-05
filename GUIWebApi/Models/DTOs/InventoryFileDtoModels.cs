namespace GUIWebApi.Models.DTOs
{
    public class InventoryFileCreateDto
    {
        public string? ContentHash { get; set; } // SHA256 af indholdet
        public string? PhysicalPath { get; set; } // Hvor den bor på serveren
        public string? RelativePath { get; set; } // Relative Path from wwwroot
        public long FileSize { get; set; }
        public string? Url { get; set; }
    }

    public class InventoryFileUpdateDto : InventoryFileCreateDto
    {
        public int InventoryFileId { get; set; }// No additional fields for now
    }   

    public class InventoryFileDto : InventoryFileUpdateDto
    {
        public List<UserFileWithProductDto>? UserFiles { get; set; }
    }
}
