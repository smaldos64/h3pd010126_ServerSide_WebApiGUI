namespace GUIWebApi.Models
{
    public class InventoryFile
    {
        public int InventoryFileId { get; set; }
        public string? ContentHash { get; set; } // SHA256 af indholdet
        public string? PhysicalPath { get; set; } // Hvor den bor på serveren
        public long FileSize { get; set; }

        public ICollection<UserFile>? UserFiles { get; set; }
    }
}
