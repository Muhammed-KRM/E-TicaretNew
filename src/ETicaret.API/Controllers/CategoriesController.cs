using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ─── Public Endpoints (kimlik doğrulama gerekmez) ────────────────────────

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug)
    {
        var category = await _categoryService.GetBySlugAsync(slug);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpGet("{parentId:int}/subcategories")]
    public async Task<ActionResult<List<CategoryDto>>> GetSubCategories(int parentId)
    {
        var categories = await _categoryService.GetSubCategoriesAsync(parentId);
        return Ok(categories);
    }

    // ─── Admin Endpoints (sadece Admin rolü erişebilir) ──────────────────────

    /// <summary>Yeni kategori oluşturur. Yalnızca Admin yetkili kullanıcılar erişebilir.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kategori oluşturulurken bir hata oluştu.", detail = ex.Message });
        }
    }

    /// <summary>Mevcut kategoriyi günceller. Yalnızca Admin yetkili kullanıcılar erişebilir.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"ID={id} olan kategori bulunamadı." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kategori güncellenirken bir hata oluştu.", detail = ex.Message });
        }
    }

    /// <summary>Kategoriyi siler. Yalnızca Admin yetkili kullanıcılar erişebilir.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"ID={id} olan kategori bulunamadı." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Kategori silinirken bir hata oluştu.", detail = ex.Message });
        }
    }
}
