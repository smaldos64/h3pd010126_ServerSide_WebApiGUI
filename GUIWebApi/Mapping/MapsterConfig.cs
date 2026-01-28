using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Mapster;
using System.Reflection;

namespace GUIWebAPI.Mapping
{
    public static class MapsterConfig
    {
        public static void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Product, ProductReadDto>()
                .Map(d => d.CategoryName, s => s.Category.Name)
                .Map(d => d.ImageFileId, s => s.ImageFileId)
                .Map(d => d.ImageUrl, s => s.ImageFile.RelativePath ?? string.Empty);

            config.NewConfig<ProductCreateDto, Product>()
                .Map(d => d.Name, s => s.Name)
                .Map(d => d.Price, s => s.Price)
                .Map(d => d.Description, s => s.Description)
                .Map(d => d.CategoryId, s => s.CategoryId)
                .Map(d => d.ImageFileId, s => s.ImageFileId);

            config.NewConfig<ImageFile, ImageFileReadDto>()
                .Map(d => d.ImageFileId, s => s.ImageFileId)
                .Map(d => d.FileName, s => s.FileName)
                .Map(d => d.RelativePath, s => s.RelativePath)
                .Map(d => d.Url, s => s.RelativePath);

            config.NewConfig<Category, CategoryReadDto>()
                .Map(d => d.CategoryId, s => s.CategoryId)
                .Map(d => d.Name, s => s.Name);
        }

        public static void RegisterGlobal()
        {
            TypeAdapterConfig global = TypeAdapterConfig.GlobalSettings;
            Register(global);

            global.Scan(Assembly.GetExecutingAssembly());
        }
    }
}