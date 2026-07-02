using Microsoft.EntityFrameworkCore;
using PayOnMap.API.Data;
using PayOnMap.API.Models;
using PayOnMap.API.Repositories.Interfaces;

namespace PayOnMap.API.Repositories;

/// <summary>
/// ریپازیتوری پرداخت
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// دریافت پرداخت به همراه اطلاعات کاربر (برای ارسال به سرویس شهرداری)
    /// این متد چون قراره روی Entity برگشتی Tracking انجام بشه (برای UpdateStatusAsync بعدی)، AsNoTracking نمی‌زنیم
    /// </summary>
    public async Task<Payment?> GetByIdWithUserAsync(Guid id)
    {
        return await _context.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUserPaymentsCountAsync(Guid userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .CountAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, PaymentStatus status, long? paidAmount = null, string? refrenceCode = null)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return false;

        payment.Status = status;

        if (paidAmount.HasValue)
            payment.PaidAmount = paidAmount.Value;

        if (!string.IsNullOrEmpty(refrenceCode))
            payment.RefrenceCode = refrenceCode;

        if (status == PaymentStatus.Success)
            payment.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// ثبت وضعیت ارسال اطلاعات پرداخت به سرویس شهرداری
    /// </summary>
    public async Task<bool> SetNotifiedToMunicipalityAsync(Guid id, bool notified)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return false;

        payment.NotifiedToMunicipality = notified;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Payments.AnyAsync(p => p.Id == id);
    }
}