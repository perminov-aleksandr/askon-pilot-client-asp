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
        public static readonly string OpenDatabaseMethod = "OpenDatabase";

        public static readonly string PilotServerConnectUrl = "/web/connect";
        public static readonly string PilotServerCallApiUrl = "/web/call";
    }
}
