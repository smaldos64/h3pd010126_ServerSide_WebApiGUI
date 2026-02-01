using GUIWebApi.Models;
using GUIWebApi.Models.DTOs;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GUIWebApi.Tools;

namespace GUIWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageFilesController : ControllerBase
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment env;

        public ImageFilesController(DBContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageFileReadDto>>> GetAll()
        {
            List<ImageFile> items = await db.ImageFiles.AsNoTracking().OrderBy(x => x.FileName).ToListAsync();

            List<ImageFileReadDto> dtos = items.Adapt<List<ImageFileReadDto>>();

            foreach (ImageFileReadDto dto in dtos)
            {
                dto.Url = MakeAbsoluteUrl(dto.Url);
            }

            return Ok(dtos);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<object>> Refresh()
        {
            try
            {
                string webRoot = env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                    webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

                string imagesRoot = Path.Combine(webRoot, "images");

                Directory.CreateDirectory(imagesRoot);

                HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".svg", ".jfif"
                };

                IEnumerable<string> filesOnDisk = Directory.EnumerateFiles(imagesRoot, "*.*", SearchOption.TopDirectoryOnly).Where(p => allowedExtensions.Contains(Path.GetExtension(p)));

                List<(string FileName, string RelativePath)> diskItems = filesOnDisk.Select(p => (Path.GetFileName(p), "/images/" + Path.GetFileName(p))).ToList();

                List<ImageFile> dbItems = await db.ImageFiles.ToListAsync();

                HashSet<string> dbPaths = new HashSet<string>(dbItems.Select(i => i.RelativePath), StringComparer.OrdinalIgnoreCase);
                HashSet<string> diskPaths = new HashSet<string>(diskItems.Select(i => i.RelativePath), StringComparer.OrdinalIgnoreCase);

                List<ImageFile> toAdd = diskItems.Where(di => !dbPaths.Contains(di.RelativePath)).Select(di => new ImageFile { FileName = di.FileName, RelativePath = di.RelativePath }).ToList();

                if (toAdd.Count > 0)
                    await db.ImageFiles.AddRangeAsync(toAdd);

                List<ImageFile> toRemove = dbItems.Where(dbItem => !diskPaths.Contains(dbItem.RelativePath)).ToList();

                if (toRemove.Count > 0)
                    db.ImageFiles.RemoveRange(toRemove);

                int added = toAdd.Count;
                int removed = toRemove.Count;

                if (added > 0 || removed > 0)
                    await db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Image files synchronized.",
                    added = added,
                    removed = removed,
                    total = await db.ImageFiles.CountAsync()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to refresh image files.", detail = ex.Message });
            }
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<ActionResult<IEnumerable<ImageFileReadDto>>> Upload([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files received." });

            //string imagesRoot = GetImagesRoot();
            string imagesRoot = ImageTools.GetImagesRoot(env);
            Directory.CreateDirectory(imagesRoot);

            //HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            //{
            //    ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".svg", ".jfif"
            //};

            List<ImageFileReadDto> uploaded = new List<ImageFileReadDto>();

            foreach (IFormFile file in files)
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "One or more files are empty." });

                string extension = Path.GetExtension(file.FileName);
                if (!ImageTools.allowedExtensions.Contains(extension))
                    return BadRequest(new { message = $"File type not allowed: {extension}" });

                string safeName = SanitizeFileName(Path.GetFileName(file.FileName));
                if (string.IsNullOrWhiteSpace(safeName))
                    return BadRequest(new { message = "Invalid file name." });

                string finalName = MakeUniqueFileName(imagesRoot, safeName);
                string finalPath = Path.Combine(imagesRoot, finalName);

                using (FileStream stream = System.IO.File.Create(finalPath))
                {
                    await file.CopyToAsync(stream);
                }

                string relative = "/images/" + finalName;
                ImageFile? existing = await db.ImageFiles.FirstOrDefaultAsync(i => i.RelativePath == relative);
                if (existing == null)
                {
                    ImageFile entity = new ImageFile
                    {
                        FileName = finalName,
                        RelativePath = relative
                    };
                    await db.ImageFiles.AddAsync(entity);
                    await db.SaveChangesAsync();

                    ImageFileReadDto dtoNew = new ImageFileReadDto
                    {
                        ImageFileId = entity.ImageFileId,
                        FileName = entity.FileName,
                        RelativePath = entity.RelativePath,
                        Url = MakeAbsoluteUrl(entity.RelativePath)
                    };
                    uploaded.Add(dtoNew);
                    continue;
                }

                ImageFileReadDto dto = new ImageFileReadDto
                {
                    ImageFileId = existing.ImageFileId,
                    FileName = existing.FileName,
                    RelativePath = existing.RelativePath,
                    Url = MakeAbsoluteUrl(existing.RelativePath)
                };
                uploaded.Add(dto);
            }

            return Ok(uploaded);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Hent billedet inklusiv dets relationer til produkter
            var imageFile = await db.ImageFiles
                .Include(i => i.Products)
                .FirstOrDefaultAsync(i => i.ImageFileId == id);

            if (imageFile == null)
            {
                return NotFound(new { message = "Billedet blev ikke fundet." });
            }

            // 2. Tjek om billedet er knyttet til nogen produkter
            if (imageFile.Products != null && imageFile.Products.Any())
            {
                int productCount = imageFile.Products.Count;
                
                string errorString = $"Billedet kan ikke slettes, da det bruges af {productCount} produkt(er) ";
                errorString += "\nMed disse Produkt Id'er og Produktnavne: \n";
                errorString += string.Join("\n", imageFile.Products.Select(p => $"- [ProductId: {p.ProductId}] {p.Name} "));
                return BadRequest(errorString);
            }

            try
            {
                // 3. Find den fysiske sti
                string imagesRoot = GetImagesRoot();
                string filePath = Path.Combine(imagesRoot, imageFile.FileName);

                // 4. Slet filen fra disken
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // 5. Slet fra databasen
                db.ImageFiles.Remove(imageFile);
                await db.SaveChangesAsync();

                return Ok(new { message = $"Filen {imageFile.FileName} er slettet korrekt." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kunne ikke slette billedet.", detail = ex.Message });
            }
        }
        private string GetImagesRoot()
        {
            string webRoot = env.WebRootPath;
            if (!string.IsNullOrWhiteSpace(webRoot))
                return Path.Combine(webRoot, "images");

            return Path.Combine(AppContext.BaseDirectory, "wwwroot", "images");
        }

        private static string SanitizeFileName(string fileName)
        {
            string name = fileName.Replace("\\", string.Empty).Replace("/", string.Empty);
            name = Regex.Replace(name, @"\s+", "-");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), string.Empty);
            }
            return name.Trim();
        }

        private static string MakeUniqueFileName(string folder, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            string candidate = fileName;
            int i = 1;

            while (System.IO.File.Exists(Path.Combine(folder, candidate)))
            {
                candidate = string.Concat(nameWithoutExt, "-", i.ToString(), ext);
                i++;
            }

            return candidate;
        }

        private string MakeAbsoluteUrl(string virtualOrRelativePath)
        {
            if (string.IsNullOrWhiteSpace(virtualOrRelativePath))
                return string.Empty;

            if (virtualOrRelativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return virtualOrRelativePath;

            string baseUrl = string.Concat(Request.Scheme, "://", Request.Host.ToUriComponent());
            string path = virtualOrRelativePath.StartsWith("/") ? virtualOrRelativePath : "/" + virtualOrRelativePath;

            return baseUrl + path;
        }

        private async Task<string> GetFastHashAsync(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                // Vi læser kun de første 8192 bytes
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                using (SHA256 sha256 = SHA256.Create())
                {
                    // Start hashing med bufferen
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);

                    // Tilføj filstørrelsen for at gøre hashen mere unik
                    byte[] sizeBytes = BitConverter.GetBytes(file.Length);
                    sha256.TransformBlock(sizeBytes, 0, sizeBytes.Length, null, 0);

                    // Tilføj filnavnet
                    byte[] nameBytes = Encoding.UTF8.GetBytes(file.FileName.ToLower());
                    sha256.TransformFinalBlock(nameBytes, 0, nameBytes.Length);

                    return BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}