using System.Net;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;
using SendEmailLambda.DTO;

namespace SendEmailLambda.Services;

public class SendTemplatedEmailService
{
    public HttpStatusCode SendTemplatedEmail(Amazon.SQS.Model.Message message)
    {
        TemplateEmailDTO templateEmailDto = new TemplateEmailDTO();
        foreach (var attribute in message.MessageAttributes)
        {
            var propertyName = attribute.Key;
            var propertyValue = attribute.Value.StringValue;

            var propertyInfo = templateEmailDto.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                propertyInfo.SetValue(templateEmailDto, convertedValue);
            }
        }

        var request = new SendTemplatedEmailRequest
        {
            Source = templateEmailDto.sendFrom,
            Destination = new Destination
            {
                ToAddresses = new List<string>{templateEmailDto.sendTo}
            },
            Template = templateEmailDto.templateName,
            TemplateData = templateEmailDto.templateData
        };
        Console.WriteLine("Sending templated email");
        var response = AmazonSimpleEmailService.GetAmazonSESClient().SendTemplatedEmailAsync(request);
        return response.Result.HttpStatusCode;
    }
}