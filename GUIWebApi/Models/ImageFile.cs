namespace GUIWebAPI.Models
{
    public sealed class ImageFile
    {
        public int ImageFileId { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}