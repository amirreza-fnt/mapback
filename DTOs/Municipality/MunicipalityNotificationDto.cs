using System.Text.Json.Serialization;

namespace PayOnMap.API.DTOs.Municipality;

public class MunicipalityNotificationDto
{
    [JsonPropertyName("shgh")]
    public string? SHGH { get; set; }

    [JsonPropertyName("shp")]
    public string? SHP { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("nosaziCode")]
    public string? NosaziCode { get; set; }

    [JsonPropertyName("melliCode")]
    public string? MelliCode { get; set; }

    [JsonPropertyName("paymentType")]
    public int PaymentType { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("payTime")]
    public DateTime PayTime { get; set; }

    [JsonPropertyName("bankGateway")]
    public string? BankGateway { get; set; }

    [JsonPropertyName("bankTrackingCode")]
    public string? BankTrackingCode { get; set; }

    [JsonPropertyName("serverTrackingCode")]
    public string? ServerTrackingCode { get; set; }
}