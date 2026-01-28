using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DBContext db;
        private readonly ILogger<ProductsController> logger;

        public ProductsController(DBContext db, ILogger<ProductsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll([FromQuery] int? categoryId = null, [FromQuery] string? q = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6)
        {
            if (pageNumber < 1)
            {
                return BadRequest(new { message = "Page number must be greater than 1." });
            }

            if (pageSize < 1)
            {
                return BadRequest(new { message = "Page size must be greater than 1." });
            }

            if (pageSize > 20)
            {
                pageSize = 20;
            }

            IQueryable<Product> query = db.Products
                .AsNoTracking()
                .Include(p => p.Category);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Name, "%" + term + "%"));
            }

            int totalCount = await query.CountAsync();
            int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            int skip = (pageNumber - 1) * pageSize;

            List<ProductReadDto> result = await query
                .OrderBy(p => p.Name)
                .ThenBy(p => p.ProductId)
                .Skip(skip)
                .Take(pageSize)
                .ProjectToType<ProductReadDto>()
                .ToListAsync();

            foreach (ProductReadDto dto in result)
            {
                dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);
            }

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());
            Response.Headers.Append("X-Page-Number", pageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count,X-Total-Pages,X-Page-Number,X-Page-Size");

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            try
            {
                ProductReadDto? dto = await db.Products
                    .AsNoTracking()
                    .Where(p => p.ProductId == id)
                    .Select(p => new ProductReadDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                        ImageFileId = p.ImageFileId,
                        ImageUrl = p.ImageFile != null ? p.ImageFile.RelativePath : string.Empty
                    })
                    .FirstOrDefaultAsync();

                if (dto == null) return NotFound();

                dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to GET product by id {ProductId}. Path: {Path}, Query: {Query}", id, HttpContext?.Request?.Path.Value, HttpContext?.Request?.QueryString.Value);
                return Problem(title: "An unexpected error occurred while retrieving the product.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> Search([FromQuery] string q, [FromQuery] int? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<ProductReadDto>());
            string term = q.Trim();

            IQueryable<Product> query = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => EF.Functions.Like(p.Name, "%" + term + "%"));

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            List<ProductReadDto> result = await query.OrderBy(p => p.Name).ThenBy(p => p.ProductId).ProjectToType<ProductReadDto>().ToListAsync();

            foreach (ProductReadDto dto in result)
            {
                dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> Create([FromBody] ProductCreateDto input)
        {
            if (input == null) return BadRequest();

            bool categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Invalid CategoryId." });
            }

            if (input.ImageFileId.HasValue)
            {
                bool imageExists = await db.ImageFiles.AnyAsync(i => i.ImageFileId == input.ImageFileId.Value);
                if (!imageExists) return BadRequest(new { message = "Invalid ImageFileId." });
            }

            Product entity = input.Adapt<Product>();

            await db.Products.AddAsync(entity);
            await db.SaveChangesAsync();

            await db.Entry(entity).Reference(p => p.Category).LoadAsync();
            await db.Entry(entity).Reference(p => p.ImageFile).LoadAsync();

            ProductReadDto dto = entity.Adapt<ProductReadDto>();
            dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);

            return CreatedAtAction(nameof(GetById), new { id = entity.ProductId }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product input)
        {
            if (input == null || id != input.ProductId) return BadRequest();

            Product? current = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            if (current.CategoryId != input.CategoryId)
            {
                bool categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Invalid CategoryId." });
                }
            }

            if (current.ImageFileId != input.ImageFileId)
            {
                if (input.ImageFileId.HasValue)
                {
                    bool imageExists = await db.ImageFiles.AnyAsync(i => i.ImageFileId == input.ImageFileId.Value);
                    if (!imageExists) return BadRequest(new { message = "Invalid ImageFileId." });
                }
            }

            current.Name = input.Name?.Trim() ?? string.Empty;
            current.Price = input.Price;
            current.Description = input.Description?.Trim() ?? string.Empty;
            current.CategoryId = input.CategoryId;
            current.ImageFileId = input.ImageFileId;

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Product? current = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            db.Products.Remove(current);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private string MakeAbsoluteUrl(string virtualOrRelativePath)
        {
            if (string.IsNullOrWhiteSpace(virtualOrRelativePath)) return string.Empty;
            if (virtualOrRelativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return virtualOrRelativePath;

            string baseUrl = string.Concat(Request.Scheme, "://", Request.Host.ToUriComponent());
            string path = virtualOrRelativePath.StartsWith('/') ? virtualOrRelativePath : '/' + virtualOrRelativePath;
            return baseUrl + path;
        }
    }
}