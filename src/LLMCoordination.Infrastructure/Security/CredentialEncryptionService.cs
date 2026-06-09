using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LLMCoordination.Infrastructure.Security;

public class CredentialEncryptionService
{
    private const int IvSizeBytes = 16;
    private readonly byte[] _key;

    public CredentialEncryptionService(IConfiguration configuration)
    {
        var devKey = configuration["Encryption:DevKey"]
            ?? throw new InvalidOperationException("Encryption:DevKey is not configured.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(devKey));
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = aes.EncryptCbc(plainBytes, aes.IV, PaddingMode.PKCS7);

        var payload = new byte[IvSizeBytes + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, IvSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, payload, IvSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length <= IvSizeBytes)
        {
            throw new CryptographicException("Invalid encrypted payload.");
        }

        var iv = payload.AsSpan(0, IvSizeBytes);
        var cipherBytes = payload.AsSpan(IvSizeBytes);

        using var aes = Aes.Create();
        aes.Key = _key;

        var plainBytes = aes.DecryptCbc(cipherBytes, iv, PaddingMode.PKCS7);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
