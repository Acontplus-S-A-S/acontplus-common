namespace Common.Core.Security.Interfaces;

public interface IDataEncryptionService
{
    byte[] EncryptToBytes(string plainText);
    string DecryptFromBytes(byte[] encryptedData);
}
