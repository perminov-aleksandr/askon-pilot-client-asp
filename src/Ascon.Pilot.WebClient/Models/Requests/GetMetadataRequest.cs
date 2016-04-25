using Ascon.Pilot.Core;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetMetadataRequest : Request<DMetadata>
    {
        public GetMetadataRequest() : base(ApplicationConst.PilotServerApiName, ApiMethod.GetMetadata)
        {
        }

        public long localVersion { get; set; }
    }
}
