using System.Dynamic;
using System.Net.Mime;
using System.Security.Authentication;
using Common.Notifications.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Utils;
using Newtonsoft.Json;
using Scriban;

namespace Common.Notifications.Services;
public interface IMailKitService
{
    Task<bool> Send(EmailModel email, CancellationToken ct = default);
}
public class MailKitService(IConfiguration configuration) : IMailKitService
{
    public async Task<bool> Send(EmailModel email, CancellationToken ct)
    {
        using var message = new MimeMessage();

        message.To.Clear();
        message.From.Add(new MailboxAddress(email.DisplayName, email.From));
        message.Sender = new MailboxAddress(email.DisplayName, email.From);

        var delimiters = new char[] { ',', ';', '|' };
        var receiver = email.To.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        foreach (string mailAddress in receiver)
            message.To.Add(MailboxAddress.Parse(mailAddress));


        if (!string.IsNullOrEmpty(email.Cc))
        {
            var cc = email.Cc.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            ;
            foreach (string mailAddress in cc)
                message.Bcc.Add(MailboxAddress.Parse(mailAddress));
        }

        var body = new BodyBuilder();

        message.Subject = email.Subject;

        var pathToHtmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", email.Template);

        if (!email.IsHtml)
        {
            var htmlString = await File.ReadAllTextAsync(pathToHtmlFile, ct);
            var emailBody = ProcessTemplate(htmlString,
                JsonConvert.DeserializeObject<IDictionary<string, object>>(email.Body));

            body.HtmlBody = emailBody;

            var pathLogo = Path.Combine(configuration.GetSection("Media").GetSection("Images").Value, "Logos", email.Logo);

            var image = await body.LinkedResources.AddAsync(pathLogo, ct);
            image.ContentId = MimeUtils.GenerateMessageId();

            body.HtmlBody = body.HtmlBody.Replace("[img-logo]", image.ContentId);
        }
        else
        {
            body.HtmlBody = email.Body;
        }

        if (email.Files is { Count: > 0 })
        {
            foreach (var formFile in email.Files)
            {
                var extension = Path.GetExtension(formFile.FileName);
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
                }
            }
        }

        message.Body = body.ToMessageBody();

        var smtpClient = new SmtpClient();
        //using SmtpClient smtpClient = new(new ProtocolLogger("smtp.log"));

        //if (email.enableSsl)
        //{
        //    await smtpClient.ConnectAsync(email.host, email.port, SecureSocketOptions.SslOnConnect, ct);
        //}
        //else
        //{
        //    await smtpClient.ConnectAsync(email.host, email.port, SecureSocketOptions.StartTls, ct);

        //smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        smtpClient.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

        smtpClient.CheckCertificateRevocation = false;
        await smtpClient.ConnectAsync(email.Host, email.Port, false, ct);
        await smtpClient.AuthenticateAsync(email.From, email.Password, ct);
        await smtpClient.SendAsync(message, ct);
        await smtpClient.DisconnectAsync(true, ct);
        return true;
    }

    private static string ProcessTemplate(string template, IDictionary<string, object> data)
    {
        var reportData = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(data));

        var scriptObject = new Scriban.Runtime.ScriptObject();
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
}
