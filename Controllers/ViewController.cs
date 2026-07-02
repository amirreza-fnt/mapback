using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using System.Security.Claims;

namespace PayOnMap.API.Controllers;

[ApiController]
[Route("api/view")]
[Authorize]
[Produces("application/json")]
public class ViewController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ViewController> _logger;

    public ViewController(AppDbContext context, ILogger<ViewController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SaveLocationView([FromBody] ViewLocationRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.LocationCode))
                return BadRequest(new { success = false, message = "کد مکان الزامی است" });

            var locationView = new LocationView
            {
                UserId = userId.Value,
                LocationCode = request.LocationCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.LocationViews.Add(locationView);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Location {Code} viewed by user {UserId}", 
                request.LocationCode, userId);

            return Ok(new { success = true, message = "مکان با موفقیت ذخیره شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving location view");
            return StatusCode(500, new { success = false, message = "خطا در ذخیره اطلاعات" });
        }
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class ViewLocationRequest
{
    public string LocationCode { get; set; } = string.Empty;
}