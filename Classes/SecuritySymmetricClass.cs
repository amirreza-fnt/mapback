using System.Security.Cryptography;
using System.Text;

namespace PayOnMap.API.Classes;

/// <summary>
/// کلاس رمزنگاری و رمزگشایی متقارن - مطابق با الگوریتم درگاه پرداخت
/// </summary>
public class SecuritySymmetricClass
{
    public enum AlgorithmEnum
    {
        DES,
        TripleDES,
        RivetsCipher2,
        RijndaelAES
    }

    private byte[] _iv;
    private string _key;
    private AlgorithmEnum _algorithm;

    public SecuritySymmetricClass(string key, byte[] iv, AlgorithmEnum algorithm)
    {
        _iv = iv;
        _key = key;
        _algorithm = algorithm;
    }

    /// <summary>
    /// رمزنگاری داده
    /// </summary>
    public byte[] EncryptData(string data)
    {
        byte[] salt = _iv;
        Rfc2898DeriveBytes rfc;
        SymmetricAlgorithm alg;
        ICryptoTransform encryptor;
        byte[] key;
        byte[] IV;

        switch (_algorithm)
        {
            case AlgorithmEnum.DES:
                alg = DES.Create();
                break;
            case AlgorithmEnum.TripleDES:
                alg = TripleDES.Create();
                break;
            case AlgorithmEnum.RivetsCipher2:
                alg = RC2.Create();
                break;
            case AlgorithmEnum.RijndaelAES:
                alg = Aes.Create();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_algorithm), "الگوریتم انتخاب شده خارج از محدوده است");
        }

        rfc = new Rfc2898DeriveBytes(_key, salt, 1000, HashAlgorithmName.SHA1);
        key = rfc.GetBytes(alg.KeySize / 8);
        IV = rfc.GetBytes(alg.BlockSize / 8);
        alg.Key = key;
        alg.IV = IV;
        encryptor = alg.CreateEncryptor(alg.Key, alg.IV);

        using var mem = new MemoryStream();
        using (var crypto = new CryptoStream(mem, encryptor, CryptoStreamMode.Write))
        {
            byte[] pt = Encoding.UTF8.GetBytes(data);
            crypto.Write(pt, 0, pt.Length);
        }
        alg.Clear();
        return mem.ToArray();
    }

    /// <summary>
    /// رمزگشایی داده
    /// </summary>
    public byte[]? DecryptData(byte[] encryptedData)
    {
        if (encryptedData == null) return null;

        byte[] salt = _iv;
        Rfc2898DeriveBytes rfc;
        SymmetricAlgorithm alg;
        ICryptoTransform decryptor;
        byte[] key;
        byte[] IV;

        switch (_algorithm)
        {
            case AlgorithmEnum.DES:
                alg = DES.Create();
                break;
            case AlgorithmEnum.TripleDES:
                alg = TripleDES.Create();
                break;
            case AlgorithmEnum.RivetsCipher2:
                alg = RC2.Create();
                break;
            case AlgorithmEnum.RijndaelAES:
                alg = Aes.Create();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_algorithm));
        }

        rfc = new Rfc2898DeriveBytes(_key, salt, 1000, HashAlgorithmName.SHA1);
        key = rfc.GetBytes(alg.KeySize / 8);
        IV = rfc.GetBytes(alg.BlockSize / 8);
        alg.Key = key;
        alg.IV = IV;
        decryptor = alg.CreateDecryptor(alg.Key, alg.IV);

        using var mem = new MemoryStream();
        using (var crypto = new CryptoStream(mem, decryptor, CryptoStreamMode.Write))
        {
            crypto.Write(encryptedData, 0, encryptedData.Length);
        }
        alg.Clear();
        return mem.ToArray();
    }

    /// <summary>
    /// تبدیل آرایه به Base64
    /// </summary>
    public string ToBase64String(byte[] dataBytes)
    {
        return Convert.ToBase64String(dataBytes);
    }

    /// <summary>
    /// تبدیل Base64 به آرایه
    /// </summary>
    public byte[]? FromBase64String(string dataBytes)
    {
        if (string.IsNullOrEmpty(dataBytes)) return null;
        return Convert.FromBase64String(dataBytes);
    }
}