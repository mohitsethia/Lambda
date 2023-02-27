using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace SendEmailLambda;

public static class SecretsManager
{
    public static async Task<string> GetSecretValue(string secretId)
    {
        IAmazonSecretsManager secretsManagerClient = new AmazonSecretsManagerClient();
        var getSecretValueRequest = new GetSecretValueRequest
        {
            SecretId = secretId
        };
        var getSecretValueResponse = await secretsManagerClient.GetSecretValueAsync(getSecretValueRequest);
        string secretValue = getSecretValueResponse.SecretString;
        return secretValue;
    }
}