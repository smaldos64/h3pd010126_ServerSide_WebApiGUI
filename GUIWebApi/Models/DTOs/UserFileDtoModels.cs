namespace GUIWebApi.Models.DTOs
{
    public class UserFileCreateDto
    {
        public string? DisplayName { get; set; }
        public DateTime UploadDate { get; set; }
        public int InventoryFileId { get; set; }
    }

    public class  UserFileUpdateDto : UserFileCreateDto
    {
        public int UserFileId { get; set; }
    }
      

    public class UserFileWithInventoryFileDto : UserFileUpdateDto
    {
        public InventoryFileUpdateDto? InventoryFile { get; set; }
    }

    public class UserFileWithProductDto : UserFileUpdateDto
    {
        public List<Product1WithCategoryDto>? Products1 { get; set; }
    }

    public class UserFileDto : UserFileUpdateDto
    {
        public InventoryFileUpdateDto? InventoryFile { get; set; }
        
        public List<Product1WithCategoryDto>? Products1 { get; set; }
    }
}
