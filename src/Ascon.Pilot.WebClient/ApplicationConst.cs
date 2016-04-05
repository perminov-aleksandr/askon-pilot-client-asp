using Microsoft.Extensions.Configuration;

namespace Ascon.Pilot.WebClient
{
    public static class ApplicationConst
    {
        static ApplicationConst()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var config = builder.Build();
            PilotServerUrl = config["PilotServer:Url"];
        }

        public static readonly string PilotServerUrl;
        public static readonly string PilotServerApiName = "IServerApi";
        public static readonly string PilotMiddlewareInstanceName = "AskonPilotMiddlewareInstance";

        public static readonly string HttpSchemeName = "http";
        public static readonly string SchemeDelimiter = "://";
    }

    public static class ApiMethod
    {
        public static readonly string OpenDatabase = "OpenDatabase";
        public static readonly string GetObjects = "GetObjects";
    }

    public static class Roles
    {
        public static readonly string Admin = "Администратор";
        public static readonly string User = "Пользователь";
    }

    public static class PilotMethod
    {
        public const string ROOT = "/";
        public const string CONNECT = "/connect";
        public const string CALL = "/call";
        public const string DISCONNECT = "/disconnect";
        public const string CALLBACK = "/callback";
        public const string WEB_CONNECT = "/web/connect";
        public const string WEB_CALL = "/web/call";
        public const string WEB_FILE = "/web/file";
    }
}
