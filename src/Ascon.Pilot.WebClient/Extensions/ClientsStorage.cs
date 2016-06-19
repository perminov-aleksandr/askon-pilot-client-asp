using System;
using System.Collections.Generic;
using System.IO;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;
using Microsoft.AspNet.Http;
using ISession = Microsoft.AspNet.Http.Features.ISession;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class SessionExt
    {
        public static IDictionary<int, MType> GetMetatypes(this ISession session)
        {
            using (var ms = new MemoryStream(session.Get(SessionKeys.MetaTypes)))
            {
                return ProtoBuf.Serializer.Deserialize<IDictionary<int, MType>>(ms);
            }
        }

        /// <summary>
        /// Set value of type <typeparam name="T">T</typeparam> at session dictionary using protobuf-net
        /// </summary>
        /// <typeparam name="T">type of value to set. Must be proto-serializable</typeparam>
        /// <param name="session">session to add values</param>
        /// <param name="key">key of value</param>
        /// <param name="value">value to set</param>
        public static void SetSessionValues<T>(this ISession session, string key, T value)
        {
            using (var bs = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(bs, value);
                session.Set(key, bs.ToArray());
            }
        }
    }

    public static class ClientsStorage
    {
        private static readonly Dictionary<Guid, HttpPilotClient>  ClientsDictionary = new Dictionary<Guid, HttpPilotClient>();

        public static HttpPilotClient GetClient(this ISession session)
        {
            var clientIdString = session.GetString(SessionKeys.ClientId);
            if (string.IsNullOrEmpty(clientIdString))
            {
                clientIdString = Guid.NewGuid().ToString();
                session.SetString(SessionKeys.ClientId, clientIdString);
            }

            var clientId = Guid.Parse(clientIdString);
            if (ClientsDictionary.ContainsKey(clientId))
            {
                var client = ClientsDictionary[clientId];
                if (!client.IsClientActive())
                {
                    return null;
                    /*client.Connect(ApplicationConst.PilotServerUrl);
                    var serverApi = session.GetServerApi();
                    var dbInfo = serverApi.OpenDatabase(
                        session.GetString(SessionKeys.DatabaseName), 
                        session.GetString(SessionKeys.Login),
                        session.GetString(SessionKeys.ProtectedPassword), false);
                    if (dbInfo == null)
                        return null;*/
                }
                return client;
            }

            var newClient = new HttpPilotClient();
            ClientsDictionary.Add(clientId, newClient);
            return newClient;
        }

        public static IServerApi GetServerApi(this ISession session, IServerCallback callback = null)
        {
            if (callback == null)
                callback = CallbackFactory.Get<IServerCallback>();
            var client = session.GetClient();
            if (client == null)
                client = Reconnect(session, callback);
            return client.GetServerApi(callback);
        }

        private static HttpPilotClient Reconnect(ISession session, IServerCallback callback)
        {
            var clientIdString = session.GetString(SessionKeys.ClientId);
            var client = new HttpPilotClient();

            client.Connect(ApplicationConst.PilotServerUrl);

            var serverApi = client.GetServerApi(callback);

            var dbName = session.GetString(SessionKeys.DatabaseName);
            var login = session.GetString(SessionKeys.Login);
            var password = session.GetString(SessionKeys.ProtectedPassword);
            var useWindowsAuth = login.Contains("/") || login.Contains("\\");
            var dbInfo = serverApi.OpenDatabase(dbName, login, password, useWindowsAuth);
            if (dbInfo == null)
                throw new TransportException();
            ClientsDictionary[Guid.Parse(clientIdString)] = client;
            return client;
        }
    }
}
