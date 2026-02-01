using GUIWebApi.Models.DTOs;

namespace GUIWebApi.ViewModels
{
    public class ImageFilesInfo
    {
        public InventoryFileReadDto? FileInventory { get; set; }
        public UserFileReadDto? UserFile { get; set; }
    }
}
