namespace Common.Core.Models;
public class Email
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
    public string DisplayName { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Cc { get; set; }
    public string Subject { get; set; }
    public string Template { get; set; }
    public string Logo { get; set; }
    public string Body { get; set; }
}
