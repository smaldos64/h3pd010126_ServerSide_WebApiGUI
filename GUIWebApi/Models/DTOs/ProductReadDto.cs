namespace GUIWebAPI.Models.DTOs
{
    public sealed class ProductReadDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int? ImageFileId { get; set; }
        public string ImageUrl { get; set; }
    }
}