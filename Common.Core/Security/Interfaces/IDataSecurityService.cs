namespace Common.Core.Security.Interfaces;

public interface IDataSecurityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    (byte[] EncryptedPassword, string PasswordHash) SetPassword(string password); 
    string GetDecryptedPassword(byte[] encryptedPassword);
}
