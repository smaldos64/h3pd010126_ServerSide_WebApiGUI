namespace GUIWebAPI.Models.DTOs
{
    public sealed class CategoryWithProductsReadDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public List<ProductListItemDto> Products { get; set; }
    }
}