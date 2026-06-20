using PayOnMap.API.Models;

namespace PayOnMap.API.Repositories.Interfaces;

public interface ILocationRepository
{
    Task<SelectedLocation?> GetByIdAsync(Guid id);
    Task<IEnumerable<SelectedLocation>> GetByUserIdAsync(Guid userId);
    Task<SelectedLocation> CreateAsync(SelectedLocation location);
    Task<SelectedLocation> UpdateAsync(SelectedLocation location);
    Task DeleteAsync(Guid id);
    Task SetAsDefaultAsync(Guid userId, Guid locationId);
}