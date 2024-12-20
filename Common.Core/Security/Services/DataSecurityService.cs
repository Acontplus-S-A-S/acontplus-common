using Common.Core.Security.Interfaces;

namespace Common.Core.Security.Services;

public class DataSecurityService(IDataEncryptionService dataEncryptionService) : IDataSecurityService
{
    public string GetDecryptedPassword(byte[] encryptedPassword)
    {
        return dataEncryptionService.DecryptFromBytes(encryptedPassword);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public (byte[] EncryptedPassword, string PasswordHash) SetPassword(string password)
    {
        var encryptedPassword = dataEncryptionService.EncryptToBytes(password);
        var passwordHash = HashPassword(password);
        return (encryptedPassword, passwordHash);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
