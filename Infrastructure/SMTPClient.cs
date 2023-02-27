using Amazon.SecretsManager.Model;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace SendEmailLambda;

public static class SMTPClient
{
    private static readonly SmtpClient _smtpClient; 
    static SMTPClient()
    {
        using var smtp = new SmtpClient();
        smtp.Connect(AppConfig.app_settings["SmtpHost"],587, SecureSocketOptions.StartTls);
        smtp.Authenticate(AppConfig.app_settings["SmtpUser"],AppConfig.app_settings["SmtpPass"]);
        _smtpClient = smtp;
    }

    public static SmtpClient GetSMTPClient()
    {
        return _smtpClient;
    }
}