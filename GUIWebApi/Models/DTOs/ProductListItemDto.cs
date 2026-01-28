namespace GUIWebAPI.Models.DTOs
{
    public sealed class ProductListItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}