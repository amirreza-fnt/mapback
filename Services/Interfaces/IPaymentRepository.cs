using PayOnMap.API.Models;

namespace PayOnMap.API.Repositories.Interfaces;

/// <summary>
/// اینترفیس ریپازیتوری پرداخت
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// ایجاد پرداخت جدید
    /// </summary>
    Task<Payment> CreateAsync(Payment payment);

    /// <summary>
    /// دریافت پرداخت بر اساس شناسه
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid id);

    /// <summary>
    /// دریافت پرداخت‌های یک کاربر
    /// </summary>
    Task<List<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// به‌روزرسانی وضعیت پرداخت
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid id, PaymentStatus status, long? paidAmount = null, string? refrenceCode = null);

    /// <summary>
    /// بررسی وجود پرداخت با شناسه خاص
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// دریافت تعداد کل پرداخت‌های کاربر
    /// </summary>
    Task<int> GetUserPaymentsCountAsync(Guid userId);
}