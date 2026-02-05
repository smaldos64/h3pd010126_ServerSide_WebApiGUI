namespace GUIWebApi.Models
{
    public sealed class Product1
    {
        public int Product1Id { get; set; }
        public string? Name { get; set; }

        public decimal Price { get; set; }
        public string? Description { get; set; }

        public int Category1Id { get; set; }
        public Category1 Category { get; set; }

        public int? UserFileId { get; set; }
        public UserFile? UserFile { get; set; }
    }
}
