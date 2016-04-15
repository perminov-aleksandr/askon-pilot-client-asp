using System;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetObjectsRequest : Request
    {
        public Guid[] ids { get; set; }  
    }
}