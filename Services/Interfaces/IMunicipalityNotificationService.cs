using PayOnMap.API.Models;

namespace PayOnMap.API.Services.Interfaces;

public interface IMunicipalityNotificationService
{
    Task<bool> NotifyPaymentAsync(Payment payment, User user);
}