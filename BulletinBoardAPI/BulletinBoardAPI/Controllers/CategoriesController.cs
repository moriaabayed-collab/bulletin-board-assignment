using BulletinBoardAPI.Models;
using BulletinBoardAPI.Models.Requests;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoardAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<Category>> GetAll()
    {
        _logger.LogInformation("GET /api/categories");
        return Ok(_categoryService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Category> GetById(int id)
    {
        _logger.LogInformation("GET /api/categories/{Id}", id);
        var category = _categoryService.GetById(id);
        if (category == null)
        {
            _logger.LogWarning("GET /api/categories/{Id} - not found.", id);
            return NotFound($"Category with ID {id} was not found.");
        }

        return Ok(category);
    }

    [Authorize]
    [HttpPost]
    public ActionResult<Category> Create([FromBody] CategoryRequest request)
    {
        _logger.LogInformation("POST /api/categories - name: '{Name}'", request.Name);
        try
        {
            var created = _categoryService.Create(request.Name);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("POST /api/categories failed: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public ActionResult<Category> Update(int id, [FromBody] CategoryRequest request)
    {
        _logger.LogInformation("PUT /api/categories/{Id}", id);
        try
        {
            var updated = _categoryService.Update(id, request.Name);
            if (updated == null)
            {
                _logger.LogWarning("PUT /api/categories/{Id} - not found.", id);
                return NotFound($"Category with ID {id} was not found.");
            }

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("PUT /api/categories/{Id} failed: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("DELETE /api/categories/{Id}", id);
        if (!_categoryService.Delete(id))
        {
            _logger.LogWarning("DELETE /api/categories/{Id} - not found.", id);
            return NotFound($"Category with ID {id} was not found.");
        }

        return NoContent();
    }
}
