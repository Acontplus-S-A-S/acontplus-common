namespace Common.Core.Security.Interfaces;

public interface ISensitiveDataEncryptionService
{
    string EncryptString(string key, string data);
    string DecryptString(string key, string cipherText);
}
