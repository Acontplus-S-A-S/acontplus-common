using Common.Core.Security;

namespace Common.Notifications.Entities;

[Table("EmailSenderConfig", Schema = "Config")]
public class EmailSenderConfig : BaseEntity
{
    // Scope of the configuration
    public int? CompanyId { get; set; } // Nullable: NULL for global configuration
    public bool IsGlobal { get; set; } // True for global settings, false for company-specific

    // Email configuration
    [Required, MaxLength(150)] public string SenderEmail { get; set; } // Example: no-reply@example.com
    [Required, MaxLength(300)] public string SenderName { get; set; } // Example: ERP Notifications
    [Required, MaxLength(150)] public string SmtpServer { get; set; } // Example: smtp.example.com
    public int SmtpPort { get; set; } // Example: 587
    public bool UseSsl { get; set; } // True if SSL/TLS is required
    [Required, MaxLength(150)] public string Username { get; set; } // SMTP authentication username
    [Required] public byte[] EncryptedPassword { get; set; } // Encrypted password
    public string PasswordHash { get; set; }

    public void SetPassword(string password, DataProtectionHelper securityHelper)
    {
        EncryptedPassword = securityHelper.EncryptToBytes(password);
        PasswordHash = securityHelper.Hash(password);
    }

    public string GetDecryptedPassword(DataProtectionHelper securityHelper)
    {
        return securityHelper.DecryptFromBytes(EncryptedPassword);
    }
    public bool VerifyPassword(string password, DataProtectionHelper securityHelper)
    {
        return securityHelper.VerifyHash(password, PasswordHash);
    }
}
