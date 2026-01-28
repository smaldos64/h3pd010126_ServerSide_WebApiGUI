namespace GUIWebAPI.Models
{
    public sealed class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int? ImageFileId { get; set; }
        public ImageFile ImageFile { get; set; }
    }
}