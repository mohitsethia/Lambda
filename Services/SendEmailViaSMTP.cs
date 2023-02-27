using System.Net;
using Amazon.SimpleEmail.Model;
using MimeKit;
using MimeKit.Text;
using SendEmailLambda.DTO;
using Message = Amazon.SQS.Model.Message;

namespace SendEmailLambda.Services;

public class SendEmailService
{
    public HttpStatusCode SendEmailViaSESClient(Amazon.SQS.Model.Message message)
    {
        EmailDTO emailDto = new EmailDTO();
        foreach (var attribute in message.MessageAttributes)
        {
            var propertyName = attribute.Key;
            var propertyValue = attribute.Value.StringValue;

            var propertyInfo = emailDto.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                propertyInfo.SetValue(emailDto, convertedValue);
            }
        }
        var request = new SendEmailRequest
        {
            Destination = new Destination
            {
                // BccAddresses = bccAddresses,
                // CcAddresses = ccAddresses,
                ToAddresses = new List<string>{emailDto.sendTo}
            },
            Message = new Amazon.SimpleEmail.Model.Message
            {
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = emailDto.htmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = emailDto.textBody
                    }
                },
                Subject = new Content
                {
                    Charset = "UTF-8",
                    Data = emailDto.emailSubject
                }
            },
            Source = emailDto.sendFrom
        };
        Console.WriteLine("Sending Email via AWS SES Service");
        var response = AmazonSimpleEmailService.GetAmazonSESClient().SendEmailAsync(request);
        return response.Result.HttpStatusCode;
    }
    public HttpStatusCode SendEmailViaSmtp(Message message)
    {
        string sendFrom = "";
        string sendTo = "";
        string emailSubject = "";
        foreach (var entry in message.MessageAttributes)
        {
            if(entry.Key == "sendFrom") {
                sendFrom = entry.Value.StringValue;
            } else if(entry.Key == "sendTo") {
                sendTo = entry.Value.StringValue;
            } else if (entry.Key == "emailSubject") {
                emailSubject = entry.Value.StringValue;
            }
        }
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(sendFrom));
        email.To.Add(MailboxAddress.Parse(sendTo));
        email.Subject = emailSubject;
        email.Body = new TextPart(TextFormat.Html);

        // send email
        // var smtp = new SMTPClient();
        Console.WriteLine("Sending email via SMTP");
        var response = SMTPClient.GetSMTPClient().SendAsync(email);
        return HttpStatusCode.Accepted;
    }
}