using GUIWebApi.Models.DTOs;

namespace GUIWebApi.ViewModels
{
    public class ImageFilesInfo
    {
        public InventoryFileCreateDto? FileInventory { get; set; }
        public UserFileCreateDto? UserFile { get; set; }
    }
}
