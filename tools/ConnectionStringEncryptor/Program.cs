using System.Security.Cryptography;
using System.Text;

Console.Write("Paste your Neon PostgreSQL connection string (input is hidden): ");
var connectionString = ReadSecret();

Console.Write("Paste your encryption key (input is hidden): ");
var key = ReadSecret();

if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(key))
{
    Console.Error.WriteLine("Connection string and encryption key are required.");
    return 1;
}

var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
var nonce = RandomNumberGenerator.GetBytes(12);
var plaintext = Encoding.UTF8.GetBytes(connectionString);
var ciphertext = new byte[plaintext.Length];
var tag = new byte[16];

using var aes = new AesGcm(keyBytes, 16);
aes.Encrypt(nonce, plaintext, ciphertext, tag);

var encrypted = Convert.ToBase64String(nonce.Concat(ciphertext).Concat(tag).ToArray());

Console.WriteLine("\nEncrypted connection string:");
Console.WriteLine(encrypted);
Console.WriteLine("\nKeep this value private. Do not commit it to GitHub.");
return 0;

static string ReadSecret()
{
    var value = new StringBuilder();
    ConsoleKeyInfo keyInfo;

    do
    {
        keyInfo = Console.ReadKey(intercept: true);
        if (keyInfo.Key == ConsoleKey.Backspace && value.Length > 0)
        {
            value.Length--;
        }
        else if (keyInfo.Key != ConsoleKey.Enter)
        {
            value.Append(keyInfo.KeyChar);
        }
    } while (keyInfo.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return value.ToString();
}
