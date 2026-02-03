using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GUIWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Mapster;
using GUIWebApi.Tools;
using System.Linq.Expressions;

namespace GUIWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class MyMapsterBaseController<TController, TContext> : ControllerBase
    where TContext : DbContext
    {
        protected readonly TContext _db;
        protected readonly ILogger<TController> _logger;

        protected MyMapsterBaseController(TContext db, ILogger<TController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // --- LÆSNING (GET) ---

        /// GetAll med valgfri tracking
        protected async Task<ActionResult<IEnumerable<TDto>>> ProjectListAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            bool useTracking = false) where TEntity : class
        {
            if (!useTracking) query = query.AsNoTracking();

            var list = await query.ProjectToType<TDto>().ToListAsync();
            return Ok(list);
        }

        // Når man bruger Mapsters ProjectToType kan der teknisk set ikke
        // anvendes tracking.
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            int page = 1,
            int pageSize = 10,
            string orderBy = "Id",
            bool descending = false) where TEntity : class
        {
            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(orderBy))
            {
                query = descending
                    ? query.OrderByDescending(x => EF.Property<object>(x, orderBy))
                    : query.OrderBy(x => EF.Property<object>(x, orderBy));
            }

            var items = await query.AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectToType<TDto>()
                .ToListAsync();

            return Ok(new PagedResult<TDto> { Items = items, TotalCount = totalCount, PageSize = pageSize, CurrentPage = page });
        }

        // Get Single
        protected async Task<ActionResult<TDto>> ProjectSingleAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            bool useTracking = false) where TEntity : class
        {
            if (!useTracking) query = query.AsNoTracking();

            var item = await query.ProjectToType<TDto>().FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        /// <summary>
        /// Henter en liste baseret på et filter (f.eks. "ForeignKey ID")
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> GetFilteredAsync<TEntity, TDto>(
            Expression<Func<TEntity, bool>> predicate,
            bool useTracking = false) where TEntity : class
        {
            IQueryable<TEntity> query = _db.Set<TEntity>().Where(predicate);

            if (!useTracking) query = query.AsNoTracking();

            var list = await query.ProjectToType<TDto>().ToListAsync();
            return Ok(list);
        }

        // --- DB HANDLINGER & FEJL (POST, PUT, DELETE) ---

        // Create (POST)
        protected async Task<ActionResult<TDto>> CreateAsync<TEntity, TCreateDto, TDto>(TCreateDto dto)
            where TEntity : class
        {
            var entity = dto.Adapt<TEntity>();
            _db.Set<TEntity>().Add(entity);
            await _db.SaveChangesAsync();

            // Map tilbage til DTO for at returnere f.eks. det nye ID
            var resultDto = entity.Adapt<TDto>();
            return CreatedAtAction(null, resultDto);
        }

        // Update (PUT)
        protected async Task<IActionResult> UpdateAsync<TEntity, TUpdateDto>(int id, TUpdateDto dto)
            where TEntity : class
        {
            var entity = await _db.Set<TEntity>().FindAsync(id);
            if (entity == null) return NotFound();

            // Mapster opdaterer eksisterende entity med værdier fra DTO
            dto.Adapt(entity);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Delete (DELETE)
        protected async Task<IActionResult> DeleteAsync<TEntity>(int id) where TEntity : class
        {
            var entity = await _db.Set<TEntity>().FindAsync(id);
            if (entity == null) return NotFound();

            //_db.Set<TEntity>().Remove(entity);
            //await _db.SaveChangesAsync();
            //return NoContent();

            return await ExecuteDbActionAsync(async () =>
            {
                _db.Set<TEntity>().Remove(entity);
                await _db.SaveChangesAsync();
            });
        }

        protected async Task<IActionResult> ExecuteDbActionAsync(Func<Task> action)
        {
            try
            {
                await action();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Databasefejl");
                if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                    return Conflict("Handlingen kunne ikke gennemføres, da data er i brug.");
                return BadRequest("Fejl ved opdatering af databasen.");
            }
        }

        // --- FIL UPLOAD HJÆLPER ---

        protected async Task<byte[]> ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
