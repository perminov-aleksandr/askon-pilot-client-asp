using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class ClientsStorage
    {
        private static readonly Dictionary<Guid, HttpClient>  ClientsDictionary = new Dictionary<Guid, HttpClient>();

        public static HttpClient GetClient(this ISession session)
        {
            var clientIdString = session.GetString(SessionKeys.ClientId);
            if (string.IsNullOrEmpty(clientIdString))
                return null;

            var clientId = Guid.Parse(clientIdString);
            if (ClientsDictionary.ContainsKey(clientId))
            {
                var client = ClientsDictionary[clientId];
                return client;
            }

            var httpClient = CreateHttpClient();

            try
            {
                var result = httpClient.PostAsync(PilotMethod.WEB_CONNECT, new StringContent(string.Empty)).Result;
                if (!result.IsSuccessStatusCode)
                    throw new Exception($"Server connection failed with status: {result.StatusCode}");
            }
            catch (Exception)
            {
                return null;
            }

            ClientsDictionary.Add(clientId, httpClient);
            return httpClient;
        }

        public static HttpClient CreateHttpClient()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookieContainer};
            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(ApplicationConst.PilotServerUrl)
            };
            return client;
        }
    }
}
