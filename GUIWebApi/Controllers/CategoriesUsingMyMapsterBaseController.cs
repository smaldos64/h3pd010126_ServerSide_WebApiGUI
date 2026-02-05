using GUIWebApi.Models.DTOs;
using GUIWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GUIWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesUsingMyMapsterBaseController : MyMapsterBaseController<CategoriesUsingMyMapsterBaseController, DBContext>
    {
        public CategoriesUsingMyMapsterBaseController(DBContext db, ILogger<CategoriesUsingMyMapsterBaseController> logger) : base(db, logger)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category1Dto>>> GetAll()
            => await ProjectListAsync<Category1, Category1Dto>(_db.Categories1, useTracking: false);

        [HttpGet("{id}")]
        public async Task<ActionResult<Category1Dto>> ProjectSingleAsync(int id)
            => await ProjectSingleAsync<Category1, Category1Dto>(_db.Categories1.Where(p => p.Category1Id == id));

        [HttpGet("GetAllInventoryImages/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Category1Dto>>> GetAllByCategoryId(int categoryId)
            => await GetFilteredAsync<Category1, Category1Dto>(p => p.Category1Id == categoryId || p.Category1Id == categoryId + 1, useTracking: false);

        [HttpPost]
        public async Task<ActionResult<Category1UpdateDto>> Create(Category1CreateDto dto)
            => await CreateAsync<Category1, Category1CreateDto, Category1UpdateDto>(dto);

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Category1UpdateDto dto)
            => await UpdateAsync<Category1, Category1UpdateDto>(id, dto);

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await DeleteAsync<Category1>(id);
    }
}
