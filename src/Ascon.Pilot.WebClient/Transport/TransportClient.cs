using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Ascon.Pilot.WebClient;
using Microsoft.Extensions.Logging;

//using log4net;

namespace Ascon.Pilot.Transport
{
    public interface ICallbackReceiver
    {
        void Receive(byte[] data);
        void Error();
    }

    public class TransportClient : IDisposable
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof (TransportClient));
        private readonly HttpClient _client;
        private readonly CookieContainer _cookie;
        private readonly HttpClientHandler _handler;

        private ICallbackReceiver _callbackReceiver;
        private volatile string _url;
        private volatile bool _active;
        private long _callbackCount;

        public bool Active { get { return _active; } }

        public TransportClient()
        {
            //ServicePointManager.DefaultConnectionLimit = 10;
            //ServicePointManager.Expect100Continue = false; //for squid proxy server. 100-Continue behaviour off!!!
            
            _cookie = new CookieContainer();
            _handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookie,
                UseProxy = false
            };
            _client = new HttpClient(_handler, false);
        }

        public void Dispose()
        {
            _active = false;
            try
            {
                _client.CancelPendingRequests();
            }
            catch (Exception){ }
            
            _client.Dispose();
            _handler.Dispose();
        }

        public void Connect(string url)
        {
            this._url = url;
            _active = true;
            var task = _client.GetAsync(url + PilotMethod.CONNECT);
            ProcessResponse(task);
        }

        public void SetProxy(IWebProxy proxy)
        {
            if (_handler.Proxy == proxy) 
                return;
            
            _handler.UseProxy = proxy != null;
            if (_handler.UseProxy)
                _handler.Proxy = proxy;
        }

        public byte[] Call(byte[] data)
        {
            CheckActive();
            using (var content = new ByteArrayContent(data))
            {
                var task = _client.PostAsync(_url + PilotMethod.CALL, content);
                return ProcessResponse(task);
            }
        }

        private byte[] ProcessResponse(Task<HttpResponseMessage> task)
        {
            HttpResponseMessage response = null;
            try
            {
                byte[] result;
                try
                {
                    response = task.Result;
                    result = response.Content.ReadAsByteArrayAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions.Count == 1)
                    {
                        var inner = e.InnerExceptions[0];
                        if (inner is HttpRequestException)
                            throw new Exception(GetReadableMessage(inner), inner);
                        //if (inner is WebException)
                        //    throw new Exception(inner.Message, inner);
                        if (inner is SocketException)
                            throw new Exception(inner.Message, inner);
                        if (inner is TaskCanceledException)
                            throw new Exception("Connection lost", inner);
                        throw inner;
                    }
                    throw;
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.BadRequest:
                        _active = false;
                        throw new Exception("Connection lost");
                    case HttpStatusCode.InternalServerError:
                        throw ReadException(result);
                    default:
                        _active = false;
                        throw new Exception(response.ReasonPhrase);
                }
                return result;
            }
            finally
            {
                if(response!=null)
                    response.Dispose();
            }
        }

        private string GetReadableMessage(Exception exception)
        {
            var inner = exception.InnerException;
            if (inner == null) 
                return exception.Message;
            //if (inner is WebException)
            //    throw new TransportException(inner.Message, exception);
            if (inner is SocketException)
                throw new Exception(inner.Message, exception);
            if (inner is TaskCanceledException)
                throw new Exception("Connection lost", exception);
            return exception.Message;
        }

        private Exception ReadException(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                //IFormatter formatter = new BinaryFormatter();
                //var exception = formatter.Deserialize(stream) as Exception;
                Exception exception = null;
                if (exception == null)
                    exception = new ArgumentException("Can not deserialize exception from server");
                return exception;
            }
        }

        private void CheckActive()
        {
            if (_active == false)
                throw new Exception("Client connection is not active");
        }

        public void OpenCallback(ICallbackReceiver callbackReceiver)
        {
            CheckActive();
            if (callbackReceiver == null)
                throw new ArgumentNullException("callbackReceiver");
            this._callbackReceiver = callbackReceiver;
            SendCallbackRequestAsync();
        }

        private void SendCallbackRequestAsync()
        {
            _client.GetAsync(_url + PilotMethod.CALLBACK).ContinueWith(ProcessCallback);
            Interlocked.Increment(ref _callbackCount);
        }

        private void ProcessCallback(Task<HttpResponseMessage> task)
        {
            Interlocked.Decrement(ref _callbackCount);
            if (_active && Interlocked.Read(ref _callbackCount) == 0)
                SendCallbackRequestAsync();
            if (task.Status == TaskStatus.RanToCompletion)
            {
                using (var message = task.Result)
                {
                    var result = message.Content.ReadAsByteArrayAsync().Result;
                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        if (result.Length > 0)
                            _callbackReceiver.Receive(result);
                    }
                    if (message.StatusCode == HttpStatusCode.BadRequest)
                    {
                        _active = false;
                        _callbackReceiver.Error();
                    }
                }
            }
            if (task.Status == TaskStatus.Faulted)
            {
                _active = false;
                _callbackReceiver.Error();
                if (task.Exception != null) 
                    task.Exception.Handle(ex => true);
            }
        }

        public void Disconnect()
        {
            if(!_active)
                return;

            _active = false;
            try
            {
                var task = _client.GetAsync(_url + PilotMethod.DISCONNECT);
                ProcessResponse(task);
            }
            catch (Exception e)
            {
                //Logger<>.Error(string.Format("An error occured while disconnecting. URL: {0}", _url) ,e);
                throw;
            }
        }
    }
}
