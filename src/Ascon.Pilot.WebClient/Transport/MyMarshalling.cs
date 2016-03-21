using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.ViewModels;
using ProtoBuf;

namespace Ascon.Pilot.WebClient.Transport
{
    public interface ICallService
    {
        byte[] Call(byte[] data);
    }

    public interface IImplementationFactory
    {
        object GetImplementation(string interfaceName);
    }
    
    public interface IGetService
    {
        T Get<T>() where T : class;
    }

    public class Marshaller : IGetService
    {
        private readonly ICallService _service;

        public Marshaller(ICallService service)
        {
            this._service = service;
        }

        public T Get<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }

    public class Unmarshaller : ICallService
    {
        private readonly IImplementationFactory _factory;
        private readonly ConcurrentDictionary<string, object> _registered = new ConcurrentDictionary<string, object>();
        
        public Unmarshaller(IImplementationFactory factory)
        {
            this._factory = factory;
        }

        public byte[] Call(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            {
                var msg = ProtoSerializer.Deserialize<MarshallingMessage>(mem);
                var obj = GetImplementation(msg.Interface);
                var method = obj.GetType().GetMethod(msg.Method);
                var parameters = method.GetParameters();

                if (parameters.Length != msg.ParamCount && parameters.Count(p => p.IsOptional == false) != msg.ParamCount)
                    throw new InvalidDataException(
                        String.Format("Method {0}.{1} received {2} parameters, but {3} required",
                            msg.Interface, msg.Method, msg.ParamCount, parameters.Length));
                var values = new object[parameters.Count()];
                for (var i = 0; i < parameters.Count(); i++)
                {
                    if (msg.NullParamIndices.Contains(i))
                        continue;
                    values[i] = ProtoSerializer.Deserialize(mem, parameters[i].ParameterType);
                    if (parameters[i].IsOptional && values[i] == null)
                        values[i] = Type.Missing;
                }

                try
                {
                    var result = method.Invoke(obj, values);
                    return ResultToBytes(result);
                }
                catch (Exception e)
                {
                    if (e is TargetInvocationException && e.InnerException != null)
                        throw e.InnerException;
                    throw;
                }
            }
        }

        private byte[] CallToData(MethodInfo method, object[] nonOutInArgs)
        {
            using (var mem = new MemoryStream())
            {
                var call = new MarshallingMessage
                {
                    Interface = method.DeclaringType.Name,
                    Method = method.Name,
                    ParamCount = nonOutInArgs.Length
                };
                for (int i = 0; i < nonOutInArgs.Length; i++)
                {
                    if (nonOutInArgs[i] == null)
                        call.NullParamIndices.Add(i);
                }
                ProtoSerializer.Serialize(mem, call);

                foreach (var arg in nonOutInArgs)
                    if (arg != null)
                        ProtoSerializer.Serialize(mem, arg);

                return mem.ToArray();
            }
        }

        private byte[] ResultToBytes(object result)
        {
            if (result == null)
                return new byte[0];
            using (var mem = new MemoryStream())
            {
                ProtoSerializer.Serialize(mem, result);
                return mem.ToArray();
            }
        }

        private object GetImplementation(string interfaceName)
        {
            return _registered.GetOrAdd(interfaceName, (iName) => _factory.GetImplementation(iName));
        }
    }
    
    static class ProtoSerializer
    {
        public static void Serialize(Stream stream, object instance)
        {
            ProtoBuf.Meta.RuntimeTypeModel.Default.SerializeWithLengthPrefix(
                stream, instance, instance.GetType(), ProtoBuf.PrefixStyle.Base128, 0);
        }

        public static object Deserialize(Stream stream, Type type)
        {
            return ProtoBuf.Meta.RuntimeTypeModel.Default.DeserializeWithLengthPrefix(
                stream, null, type, ProtoBuf.PrefixStyle.Base128, 0);
        }

        public static T Deserialize<T>(Stream stream)
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Base128, 0);
        }
    }
}