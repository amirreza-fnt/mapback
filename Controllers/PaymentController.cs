using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOnMap.API.Classes;
using PayOnMap.API.DTOs.Payment;
using PayOnMap.API.Models;
using PayOnMap.API.Repositories.Interfaces;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PayOnMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// ایجاد پرداخت جدید (نیاز به احراز هویت)
    /// </summary>
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { success = false, message = "کاربر یافت نشد" });

            if (request.Amount <= 0)
                return BadRequest(new { success = false, message = "مبلغ نامعتبر است" });

            if (string.IsNullOrWhiteSpace(request.LocationCode))
                return BadRequest(new { success = false, message = "کد نوسازی الزامی است" });

            // ایجاد شناسه سفارش
            var orderId = Guid.NewGuid();

            var payment = new Payment
            {
                Id = orderId,
                UserId = userId.Value,
                LocationCode = request.LocationCode,
                Title = request.Title ?? "پرداخت عوارض",
                BillId = request.BillId,
                PaymentId = request.PaymentId,
                Amount = request.Amount,
                Description = request.Description ?? $"پرداخت عوارض ملک {request.LocationCode}",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(30) // 30 دقیقه اعتبار
            };

            await _paymentRepository.CreateAsync(payment);

            var appId = _configuration["PaymentService:AppId"];
            var paymentUrl = $"https://pay.sabzevar.ir/{appId}/{orderId}";

            _logger.LogInformation("Payment created: OrderId={OrderId}, UserId={UserId}, Amount={Amount}",
                orderId, userId, request.Amount);

            return Ok(new CreatePaymentResponse
            {
                Success = true,
                Message = "سفارش با موفقیت ایجاد شد",
                OrderId = orderId,
                PaymentUrl = paymentUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new CreatePaymentResponse
            {
                Success = false,
                Message = "خطا در ایجاد سفارش پرداخت"
            });
        }
    }

    /// <summary>
    /// استعلام اطلاعات پرداخت توسط درگاه (GET /Check/{orderId})
    /// </summary>
    [HttpGet("/Check/{orderId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Check(Guid orderId)
    {
        try
        {
            var appIdString = Guid.Parse(_configuration["PaymentService:AppId"] ?? 
                throw new InvalidOperationException("AppId not configured"));

            var payment = await _paymentRepository.GetByIdAsync(orderId);

            if (payment != null && payment.ExpiredAt >= DateTime.UtcNow && payment.Status == PaymentStatus.Pending)
            {
                var paymentInfo = new FetchPaymentInfoStruct
                {
                    AppID = appIdString,
                    OrderID = payment.Id.ToString(),
                    Price = payment.Amount,
                    Description = payment.Description ?? "پرداخت عوارض از طریق سامانه پرداخت روی نقشه",
                    TimeRequestUTC = DateTime.UtcNow
                };
                return Ok(paymentInfo);
            }

            return Ok(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Check endpoint for OrderId={OrderId}", orderId);
            return Ok(null);
        }
    }

    /// <summary>
    /// دریافت نتیجه پرداخت از درگاه (POST /Save)
    /// </summary>
    [HttpPost("/Save")]
    [AllowAnonymous]
    public async Task<IActionResult> Save()
    {
        try
        {
            string data;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(data))
                return Ok(0);

            var appIdString = Guid.Parse(_configuration["PaymentService:AppId"] ?? 
                throw new InvalidOperationException("AppId not configured"));
            var secKeyString = _configuration["PaymentService:SecurityKey"] ?? 
                throw new InvalidOperationException("SecurityKey not configured");

            var decodeB64Data = Security.Base64Decode(data);
            var deserialData = JsonSerializer.Deserialize<PostEncryptedInfoStruct>(decodeB64Data);

            if (deserialData != null && (DateTime.UtcNow - deserialData.DateCreatedUTC) <= TimeSpan.FromMinutes(10))
            {
                var secClass = new SecuritySymmetricClass(secKeyString, deserialData.IvKey, 
                    SecuritySymmetricClass.AlgorithmEnum.RijndaelAES);
                var dataDec = secClass.DecryptData(deserialData.DataEncripted);

                if (dataDec != null && dataDec.Length > 0)
                {
                    var rawData = Encoding.UTF8.GetString(dataDec);
                    var paymentData = JsonSerializer.Deserialize<PostPaymentInfoStruct>(rawData);

                    if (paymentData != null &&
                        paymentData.AppID == appIdString &&
                        Guid.TryParse(paymentData.OrderID, out Guid orderId))
                    {
                        var payment = await _paymentRepository.GetByIdAsync(orderId);

                        if (payment != null && payment.ExpiredAt >= DateTime.UtcNow && payment.Status == PaymentStatus.Pending)
                        {
                            // به‌روزرسانی اطلاعات پرداخت
                            payment.Description = paymentData.Description;
                            payment.RefrenceCode = paymentData.PayRefrenceCode;
                            payment.PayGateway = paymentData.PayGateway;
                            payment.PaidAmount = paymentData.PayPrice;
                            payment.PaidAt = paymentData.PayTimeUTC;
                            payment.Status = PaymentStatus.Success;

                            await _paymentRepository.UpdateStatusAsync(
                                payment.Id,
                                PaymentStatus.Success,
                                paymentData.PayPrice,
                                paymentData.PayRefrenceCode
                            );

                            _logger.LogInformation("Payment successful: OrderId={OrderId}, RefCode={RefCode}",
                                orderId, paymentData.PayRefrenceCode);

                            return Ok(paymentData.ServerOrderID);
                        }
                    }
                }
            }

            return Ok(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Save endpoint");
            return Ok(0);
        }
    }

    /// <summary>
    /// دریافت تاریخچه پرداخت‌های کاربر (نیاز به احراز هویت)
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { success = false, message = "کاربر یافت نشد" });

            var payments = await _paymentRepository.GetUserPaymentsAsync(userId.Value, page, pageSize);
            var totalCount = await _paymentRepository.GetUserPaymentsCountAsync(userId.Value);

            var historyItems = payments.Select((p, index) => new PaymentHistoryItemDto
            {
                Id = ((page - 1) * pageSize) + index + 1,
                TrackingCode = p.RefrenceCode ?? "-",
                AuthCode = p.RefrenceCode ?? "-",
                BillId = p.BillId ?? "-",
                PaymentId = p.PaymentId ?? "-",
                Amount = p.Amount.ToString("N0") + " ریال",
                Date = p.CreatedAt.ToLocalTime().ToString("yyyy/MM/dd"),
                Time = p.CreatedAt.ToLocalTime().ToString("HH:mm"),
                Status = GetStatusText(p.Status),
                LocationCode = p.LocationCode,
                Title = p.Title
            }).ToList();

            return Ok(new
            {
                success = true,
                data = historyItems,
                totalCount,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history");
            return StatusCode(500, new { success = false, message = "خطا در دریافت تاریخچه پرداخت‌ها" });
        }
    }

    #region Private Methods

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private string GetStatusText(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Success => "موفق",
            PaymentStatus.Failed => "ناموفق",
            PaymentStatus.Pending => "در انتظار",
            PaymentStatus.Expired => "منقضی شده",
            PaymentStatus.Cancelled => "لغو شده",
            _ => "نامشخص"
        };
    }

    #endregion
}