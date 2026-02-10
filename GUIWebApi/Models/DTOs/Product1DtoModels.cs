namespace GUIWebApi.Models.DTOs
{
    public class Product1CreateDto
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Category1Id { get; set; }
        public int? UserFileId { get; set; }
    }

    public class Product1UpdateDto : Product1CreateDto
    {
        public int Product1Id { get; set; }
    }

    public class Product1WithCategoryDto : Product1UpdateDto
    {
        public Category1UpdateDto? Category { get; set; }
    }

    public class Product1WithUserFileDto : Product1UpdateDto
    {
        public UserFileWithInventoryFileDto? UserFile { get; set; }
    }

    public class Product1Dto : Product1UpdateDto
    {
        public Category1UpdateDto? Category { get; set; }
        public UserFileDto? UserFile { get; set; }
        //UserFileWithInventoryFileDto
    }
}
