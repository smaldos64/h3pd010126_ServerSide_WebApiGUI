namespace GUIWebApi.Models
{
    public class UserFile
    {
        public int UserFileId { get; set; }
        public string? DisplayName { get; set; } // "ferie.jpg"
        public DateTime UploadDate { get; set; }
        public int InventoryFileId { get; set; } // Relation til den fysiske fil
        public InventoryFile? Inventory { get; set; }

        public ICollection<Product1> Products1 { get; set; }
    }
}
