using System.Security.Cryptography;
using System.Text;

namespace Common.Notifications.Helpers;

public class AwsSesSmtpCredentialConverter
{
    public static (string smtpUsername, string smtpPassword) ConvertIamToSmtpCredentials(
        string iamAccessKey,
        string iamSecretKey,
        string region = "us-east-1")
    {
        // SMTP username is the same as the IAM access key
        string smtpUsername = iamAccessKey;

        // Constants as per AWS documentation
        string date = "11111111";
        string service = "ses";
        string terminal = "aws4_request";
        string message = "SendRawEmail";
        byte version = 0x04;

        // Step 1: Create the signature key following AWS algorithm
        byte[] kSecret = Encoding.UTF8.GetBytes("AWS4" + iamSecretKey);
        byte[] kDate = HmacSha256(date, kSecret);
        byte[] kRegion = HmacSha256(region, kDate);
        byte[] kService = HmacSha256(service, kRegion);
        byte[] kTerminal = HmacSha256(terminal, kService);
        byte[] kMessage = HmacSha256(message, kTerminal);

        // Step 2: Create signature and version - concatenate version byte with kMessage
        byte[] signatureAndVersion = new byte[kMessage.Length + 1];
        signatureAndVersion[0] = version;
        Array.Copy(kMessage, 0, signatureAndVersion, 1, kMessage.Length);

        // Step 3: Base64 encode the result to get SMTP password
        string smtpPassword = Convert.ToBase64String(signatureAndVersion);

        return (smtpUsername, smtpPassword);
    }

    private static byte[] HmacSha256(string data, byte[] key)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
