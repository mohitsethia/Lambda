using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using SendEmailLambda.DTO;
using SendEmailLambda.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendEmailLambda;

public class Function
{
    private readonly SendEmailService _sendEmailService = new SendEmailService();
    private readonly SendTemplatedEmailService _sendTemplatedEmailService = new SendTemplatedEmailService();
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SendEmailHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var sqsClient = SQSClientProvider.GetSQSClient();
        var qUrl = AppConfig.app_settings["QueueUrl"];
        var message = await GetMessage(sqsClient, qUrl, 2);
        Console.WriteLine($"message count: {message.Messages.Count}");
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
        List<string> attributesList = new List<string>();
        attributesList.Add("*");
        return await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest{
        QueueUrl=qUrl,
        WaitTimeSeconds=waitTime,
        MessageAttributeNames = attributesList
        // (Could also request attributes, set visibility timeout, etc.)
      });
    }

    private async Task<HttpStatusCode> ProcessMessageAsync(Message message, ILambdaContext context)
    {
        // context.Logger.LogLine($"Processed message {message.Body}");
        HttpStatusCode response = HttpStatusCode.Ambiguous;
        if (message.MessageAttributes.ContainsKey("eventType"))
        {
            MessageAttributeValue value;
            message.MessageAttributes.TryGetValue("eventType", out value);
            if (value.StringValue == "sendEmail") {
                response = _sendEmailService.SendEmailViaSmtp(message);
            } else if (value.StringValue == "sendTemplatedEmail") {
                response = _sendTemplatedEmailService.SendTemplatedEmail(message);
            }
        }
        return response;
    }
}