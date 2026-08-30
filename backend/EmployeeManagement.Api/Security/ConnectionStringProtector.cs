using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagement.Api.Security;

public static class ConnectionStringProtector
{
    public static string Decrypt(string encryptedValue, string key)
    {
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var payload = Convert.FromBase64String(encryptedValue);

        var nonce = payload[..12];
        var tag = payload[^16..];
        var ciphertext = payload[12..^16];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(keyBytes, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}
