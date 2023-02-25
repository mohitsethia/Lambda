using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Runtime;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendEmailLambda;

public class Function
{
    private readonly IConfigurationSection app_settings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings");
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SendEmailHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var accessKeyID = app_settings["AccessKeyID"];
        var secretKey = app_settings["SecretAccessKey"];
        var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
        var sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
        var qUrl = "https://sqs.us-east-1.amazonaws.com/363402790710/testqueue";
        var message = await GetMessage(sqsClient, qUrl, 2);
        foreach (var msg in message.Messages)
        {
            await ProcessMessageAsync(msg, context);
        }
    }

    //
    // Method to read a message from the given queue
    // In this example, it gets one message at a time
    private static async Task<ReceiveMessageResponse> GetMessage(
      IAmazonSQS sqsClient, string qUrl, int waitTime=0)
    {
        List<string> AttributesList = new List<string>();
        AttributesList.Add("*");
      return await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest{
        QueueUrl=qUrl,
        WaitTimeSeconds=waitTime,
        MessageAttributeNames = AttributesList
        // (Could also request attributes, set visibility timeout, etc.)
      });
    }

    private async Task ProcessMessageAsync(Message message, ILambdaContext context)
    {
        context.Logger.LogLine($"Processed message {message.Body}");
        // create message
        var email = new MimeMessage();
        foreach (var entry in message.MessageAttributes)
        {
            if(entry.Key == "sendFrom") {
              email.From.Add(MailboxAddress.Parse(entry.Value.StringValue));
            } else if(entry.Key == "sendTo") {
              email.To.Add(MailboxAddress.Parse(entry.Value.StringValue));
            }
        }
        email.Subject = "Hey there!";
        email.Body = new TextPart(TextFormat.Html);

        // send email
        using var smtp = new SmtpClient();
        smtp.Connect(app_settings["SmtpHost"], int.Parse(app_settings["SmtpPort"]), SecureSocketOptions.StartTls);
        smtp.Authenticate(app_settings["SmtpUser"], app_settings["SmtpPass"]);
        smtp.Send(email);
        // var sendGridClient = new SendGridClient("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");

        // var from = new EmailAddress($"'{message.Attributes["sendFrom"]}'", $"'{message.Attributes["sendFromName"]}'");
        // var subject = "Sending with SendGrid is Fun";
        // var to = new EmailAddress($"'{message.Attributes["sendTo"]}'", $"'{message.Attributes["sendToName"]}'");
        // var plainTextContent = "and easy to do anywhere, even with C#";
        // var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
        // var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        // var response = await sendGridClient.SendEmailAsync(msg);

        // context.Logger.LogLine($"Email sent: {response.StatusCode}");

    }
}