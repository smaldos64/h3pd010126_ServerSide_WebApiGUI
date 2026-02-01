namespace GUIWebApi.Models.DTOs
{
    public class UserFileReadDto
    {
        public int UserFileId { get; set; }
        public string? DisplayName { get; set; }
        public DateTime UploadDate { get; set; }
        public int FileInventoryId { get; set; }
        //public string ?Inventory { get; set; }
    }

    public class  UserFileDto : UserFileReadDto
    {
        public InventoryFileReadDto? Inventory { get; set; }
    }
}
