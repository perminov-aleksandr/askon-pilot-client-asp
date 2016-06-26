using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Controllers;
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
        public static readonly string PilotMiddlewareInstanceName = "AskonPilotMiddlewareInstance";

        public static readonly string HttpSchemeName = "http";
        public static readonly string SchemeDelimiter = "://";
        public static readonly string AppName = "Web-клиент Pilot ICE";

        public static readonly string DefaultGlyphicon = "";
        public static readonly IDictionary<string, string> TypesGlyphiconDictionary = new Dictionary<string, string>()
        {
            { SystemTypes.PROJECT_FOLDER, "glyphicon glyphicon-folder-open"},
            { SystemTypes.PROJECT_FILE, "glyphicon glyphicon-file" },
            { SystemTypes.SMART_FOLDER, "glyphicon glyphicon-book" }
        };

        //implicitly default files panel type is LIST cause its index in enum is 0
        public const FilesPanelType DefaultFilesPanelType = 0;

        public const int SOURCEFOLDER_TYPEID = -24;
    }
    
    public static class Roles
    {
        public static readonly string Admin = "Администратор";
        public static readonly string User = "Пользователь";
    }
}
