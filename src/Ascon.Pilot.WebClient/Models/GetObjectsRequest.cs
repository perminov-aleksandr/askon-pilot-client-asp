using System;

namespace Ascon.Pilot.WebClient.Models
{
    public class GetObjectsRequest : Request
    {
        public Guid[] ids { get; set; }  
    }
}