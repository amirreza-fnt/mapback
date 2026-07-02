namespace PayOnMap.API.Models;

public class LocationView
{
    public Guid UserId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}