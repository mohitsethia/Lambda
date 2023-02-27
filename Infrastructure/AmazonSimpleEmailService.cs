using Amazon;
using Amazon.Internal;
using Amazon.Runtime;
using Amazon.SimpleEmail;

namespace SendEmailLambda;

public static class AmazonSimpleEmailService
{
    private static readonly AmazonSimpleEmailServiceClient _amazonSimpleEmailServiceClient;

    static AmazonSimpleEmailService()
    {
        var accessKeyID = AppConfig.app_settings["AccessKeyId"];
        var secretKey = AppConfig.app_settings["SecretAccessKey"];
        var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
        var client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.APSoutheast1);
        _amazonSimpleEmailServiceClient = client;
    }

    public static AmazonSimpleEmailServiceClient GetAmazonSESClient()
    {
        return _amazonSimpleEmailServiceClient;
    }
}