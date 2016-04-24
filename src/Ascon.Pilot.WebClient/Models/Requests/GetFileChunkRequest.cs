using System;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetFileChunkRequest : Request
    {
        public GetFileChunkRequest() : base(ApplicationConst.PilotServerApiName, "GetFileChunk")
        {
        }

        public Guid id { get; set; }
        public long pos { get; set; }
        public long count { get; set; }
    }
}