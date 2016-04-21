using System;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetObjectsRequest : Request
    {
        public GetObjectsRequest() 
            : base(ApplicationConst.PilotServerApiName, ApiMethod.GetObjects)
        {
        }
        
        public Guid[] ids { get; set; }  
    }
}