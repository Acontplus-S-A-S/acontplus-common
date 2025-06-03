using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.Mime;
using System.Security.Authentication;
using Common.Notifications.Abstractions;
using Common.Notifications.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Utils;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Scriban;
using Scriban.Runtime;

namespace Common.Notifications.Services;

public class MailKitService : IMailKitService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    // A simple pool for SmtpClient instances
    // For very high concurrency, a more sophisticated pool (e.g., using Channel<T>) might be needed.
    private readonly ConcurrentBag<SmtpClient> _smtpClientPool;
    private readonly int _maxPoolSize;

    public MailKitService(IConfiguration configuration, ILogger<MailKitService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Configure retry policy: 3 attempts with exponential backoff (e.g., 1s, 2s, 4s)
        _retryPolicy = Policy
            // Handle SmtpProtocolException and check its message for "too many login attempts"
            .Handle<SmtpProtocolException>(ex =>
            {
                _logger.LogDebug(ex, "Caught SmtpProtocolException: {Message}", ex.Message);
                return ex.Message.Contains("too many login attempts", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("Service not available", StringComparison.OrdinalIgnoreCase);
            })
            // Handle SmtpCommandException and check its status code or message
            .Or<SmtpCommandException>(ex =>
            {
                _logger.LogDebug(ex, "Caught SmtpCommandException with StatusCode {StatusCode}: {Message}", ex.StatusCode, ex.Message);
                // SMTP status codes in 4xx range are transient (e.g., 451 Requested action aborted: local error in processing)
                // 4.7.0 is a common enhanced status code for too many login attempts from some servers like Gmail.
                // We can also check the message for specific text.
                return ((int)ex.StatusCode >= 400 && (int)ex.StatusCode < 500) ||
                       ex.Message.Contains("too many login attempts", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("Service not available", StringComparison.OrdinalIgnoreCase);
            })
            // Handle AuthenticationException if you suspect that authentication itself might be momentarily flaky
            .Or<MailKit.Security.AuthenticationException>()
            // Handle general network socket errors
            .Or<System.Net.Sockets.SocketException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Attempt {RetryCount} failed to send email. Retrying in {TimeSpan}...", retryCount, timeSpan);
                });

        _maxPoolSize = _configuration.GetValue<int>("MailKit:MaxPoolSize", 5);
        _smtpClientPool = new ConcurrentBag<SmtpClient>();
    }

    private async Task<SmtpClient> GetConnectedSmtpClientAsync(EmailModel email, CancellationToken ct)
    {
        if (_smtpClientPool.TryTake(out var client))
        {
            if (client.IsConnected && client.IsAuthenticated)
            {
                _logger.LogDebug("Reusing existing SMTP client from pool.");
                return client;
            }
            else
            {
                _logger.LogDebug("SMTP client from pool was disconnected or unauthenticated. Disposing and creating new.");
                client.Dispose(); // Dispose of the stale client
            }
        }

        // Create a new client if pool is empty or client was stale
        var newClient = new SmtpClient();
        newClient.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        newClient.CheckCertificateRevocation = false;

        _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}...", email.SmtpServer, email.SmtpPort);
        // Use SecureSocketOptions.Auto to let MailKit determine the best option
        await newClient.ConnectAsync(email.SmtpServer, email.SmtpPort, MailKit.Security.SecureSocketOptions.Auto, ct);
        _logger.LogInformation("Authenticating with SMTP server for {SenderEmail}...", email.SenderEmail);
        await newClient.AuthenticateAsync(email.SenderEmail, email.Password, ct);
        _logger.LogInformation("Successfully connected and authenticated to SMTP server.");
        return newClient;
    }

    private void ReturnSmtpClientToPool(SmtpClient client)
    {
        if (_smtpClientPool.Count < _maxPoolSize)
        {
            _smtpClientPool.Add(client);
            _logger.LogDebug("Returned SMTP client to pool. Current pool size: {PoolSize}", _smtpClientPool.Count);
        }
        else
        {
            _logger.LogDebug("SMTP client pool is full. Disposing client.");
            client.Disconnect(quit: true); // Disconnect and dispose if pool is full
            client.Dispose();
        }
    }

    public async Task<bool> SendAsync(EmailModel email, CancellationToken ct)
    {
        SmtpClient? smtpClient = null; // Declare outside try for finally block

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using var message = new MimeMessage();

                message.To.Clear();
                message.From.Add(new MailboxAddress(email.SenderName, email.SenderEmail));
                message.Sender = new MailboxAddress(email.SenderName, email.SenderEmail);

                var delimiters = new char[] { ',', ';', '|' };
                var receiver = email.RecipientEmail.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (string mailAddress in receiver)
                    message.To.Add(MailboxAddress.Parse(mailAddress));

                if (!string.IsNullOrEmpty(email.Cc))
                {
                    var cc = email.Cc.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string mailAddress in cc)
                        message.Cc.Add(MailboxAddress.Parse(mailAddress));
                        //message.Bcc.Add(MailboxAddress.Parse(mailAddress)); // Changed Cc to Bcc for privacy
                }

                var body = new BodyBuilder();
                message.Subject = email.Subject;

                if (!email.IsHtml)
                {
                    var pathToHtmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", email.Template);
                    if (!File.Exists(pathToHtmlFile))
                    {
                        _logger.LogError("Email template file not found: {PathToHtmlFile}", pathToHtmlFile);
                        throw new FileNotFoundException($"Email template file not found: {pathToHtmlFile}");
                    }

                    var htmlString = await File.ReadAllTextAsync(pathToHtmlFile, ct);
                    var emailBody = ProcessTemplate(htmlString,
                        JsonConvert.DeserializeObject<IDictionary<string, object>>(email.Body)!);

                    body.HtmlBody = emailBody;

                    // Ensure the Media:Images:Logos configuration path is correct and exists
                    var mediaImagesPath = _configuration.GetSection("Media").GetSection("Images").Value;
                    if (string.IsNullOrEmpty(mediaImagesPath))
                    {
                        _logger.LogWarning("Configuration 'Media:Images' is not set. Skipping logo embedding.");
                    }
                    else
                    {
                        var pathLogo = Path.Combine(mediaImagesPath, "Logos", email.Logo);
                        if (File.Exists(pathLogo))
                        {
                            var image = await body.LinkedResources.AddAsync(pathLogo, ct);
                            image.ContentId = MimeUtils.GenerateMessageId();
                            body.HtmlBody = body.HtmlBody.Replace("[img-logo]", $"cid:{image.ContentId}"); // Correct CID format
                        }
                        else
                        {
                            _logger.LogWarning("Email logo file not found: {PathLogo}", pathLogo);
                            // Consider if you want to throw an error or just log and continue without logo
                        }
                    }
                }
                else
                {
                    body.HtmlBody = email.Body;
                }

                if (email.Files is { Count: > 0 })
                {
                    foreach (var formFile in email.Files)
                    {
                        var extension = Path.GetExtension(formFile.FileName)?.ToLowerInvariant(); // Normalize extension
                        switch (extension)
                        {
                            case ".pdf":
                                body.Attachments.Add(formFile.FileName, formFile.Content,
                                    MimeKit.ContentType.Parse(MediaTypeNames.Application.Pdf));
                                break;
                            case ".xml":
                                body.Attachments.Add(formFile.FileName, formFile.Content,
                                    MimeKit.ContentType.Parse(MediaTypeNames.Application.Xml));
                                break;
                            default:
                                body.Attachments.Add(formFile.FileName, formFile.Content, MimeKit.ContentType.Parse(MediaTypeNames.Application.Octet));
                                //_logger.LogWarning("Unsupported attachment type: {FileName} ({Extension}). Skipping.", formFile.FileName, extension);
                                // You might want to add a generic application/octet-stream for unknown types
                                break;
                        }
                    }
                }

                message.Body = body.ToMessageBody();

                smtpClient = await GetConnectedSmtpClientAsync(email, ct);
                await smtpClient.SendAsync(message, ct);
                _logger.LogInformation("Email sent successfully to {RecipientEmail} with subject '{Subject}'.", email.RecipientEmail, email.Subject);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {RecipientEmail} with subject '{Subject}'.", email.RecipientEmail, email.Subject);
            // If the client becomes unusable (e.g., due to authentication failure), ensure it's not returned to the pool
            smtpClient?.Disconnect(quit: true, ct);
            smtpClient?.Dispose();
            smtpClient = null; // Mark as disposed so it's not returned to the pool
            throw; // Re-throw to propagate the error
        }
        finally
        {
            // Only return to pool if the client is still valid and connected
            if (smtpClient != null && smtpClient.IsConnected && smtpClient.IsAuthenticated)
            {
                ReturnSmtpClientToPool(smtpClient);
            }
            else if (smtpClient != null)
            {
                // If it's not connected/authenticated, dispose it as it's likely stale
                smtpClient.Dispose();
            }
        }
    }

    private static string ProcessTemplate(string template, IDictionary<string, object> data)
    {
        var reportData = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(data));

        var scriptObject = new ScriptObject();
        foreach (var prop in reportData)
        {
            scriptObject.Add(prop.Key, prop.Value);
        }

        var templateTo = Template.Parse(template);
        return templateTo.Render(scriptObject, member => LowerFirstCharacter(member.Name));
    }

    private static string LowerFirstCharacter(string value)
    {
        if (value.Length > 1)
            return char.ToLower(value[0]) + value.Substring(1);
        return value;
    }

    public void Dispose()
    {
        // Dispose all clients in the pool when the service is disposed
        while (_smtpClientPool.TryTake(out var client))
        {
            if (client.IsConnected)
            {
                client.Disconnect(quit: true);
            }
            client.Dispose();
        }
    }
}
