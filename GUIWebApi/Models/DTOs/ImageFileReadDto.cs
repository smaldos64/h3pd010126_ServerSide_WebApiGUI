namespace GUIWebAPI.Models.DTOs
{
    public sealed class ImageFileReadDto
    {
        public int ImageFileId { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string Url { get; set; }
    }
}