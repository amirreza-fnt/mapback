// Controllers/LocationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PayOnMap.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
[Produces("application/json")]
public class LocationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LocationController> _logger;

    public LocationController(AppDbContext context, ILogger<LocationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLocations()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var locations = await _context.SelectedLocations
                .Where(l => l.UserId == userId.Value)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.LocationCode,
                    l.Address,
                    l.Title,
                    l.Latitude,
                    l.Longitude,
                    l.IsDefault,
                    l.CreatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = locations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting locations");
            return StatusCode(500, new { success = false, message = "خطا در دریافت مکان‌ها" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddLocation([FromBody] AddLocationRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // بررسی تکراری نبودن
            var exists = await _context.SelectedLocations
                .AnyAsync(l => l.UserId == userId.Value && l.LocationCode == request.LocationCode);

            if (exists)
                return BadRequest(new { success = false, message = "این مکان قبلاً اضافه شده است" });

            var location = new SelectedLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                LocationCode = request.LocationCode,
                Address = request.Address ?? "",
                Title = request.Title,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SelectedLocations.Add(location);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Location {Code} added for user {UserId}", request.LocationCode, userId);

            return Ok(new
            {
                success = true,
                message = "مکان با موفقیت اضافه شد",
                data = new
                {
                    location.Id,
                    location.LocationCode,
                    location.Address,
                    location.Title,
                    location.IsDefault,
                    location.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding location");
            return StatusCode(500, new { success = false, message = "خطا در ذخیره مکان" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var location = await _context.SelectedLocations
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId.Value);

            if (location == null)
                return NotFound(new { success = false, message = "مکان یافت نشد" });

            _context.SelectedLocations.Remove(location);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "مکان با موفقیت حذف شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location");
            return StatusCode(500, new { success = false, message = "خطا در حذف مکان" });
        }
    }

    [HttpGet("check/{locationCode}")]
    public async Task<IActionResult> CheckLocation(string locationCode)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var exists = await _context.SelectedLocations
                .AnyAsync(l => l.UserId == userId.Value && l.LocationCode == locationCode);

            return Ok(new { success = true, isSaved = exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking location");
            return StatusCode(500, new { success = false, message = "خطا" });
        }
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class AddLocationRequest
{
    public string LocationCode { get; set; } = "";
    public string? Address { get; set; }
    public string? Title { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}