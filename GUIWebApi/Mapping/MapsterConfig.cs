using GUIWebApi.Models;
using GUIWebApi.Models.DTOs;
using GUIWebApi.Tools;
using Mapster;
using System.Reflection;

namespace GUIWebAPI.Mapping
{
    public static class MapsterConfig
    {
        public static void Register(TypeAdapterConfig config)
        {
            config.NewConfig<InventoryFile, InventoryFileCreateDto>()
                .Map(d => d.Url, s => s.RelativePath.MakeUrl());

            config.NewConfig<InventoryFile, InventoryFileUpdateDto>()
                .Map(d => d.Url, s => s.RelativePath.MakeUrl());

            config.NewConfig<InventoryFile, InventoryFileDto>()
                .Map(d => d.Url, s => s.RelativePath.MakeUrl());
        }

        public static void RegisterGlobal()
        {
            TypeAdapterConfig global = TypeAdapterConfig.GlobalSettings;
            Register(global);

            global.Scan(Assembly.GetExecutingAssembly());
        }
    }
}