using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;

namespace SendEmailLambda;

public static class SQSClientProvider
{
    private static readonly IAmazonSQS _sqsClient;
    
    static SQSClientProvider()
    {
        var accessKeyID = AppConfig.app_settings["AccessKeyID"];
        var secretKey = AppConfig.app_settings["SecretAccessKey"];
        var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
        var sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.USEast1);
        _sqsClient = sqsClient;
    }
 
    public static IAmazonSQS GetSQSClient()
    {
        return _sqsClient;
    }
}