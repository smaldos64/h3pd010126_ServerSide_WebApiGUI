using GUIWebApi.Models;
using GUIWebApi.Models.DTOs;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryFilesController : MyMapsterBaseController<InventoryFilesController, DBContext>
    {
        private readonly DBContext db;

        public InventoryFilesController(DBContext db, ILogger<InventoryFilesController> logger) : base(db, logger)
        {
            this.db = db;
        }

        [HttpGet("GetAllInventoryImages")]
        public async Task<IActionResult> GetAllInventoryImages()
        {
            List<InventoryFile> items = await db.InventoryFiles.
                Include(u => u.UserFiles).
                ThenInclude(p => p.Products1).
                ThenInclude(c => c.Category).
                AsNoTracking().ToListAsync();

            List<InventoryFileDto> dtos = items.Adapt<List<InventoryFileDto>>();

            return Ok(dtos);
        }

        [HttpGet("GetAllInventoryImages_1")]
        public async Task<ActionResult<IEnumerable<InventoryFileDto>>> GetAllInventoryImages_1()
        {
            return (await ProjectListAsync<InventoryFile, InventoryFileDto>(_db.InventoryFiles, useTracking: false));
        }
    }
}
