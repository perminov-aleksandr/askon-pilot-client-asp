using System;
using System.Collections.Generic;
using System.Linq;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class SidePanelViewModel
    {
        public IReadOnlyDictionary<int, MType> Types { get; set; }
        public Guid ObjectId { get; set; }
        public List<SidePanelItem> Items { get; set; }

        public dynamic[] ToDynamic()
        {
            var result = new List<dynamic>(Items.Count);
            foreach (var sidePanelItem in Items)
            {
                result.Add(sidePanelItem.GetDynamic(ObjectId, Types));
            }
            return result.ToArray();
        }
    }

    public class SidePanelItem
    {
        //public string Name { get; set; }
        public DObject DObject { get; set; }
        public List<SidePanelItem> SubItems { get; set; }

        public dynamic GetDynamic(Guid id, IReadOnlyDictionary<int, MType> types)
        {
            var nodes = new List<dynamic>();
            if (SubItems != null)
                foreach (var sidePanelItem in SubItems)
                {
                    nodes.Add(sidePanelItem.GetDynamic(id, types));
                }
            return new
            {
                id = DObject.Id,
                text = $"{types[DObject.TypeId].Title} {DObject.Id}",
                state = new {
                    selected = DObject.Id == id
                },
                nodes = nodes.Any() ? nodes.ToArray() : null
            };
        }
    }
}
