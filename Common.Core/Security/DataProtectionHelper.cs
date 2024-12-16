using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Common.Core.Security;
public class DataProtectionHelper
{
    private readonly IDataProtector _protector;
    private readonly IConfiguration _configuration;

    public DataProtectionHelper(IDataProtectionProvider provider, IConfiguration configuration)
    {
        _configuration = configuration;
        var protectorKey = _configuration["DataProtection:ProtectorKey"];
        _protector = provider.CreateProtector(protectorKey);
    }

    public byte[] EncryptToBytes(string plainText)
    {
        return _protector.Protect(Encoding.UTF8.GetBytes(plainText));
    }

    public string DecryptFromBytes(byte[] encryptedData)
    {
        return Encoding.UTF8.GetString(_protector.Unprotect(encryptedData));
    }

    public string Hash(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }

    public bool VerifyHash(string input, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(input, hash);
    }
}
