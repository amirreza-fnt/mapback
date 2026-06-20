using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using PayOnMap.API.Repositories.Interfaces;

namespace PayOnMap.API.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SelectedLocation?> GetByIdAsync(Guid id)
    {
        return await _context.SelectedLocations
            .Include(sl => sl.User)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<IEnumerable<SelectedLocation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SelectedLocations
            .Where(sl => sl.UserId == userId)
            .OrderByDescending(sl => sl.IsDefault)
            .ThenByDescending(sl => sl.CreatedAt)
            .ToListAsync();
    }

    public async Task<SelectedLocation> CreateAsync(SelectedLocation location)
    {
        location.CreatedAt = DateTime.UtcNow;
        await _context.SelectedLocations.AddAsync(location);
        return location;
    }

    public async Task<SelectedLocation> UpdateAsync(SelectedLocation location)
    {
        _context.SelectedLocations.Update(location);
        return await Task.FromResult(location);
    }

    public async Task DeleteAsync(Guid id)
    {
        var location = await _context.SelectedLocations.FindAsync(id);
        if (location != null)
        {
            _context.SelectedLocations.Remove(location);
        }
    }

    public async Task SetAsDefaultAsync(Guid userId, Guid locationId)
    {
        // حذف حالت پیش‌فرض از همه مکان‌های کاربر
        var allLocations = await _context.SelectedLocations
            .Where(sl => sl.UserId == userId)
            .ToListAsync();

        foreach (var loc in allLocations)
        {
            loc.IsDefault = (loc.Id == locationId);
        }
    }
}