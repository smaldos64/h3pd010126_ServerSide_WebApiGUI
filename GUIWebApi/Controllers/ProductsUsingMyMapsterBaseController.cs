using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GUIWebApi.Models;
using GUIWebApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GUIWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsUsingMyMapsterBaseController : MyMapsterBaseController<ProductsUsingMyMapsterBaseController, DBContext>
    {
        public ProductsUsingMyMapsterBaseController(DBContext db, ILogger<ProductsUsingMyMapsterBaseController> logger) : base(db, logger)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product1Dto>>> GetAll()
            => await ProjectListAsync<Product1, Product1Dto>(_db.Products1, useTracking: false);

        [HttpGet("{id}")]
        public async Task<ActionResult<Product1Dto>> ProjectSingleAsync(int id)
            => await ProjectSingleAsync<Product1, Product1Dto>(_db.Products1.Where(p => p.Product1Id == id));
            
        [HttpGet("GetAllInventoryImages/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product1Dto>>> GetAllByCategoryId(int categoryId)
            => await GetFilteredAsync<Product1, Product1Dto>(p => p.Category1Id == categoryId, useTracking:false);

        [HttpPost]
        public async Task<ActionResult<Product1UpdateDto>> Create(Product1CreateDto dto)
            => await CreateAsync<Product1, Product1CreateDto, Product1UpdateDto>(dto);

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product1UpdateDto dto)
            => await UpdateAsync<Product1, Product1UpdateDto>(id, dto);

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await DeleteAsync<Product1>(id);
    }
}
