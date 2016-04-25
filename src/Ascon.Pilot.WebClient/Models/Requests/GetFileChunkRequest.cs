using System;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetFileChunkRequest : Request<byte[]>
    {
        public GetFileChunkRequest() : base(ApplicationConst.PilotServerApiName, ApiMethod.GetFileChunk)
        {
        }

        public Guid id { get; set; }
        public long pos { get; set; }
        public long count { get; set; }
    }
}