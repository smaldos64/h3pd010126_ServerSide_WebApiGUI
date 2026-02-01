using GUIWebApi.Models.DTOs;
using GUIWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Categories1Controller : ControllerBase
    {
        private readonly DBContext db;

        public Categories1Controller(DBContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category1>>> GetAll([FromQuery] bool includeProducts = false, [FromQuery] string? q = null)
        {
            IQueryable<Category1> query = db.Categories1.AsNoTracking();

            if (includeProducts)
                query = query.Include(c => c.Products);

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Name, "%" + term + "%"));
            }

            List<Category1> items = await query.OrderBy(c => c.Name).ToListAsync();

            if (!includeProducts)
            {
                List<CategoryReadDto> flat = items.Select(c => new CategoryReadDto
                {
                    CategoryId = c.Category1Id,
                    Name = c.Name
                }).ToList();

                return Ok(flat);
            }

            List<CategoryWithProductsReadDto> withProducts = items.Select(c => new CategoryWithProductsReadDto
            {
                CategoryId = c.Category1Id,
                Name = c.Name,
                Products = c.Products.OrderBy(p => p.Name).ThenBy(p => p.Product1Id).Select(p => new ProductListItemDto
                {
                    ProductId = p.Product1Id,
                    Name = p.Name,
                    Price = p.Price
                }).ToList()
            }).ToList();

            return Ok(withProducts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id, [FromQuery] bool includeProducts = false)
        {
            IQueryable<Category1> query = db.Categories1.AsNoTracking();
            if (includeProducts)
            {
                query = query.Include(c => c.Products);
            }

            Category1? category = await query.FirstOrDefaultAsync(c => c.Category1Id == id);
            if (category == null) return NotFound();

            if (!includeProducts)
            {
                CategoryReadDto dto = new CategoryReadDto
                {
                    CategoryId = category.Category1Id,
                    Name = category.Name
                };
                return Ok(dto);
            }

            CategoryWithProductsReadDto full = new CategoryWithProductsReadDto
            {
                CategoryId = category.Category1Id,
                Name = category.Name,
                Products = category.Products.OrderBy(p => p.Name).ThenBy(p => p.Product1Id).Select(p => new ProductListItemDto
                {
                    ProductId = p.Product1Id,
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

            List<Category1> items = await db.Categories1
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.Name, "%" + term + "%"))
                .OrderBy(c => c.Name)
                .ToListAsync();

            List<CategoryReadDto> result = items.Select(c => new CategoryReadDto
            {
                CategoryId = c.Category1Id,
                Name = c.Name
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category1 input)
        {
            if (input == null) return BadRequest();

            Category1 entity = new Category1
            {
                Name = input.Name?.Trim() ?? string.Empty
            };

            await db.Categories1.AddAsync(entity);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Category1Id }, new CategoryReadDto
            {
                CategoryId = entity.Category1Id,
                Name = entity.Name
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category1 input)
        {
            if (input == null || id != input.Category1Id) return BadRequest();

            Category1? current = await db.Categories1.FirstOrDefaultAsync(c => c.Category1Id == id);
            if (current == null) return NotFound();

            current.Name = input.Name?.Trim() ?? string.Empty;

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Category1? current = await db.Categories1.FirstOrDefaultAsync(c => c.Category1Id == id);
            if (current == null) return NotFound();

            db.Categories1.Remove(current);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
