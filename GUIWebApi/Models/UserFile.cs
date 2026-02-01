namespace GUIWebApi.Models
{
    public class UserFile
    {
        public int UserFileId { get; set; }
        public string? DisplayName { get; set; } // "ferie.jpg"
        public DateTime UploadDate { get; set; }
        public int FileInventoryId { get; set; } // Relation til den fysiske fil
        public FileInventory? Inventory { get; set; }

        public ICollection<Product1> Products1 { get; set; }
    }
}
