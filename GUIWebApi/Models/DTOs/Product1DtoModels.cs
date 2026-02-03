namespace GUIWebApi.Models.DTOs
{
    public class Product1CreateDto
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Category1Id { get; set; }
        public int? ImageFileId { get; set; }
    }

    public class Product1UpdateDto : Product1CreateDto
    {
        public int Product1Id { get; set; }
    }

    public class Product1Dto : Product1UpdateDto
    {
        public Category1UpdateDto? Category { get; set; }
        public UserFileReadDto? ImageFile { get; set; }
    }
}
