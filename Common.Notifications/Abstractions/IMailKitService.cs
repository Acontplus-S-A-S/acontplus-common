namespace Common.Notifications.Abstractions;

public interface IMailKitService
{
    Task<bool> SendAsync(EmailModel email, CancellationToken ct = default);
}
