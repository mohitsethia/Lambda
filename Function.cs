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
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SendEmailHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var sqsClient = SQSClientProvider.GetSQSClient();
        var qUrl = AppConfig.app_settings["QueueURL"];
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
            } else if (entry.Key == "emailSubject") {
                email.Subject = entry.Value.StringValue;
            }
        }
        email.Body = new TextPart(TextFormat.Html);

        // send email
        SMTPClient.GetSMTPClient().Send(email);

    }
}