namespace PayOnMap.API.DTOs.Login;

/// <summary>
/// اطلاعات کاربر دریافتی از SSO سبزوار
/// </summary>
public class SSOUserInfo
{
    public string SSOUserId { get; set; } = string.Empty;  // اصلاح شده: string به جای Guid
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Avatar { get; set; }
}