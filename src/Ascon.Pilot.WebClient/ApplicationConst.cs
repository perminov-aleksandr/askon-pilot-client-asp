using System.Collections.Generic;
using Ascon.Pilot.Core;
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
        public static readonly string AppName = "Web-клиент Pilot ICE";

        public static readonly string DefaultGlyphicon = "";
        public static readonly IDictionary<string, string> TypesGlyphiconDictionary = new Dictionary<string, string>()
        {
            { SystemTypes.PROJECT_FOLDER, "folder-open"},
            { SystemTypes.PROJECT_FILE, "file" },
            { SystemTypes.SMART_FOLDER, "book" }
        };
    }
    
    //todo: use reflection with IServerApi methods
    public static class ApiMethod
    {
        public static readonly string OpenDatabase = "OpenDatabase";
        public static readonly string GetObjects = "GetObjects";
        public static readonly string GetFileChunk = "GetFileChunk";
        public static readonly string GetMetadata = "GetMetadata";
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
