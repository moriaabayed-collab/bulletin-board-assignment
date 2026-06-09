using BulletinBoardAPI.Models.Mappers;
using BulletinBoardAPI.Models.Requests;
using BulletinBoardAPI.Models.Responses;
using BulletinBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoardAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _advertisementService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<AdvertisementsController> _logger;

    public AdvertisementsController(IAdvertisementService advertisementService, ICategoryService categoryService, ILogger<AdvertisementsController> logger)
    {
        _advertisementService = advertisementService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<AdvertisementResponse>> GetAll([FromQuery] int? categoryId)
    {
        _logger.LogInformation("GET /api/advertisements - categoryId: {CategoryId}", categoryId);
        try
        {
            var ads = categoryId.HasValue
                ? _advertisementService.GetByCategory(categoryId.Value)
                : _advertisementService.GetAll();
            
            return Ok(ads.ToResponseList(_categoryService.GetAll()));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("GET /api/advertisements failed: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<AdvertisementResponse> GetById(int id)
    {
        _logger.LogInformation("GET /api/advertisements/{Id}", id);
        var ad = _advertisementService.GetById(id);
        if (ad == null)
        {
            _logger.LogWarning("GET /api/advertisements/{Id} - not found.", id);
            return NotFound($"Advertisement with ID {id} was not found.");
        }

        return Ok(ad.ToResponse(_categoryService.GetById(ad.CategoryId)));
    }

    [Authorize]
    [HttpPost]
    public ActionResult<AdvertisementResponse> Create([FromBody] AdvertisementRequest request)
    {
        _logger.LogInformation("POST /api/advertisements - title: '{Title}'", request.Title);
        try
        {
            var ad = request.ToAdvertisement();
            var created = _advertisementService.Create(ad);
            var category = _categoryService.GetById(created.CategoryId);
            var response = created.ToResponse(category);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("POST /api/advertisements failed: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public ActionResult<AdvertisementResponse> Update(int id, [FromBody] AdvertisementRequest request)
    {
        _logger.LogInformation("PUT /api/advertisements/{Id}", id);
        try
        {
            var ad = request.ToAdvertisement(id);
            var updated = _advertisementService.Update(id, ad);
            if (updated == null)
            {
                _logger.LogWarning("PUT /api/advertisements/{Id} - not found.", id);
                return NotFound($"Advertisement with ID {id} was not found.");
            }

            var category = _categoryService.GetById(updated.CategoryId);
            return Ok(updated.ToResponse(category));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("PUT /api/advertisements/{Id} failed: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("DELETE /api/advertisements/{Id}", id);
        if (!_advertisementService.Delete(id))
        {
            _logger.LogWarning("DELETE /api/advertisements/{Id} - not found.", id);
            return NotFound($"Advertisement with ID {id} was not found.");
        }

        return NoContent();
    }
}
