using GUIWebApi.Models;
using GUIWebApi.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GUIWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageFiles1Controller : ControllerBase
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment env;

        public ImageFiles1Controller(DBContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public async Task ProcessSmartUpload(IFormFile file)
        {
            // 1. Generer den hurtige hash (8KB + Size)
            string fileHash = await GetFastHashAsync(file);

            // 2. Tjek om indholdet ALLEREDE findes i vores 'Inventory'
            var inventory = await db.FileInventories
                .FirstOrDefaultAsync(x => x.ContentHash == fileHash);

            if (inventory == null)
            {
                // Filen er ny! Gem den fysisk på disken
                // Vi bruger hashen som filnavn for at undgå dubletter på disk-niveau
                string storageName = $"{fileHash}{Path.GetExtension(file.FileName)}";
                string path = Path.Combine("storage/files", storageName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                inventory = new FileInventory
                {
                    ContentHash = fileHash,
                    PhysicalPath = path,
                    FileSize = file.Length
                };
                db.FileInventories.Add(inventory);
                await db.SaveChangesAsync();
            }

            // 3. Opret altid en reference til brugeren (selvom filen fandtes i forvejen)
            var userFile = new UserFile
            {
                DisplayName = file.FileName,
                UploadDate = DateTime.UtcNow,
                FileInventoryId = inventory.FileInventoryId
            };
            db.UserFiles.Add(userFile);
            await db.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserFileAsync(int userFileId)
        {
            // 1. Find brugerens fil-reference
            var userFile = await db.UserFiles
                .Include(u => u.Inventory)
                .FirstOrDefaultAsync(u => u.UserFileId == userFileId);

            if (userFile == null) return false;

            var inventoryId = userFile.FileInventoryId;
            var inventoryRecord = userFile.Inventory;

            // 2. Fjern brugerens reference fra DB
            db.UserFiles.Remove(userFile);
            await db.SaveChangesAsync();

            // 3. Tjek om andre stadig bruger den fysiske fil
            bool isStillInUse = await db.UserFiles
                .AnyAsync(u => u.FileInventoryId == inventoryId);

            if (!isStillInUse)
            {
                // Ingen bruger den længere - kør Clean-up for denne specifikke post
                await CleanUpPhysicalFileAsync(inventoryRecord);
            }

            return true;
        }

        private async Task CleanUpPhysicalFileAsync(FileInventory inventory)
        {
            try
            {
                // 1. Slet filen fra harddisken
                if (System.IO.File.Exists(inventory.PhysicalPath))
                {
                    System.IO.File.Delete(inventory.PhysicalPath);
                }

                // 2. Fjern posten fra Inventory-tabellen
                db.FileInventories.Remove(inventory);
                await db.SaveChangesAsync();
            }
            catch (IOException ex)
            {
                // Log fejlen - vi kunne ikke slette filen lige nu. 
                // Den kan evt. tages af en natlig Background Worker.
                //_logger.LogError($"Kunne ikke slette fysisk fil: {inventory.PhysicalPath}", ex);
                int test = 10;
            }
        }

        public async Task<string> GetFastHashAsync(IFormFile file)
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
