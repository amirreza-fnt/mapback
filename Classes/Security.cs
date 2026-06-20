using System.Text;

namespace PayOnMap.API.Classes;

/// <summary>
/// کلاس کمکی برای تبدیل Base64
/// </summary>
public static class Security
{
    public static string EncodeToBase64(byte[] plainArr)
    {
        return Convert.ToBase64String(plainArr);
    }

    public static string EncodeToBase64(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        try
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static byte[] Base64Decode(string base64EncodedData)
    {
        return Convert.FromBase64String(base64EncodedData);
    }

    public static string DecodeFromBase64(string base64EncodedData)
    {
        if (string.IsNullOrEmpty(base64EncodedData)) return string.Empty;
        try
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}