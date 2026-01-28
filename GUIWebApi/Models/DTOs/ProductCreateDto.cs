using System.ComponentModel.DataAnnotations;

namespace GUIWebAPI.Models.DTOs
{
    public sealed class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price must be >= 1")]
        public decimal Price { get; set; }

        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be >= 1")]
        public int CategoryId { get; set; }

        public int? ImageFileId { get; set; }
    }
}