using System.Net;
using System.Net.Mail;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.Engines;

public class EmailEngine : IEmailEngine
{
    private readonly SmtpConfig _smtpConfig;
    private readonly IConfiguration _configuration;

    public EmailEngine(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpConfig = GetSmtpConfig();
    }

    private SmtpConfig GetSmtpConfig()
    {
        var smtpConfig = new SmtpConfig
        {
            Host = _configuration["SmtpConfigs:SmtpHost"] ?? string.Empty,
            Port = int.TryParse(_configuration["SmtpConfigs:SmtpPort"], out var port) ? port : 0,
            Username = _configuration["SmtpConfigs:SmtpUsername"] ?? string.Empty,
            Password = _configuration["SmtpConfigs:SmtpPassword"] ?? string.Empty,
            Recipients = _configuration["SmtpConfigs:SmtpRecipients"] ?? string.Empty
        };
        
        ValidateSmtpConfig(smtpConfig);

        return smtpConfig;
    }
    
    private void ValidateSmtpConfig(SmtpConfig smtpConfig)
    {
        if (string.IsNullOrEmpty(smtpConfig.Host))
            throw new ArgumentException("SMTP Host is not configured.");
        if (smtpConfig.Port == 0)
            throw new ArgumentException("SMTP Port is not configured or invalid.");
        if (string.IsNullOrEmpty(smtpConfig.Username))
            throw new ArgumentException("SMTP Username is not configured.");
        if (string.IsNullOrEmpty(smtpConfig.Password))
            throw new ArgumentException("SMTP Password is not configured.");
        if (string.IsNullOrEmpty(smtpConfig.Recipients))
            throw new ArgumentException("SMTP Recipients are not configured.");
    }

    public async Task SendEmailAsync(EmailInfo emailInfo)
    {
        if (IsEmailConfigMissing())
        {
            Console.WriteLine("Email configuration is missing.");
            return;
        }

        try
        {
            var email = CreateEmailMessage(emailInfo);
            await SendEmailAsync(email);
        }
        catch (Exception ex)
        {
            Console.WriteLine("EmailEngine: SendEmailAsync()", ex);
        }
    }

    private bool IsEmailConfigMissing()
    {
        return string.IsNullOrEmpty(_smtpConfig.Host)
               || string.IsNullOrEmpty(_smtpConfig.Recipients)
               || string.IsNullOrEmpty(_smtpConfig.Username)
               || string.IsNullOrEmpty(_smtpConfig.Password)
               || _smtpConfig.Port == 0;
    }

    private MailMessage CreateEmailMessage(EmailInfo emailInfo)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_smtpConfig.Username, "Gateway"),
            Subject = emailInfo.Subject,
            Body = emailInfo.Message,
            IsBodyHtml = true
        };

        foreach (var recipient in GetSplitArray(_smtpConfig.Recipients))
        {
            message.To.Add(new MailAddress(recipient));
        }

        foreach (var bcc in GetSplitArray(emailInfo.Bcc))
        {
            if (!string.IsNullOrEmpty(bcc))
                message.Bcc.Add(new MailAddress(bcc));
        }

        foreach (var cc in GetSplitArray(emailInfo.Cc))
        {
            if (!string.IsNullOrEmpty(cc))
                message.CC.Add(new MailAddress(cc));
        }

        return message;
    }

    private async Task SendEmailAsync(MailMessage message)
    {
        try
        {
            using var smtpClient = new SmtpClient
            {
                Host = _smtpConfig.Host,
                Port = _smtpConfig.Port,
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password)
            };

            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR EmailEngine: SendEmailAsync()", ex);
        }
    }

    private IEnumerable<string> GetSplitArray(string emailStr)
    {
        if (string.IsNullOrEmpty(emailStr))
            return Array.Empty<string>();

        var delimiter = emailStr.Contains(",") ? "," : (emailStr.Contains(";") ? ";" : " ");
        return emailStr.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
    }
}