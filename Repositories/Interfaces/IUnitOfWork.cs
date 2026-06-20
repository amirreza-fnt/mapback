namespace PayOnMap.API.Repositories.Interfaces;

/// <summary>
/// واحد کار - مدیریت تراکنش‌ها
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPaymentRepository Payments { get; }
    ILocationRepository Locations { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// ذخیره تمام تغییرات
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// شروع تراکنش
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// کامیت تراکنش
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// رول‌بک تراکنش
    /// </summary>
    Task RollbackTransactionAsync();
}