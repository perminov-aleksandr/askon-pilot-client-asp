using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetObjectsRequest : Request<DObject[]>
    {
        public GetObjectsRequest() 
            : base(ApplicationConst.PilotServerApiName, ApiMethod.GetObjects)
        {
        }
        
        public Guid[] ids { get; set; }

        public new async Task<DObject[]> SendAsync(HttpClient client)
        {
            if (ids == null || ids.Length == 0)
                return null;

            var stringResult = await PostAsync(client);
            try
            {
                var dObjectArray = JsonConvert.DeserializeObject<DObject[]>(stringResult);
                var jObjectArray = JsonConvert.DeserializeObject<JObject[]>(stringResult);
                for (var i = 0; i < jObjectArray.Length; i++)
                {
                    var dAttributes = dObjectArray[i].Attributes;
                    var jAttributes = jObjectArray[i]["Attributes"];
                    dObjectArray[i].Attributes = ExtractNonParsedAttributes(dAttributes, jAttributes);
                }
                return dObjectArray;
            }
            catch (JsonReaderException ex)
            {
                return null;
            }
        }

        private static SortedList<string, DValue> ExtractNonParsedAttributes(SortedList<string, DValue> dAttributes, JToken jAttributes)
        {
            var newAttributes = new SortedList<string, DValue>(dAttributes.Count);
            foreach (var attribute in dAttributes)
            {
                //todo: reflection GetType().GetProperties() ??
                var strVal = jAttributes[attribute.Key].SelectToken("StrValue")?.Value<string>();
                var intVal = jAttributes[attribute.Key].SelectToken("IntValue")?.Value<long?>();
                var dateVal = jAttributes[attribute.Key].SelectToken("DateValue")?.Value<DateTime?>();
                var decVal = jAttributes[attribute.Key].SelectToken("DecimalValue")?.Value<decimal?>();
                var arrVal = jAttributes[attribute.Key].SelectToken("ArrayValue")?.Value<string[]>();
                var isArr = jAttributes[attribute.Key].SelectToken("IsArray")?.Value<bool>();
                var dblVal = jAttributes[attribute.Key].SelectToken("DoubleValue")?.Value<double?>();

                var dValue = new DValue();

                if (!string.IsNullOrEmpty(strVal))
                    dValue.StrValue = strVal;
                else if (intVal.HasValue)
                    dValue.IntValue = intVal;
                else if (dateVal.HasValue)
                    dValue.DateValue = dateVal;
                else if (decVal.HasValue)
                    dValue.DecimalValue = decVal;
                else if (dblVal.HasValue)
                    dValue.DoubleValue = dblVal;
                else if (arrVal != null && isArr.HasValue && isArr.Value)
                    dValue.ArrayValue = arrVal;
                
                newAttributes.Add(attribute.Key, dValue);
            }
            return newAttributes;
        }
    }
}