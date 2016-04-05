using System;
using System.Linq;

namespace Ascon.Pilot.Transport
{
    public interface IMarshallingFactory
    {
        ICallService GetUnmarshaller(IImplementationFactory implementationFactory);
        IGetService GetMarshaller(ICallService callService);
    }

    class MarshallingFactory : IMarshallingFactory
    {
        private IGetService _marshaller;
        private ICallService _unmarshaller;

        public ICallService GetUnmarshaller(IImplementationFactory implementationFactory)
        {
            return _unmarshaller ?? (_unmarshaller = new Unmarshaller(implementationFactory));
        }

        public IGetService GetMarshaller(ICallService callService)
        {
            return _marshaller ?? (_marshaller = new Marshaller(callService));
        }
    }

    class JsonMarshallingFactory : IMarshallingFactory
    {
        private IGetService _marshaller;
        private ICallService _unmarshaller;

        public ICallService GetUnmarshaller(IImplementationFactory implementationFactory)
        {
            return _unmarshaller ?? (_unmarshaller = new JsonUnmarshaller(implementationFactory));
        }

        public IGetService GetMarshaller(ICallService callService)
        {
            return _marshaller ?? (_marshaller = new Marshaller(callService));
        }
    }
}
