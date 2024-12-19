namespace Common.Notifications.Models;

public class EmailModel
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string Username { get; set; }
    public byte[] EncryptedPassword { get; set; } // Encrypted password
    public string PasswordHash { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string RecipientEmail { get; set; }
    public string Cc { get; set; }
    public string Subject { get; set; }
    public bool IsHtml { get; set; }
    public string Template { get; set; }
    public string Logo { get; set; }
    public string Body { get; set; }
    public List<FileModel> Files { get; set; }
}
