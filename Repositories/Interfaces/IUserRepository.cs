using PayOnMap.API.Models;

namespace PayOnMap.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetBySSOUserIdAsync(string ssoUserId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10);
}