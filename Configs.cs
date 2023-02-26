using Microsoft.Extensions.Configuration;

namespace SendEmailLambda;

public static class AppConfig
{
    public static readonly IConfigurationSection app_settings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings");
}