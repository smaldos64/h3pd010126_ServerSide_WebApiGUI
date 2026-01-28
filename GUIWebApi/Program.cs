using GUIWebAPI.Mapping;
using GUIWebAPI.Models;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace GUIWebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactClient", policy =>
                {
                    policy.SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            TypeAdapterConfig typeadapterconfig = TypeAdapterConfig.GlobalSettings;
            MapsterConfig.Register(typeadapterconfig);
            typeadapterconfig.Scan(Assembly.GetExecutingAssembly());

            builder.Services.AddSingleton(typeadapterconfig);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            string contentRoot = builder.Environment.ContentRootPath;
            string webRootPath = Path.Combine(contentRoot, "wwwroot");
            string imagesPath = Path.Combine(webRootPath, "images");
            Directory.CreateDirectory(imagesPath);

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 500_000_000;
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 500_000_000;
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("ReactClient");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            });

            app.UseStaticFiles();

            string fallbackWwwRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            if (Directory.Exists(fallbackWwwRoot))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(fallbackWwwRoot),
                    RequestPath = ""
                });
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}