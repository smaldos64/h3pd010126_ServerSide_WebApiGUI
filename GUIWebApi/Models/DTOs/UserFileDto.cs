namespace GUIWebApi.Models.DTOs
{
    public class UserFileDto
    {
        public int UserFileId { get; set; }
        public string? DisplayName { get; set; }
        public DateTime UploadDate { get; set; }
        public int FileInventoryId { get; set; }
        public List<FileInventoryReadDto> Inventories { get; set; }
    }
}
