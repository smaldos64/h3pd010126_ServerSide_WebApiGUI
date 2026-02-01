namespace GUIWebApi.Tools
{
    public static class ImageTools
    {
        public static readonly HashSet<string> allowedExtensions = new HashSet<string>
        {
            ".jpeg",
            ".jpg",
            ".png",
            ".gif",
            ".bmp",
            ".webp",
            ".tiff",
            ".jfif"
        };

        public static string GetImagesRoot(IWebHostEnvironment env)
        {
            string webRoot = env.WebRootPath;
            if (!string.IsNullOrWhiteSpace(webRoot))
                return Path.Combine(webRoot, "images");

            return Path.Combine(AppContext.BaseDirectory, "wwwroot", "images");
        }
    }
}
