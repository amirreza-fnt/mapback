namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// اطلاعات کاربر دریافتی از SSO سبزوار
/// </summary>
public class SSOUserInfo
{
    public string SSOUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    
    // ✅ فیلدهای جدید اضافه شده از SSO
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MelliCode { get; set; }
    public string? Address { get; set; }
    public bool IsManager { get; set; }
}