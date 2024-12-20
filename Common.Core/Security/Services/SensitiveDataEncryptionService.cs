using System.Security.Cryptography;
using System.Text;
using Common.Core.Security.Interfaces;

namespace Common.Core.Security.Services;

public class SensitiveDataEncryptionService : ISensitiveDataEncryptionService
{
    /// <summary>
    /// Encrypts the provided data using AES encryption and returns the encrypted byte array.
    /// </summary>
    /// <param name="key">The encryption key (must be 16, 24, or 32 bytes long).</param>
    /// <param name="data">The plaintext data to encrypt.</param>
    /// <returns>A byte array containing the IV followed by the encrypted data.</returns>
    public async Task<byte[]> EncryptToBytesAsync(string key, string data)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Encryption key cannot be null or empty.", nameof(key));
        if (string.IsNullOrWhiteSpace(data)) throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();
        var iv = aes.IV;

        using var memoryStream = new MemoryStream();

        // Write IV to the beginning of the stream
        await memoryStream.WriteAsync(iv);

        using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
        await using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        await using (var streamWriter = new StreamWriter(cryptoStream))
        {
            await streamWriter.WriteAsync(data);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Decrypts the provided byte array using AES encryption and returns the plaintext string.
    /// </summary>
    /// <param name="key">The decryption key (must match the key used for encryption).</param>
    /// <param name="encryptedData">The byte array containing the IV followed by the encrypted data.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public async Task<string> DecryptFromBytesAsync(string key, byte[] encryptedData)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Decryption key cannot be null or empty.", nameof(key));
        if (encryptedData == null || encryptedData.Length == 0) throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedData));

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);

        using var memoryStream = new MemoryStream(encryptedData);

        // Read IV from the beginning of the stream
        var iv = new byte[16];
        await memoryStream.ReadExactlyAsync(iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        await using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return await streamReader.ReadToEndAsync();
    }
}
