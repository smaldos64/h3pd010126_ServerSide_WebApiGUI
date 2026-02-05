namespace GUIWebApi.Models.DTOs
{
    public class Category1CreateDto
    {
        public string? Name { get; set; }
    }
    public class Category1UpdateDto : Category1CreateDto
    {
        public int Category1Id { get; set; }
    }

    public class Category1Dto : Category1UpdateDto
    {
        public List<Product1WithUserFileDto>? Products1 { get; set; }
    }
}
