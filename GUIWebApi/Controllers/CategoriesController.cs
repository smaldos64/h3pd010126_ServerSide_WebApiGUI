using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DBContext db;

        public CategoriesController(DBContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll([FromQuery] bool includeProducts = false, [FromQuery] string? q = null)
        {
            IQueryable<Category> query = db.Categories.AsNoTracking();

            if (includeProducts)
                query = query.Include(c => c.Products);

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Name, "%" + term + "%"));
            }

            List<Category> items = await query.OrderBy(c => c.Name).ToListAsync();

            if (!includeProducts)
            {
                List<CategoryReadDto> flat = items.Select(c => new CategoryReadDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                }).ToList();

                return Ok(flat);
            }

            List<CategoryWithProductsReadDto> withProducts = items.Select(c => new CategoryWithProductsReadDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Products = c.Products.OrderBy(p => p.Name).ThenBy(p => p.ProductId).Select(p => new ProductListItemDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price
                }).ToList()
            }).ToList();

            return Ok(withProducts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id, [FromQuery] bool includeProducts = false)
        {
            IQueryable<Category> query = db.Categories.AsNoTracking();
            if (includeProducts)
            {
                query = query.Include(c => c.Products);
            }

            Category? category = await query.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            if (!includeProducts)
            {
                CategoryReadDto dto = new CategoryReadDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name
                };
                return Ok(dto);
            }

            CategoryWithProductsReadDto full = new CategoryWithProductsReadDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Products = category.Products.OrderBy(p => p.Name).ThenBy(p => p.ProductId).Select(p => new ProductListItemDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price
                }).ToList()
            };

            return Ok(full);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<CategoryReadDto>());
            string term = q.Trim();

            List<Category> items = await db.Categories
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.Name, "%" + term + "%"))
                .OrderBy(c => c.Name)
                .ToListAsync();

            List<CategoryReadDto> result = items.Select(c => new CategoryReadDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category input)
        {
            if (input == null) return BadRequest();

            Category entity = new Category
            {
                Name = input.Name?.Trim() ?? string.Empty
            };

            await db.Categories.AddAsync(entity);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.CategoryId }, new CategoryReadDto
            {
                CategoryId = entity.CategoryId,
                Name = entity.Name
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category input)
        {
            if (input == null || id != input.CategoryId) return BadRequest();

            Category? current = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (current == null) return NotFound();

            current.Name = input.Name?.Trim() ?? string.Empty;

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Category? current = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (current == null) return NotFound();

            db.Categories.Remove(current);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}