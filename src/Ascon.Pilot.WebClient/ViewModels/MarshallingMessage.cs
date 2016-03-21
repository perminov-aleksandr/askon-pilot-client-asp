using System.Collections.Generic;
using ProtoBuf;

namespace Ascon.Pilot.WebClient.ViewModels
{
    [ProtoContract]
    public class MarshallingMessage
    {
        [ProtoMember(1)]
        public string Interface { get; set; }

        [ProtoMember(2)]
        public string Method { get; set; }

        [ProtoMember(3)]
        public int ParamCount { get; set; }

        [ProtoMember(4)]
        public List<int> NullParamIndices { get; private set; }

        public MarshallingMessage()
        {
            NullParamIndices = new List<int>();
        }
    }
}