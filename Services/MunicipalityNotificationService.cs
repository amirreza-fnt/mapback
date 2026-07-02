using System.Text;
using System.Text.Json;
using PayOnMap.API.DTOs.Municipality;
using PayOnMap.API.Models;
using PayOnMap.API.Services.Interfaces;

namespace PayOnMap.API.Services;

public class MunicipalityNotificationService : IMunicipalityNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MunicipalityNotificationService> _logger;

    public MunicipalityNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MunicipalityNotificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> NotifyPaymentAsync(Payment payment, User user)
    {
        try
        {
            var dto = new MunicipalityNotificationDto
            {
                // ✅ محافظت کامل از همه فیلدهای string در برابر null
                SHGH = payment.BillId ?? "",
                SHP = payment.PaymentId ?? "",
                Amount = payment.PaidAmount ?? payment.Amount,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                NosaziCode = payment.LocationCode ?? "",
                MelliCode = user.MelliCode ?? "",
                PaymentType = (int)payment.ChargeType,
                Mobile = user.Phone ?? "",
                PayTime = payment.PaidAt ?? DateTime.UtcNow,
                BankGateway = payment.PayGateway?.ToString() ?? "",
                
                // ✅ اصلاح خطای BankTrackingCode:
                // اگر TrackingCode خالی بود، از RefrenceCode استفاده کن، وگرنه رشته خالی
                BankTrackingCode = payment.TrackingCode ?? payment.RefrenceCode ?? "",
                
                ServerTrackingCode = payment.RefrenceCode ?? ""
            };

            var json = JsonSerializer.Serialize(dto);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            var baseUrl = _configuration["Municipality:RegPaidBillUrl"]
                ?? "https://regpaidbill.sabzevar.ir/RegPaidBillFromMap.ashx";

            var url = $"{baseUrl}?data={Uri.EscapeDataString(base64)}";

            var response = await _httpClient.GetAsync(url);
            var responseText = await response.Content.ReadAsStringAsync();

            var firstLine = responseText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];
            var ok = bool.TryParse(firstLine, out var parsed) && parsed;

            if (!ok)
            {
                _logger.LogWarning(
                    "Municipality notification failed for PaymentId={PaymentId}. Response={Response}",
                    payment.Id, responseText);
            }
            else
            {
                _logger.LogInformation(
                    "Municipality notified successfully for PaymentId={PaymentId}", payment.Id);
            }

            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error notifying municipality for PaymentId={PaymentId}", payment.Id);
            return false;
        }
    }
}