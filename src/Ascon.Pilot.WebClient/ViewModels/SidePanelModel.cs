using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class SidePanelViewModel
    {
        public Guid ObjectId { get; set; }
        public List<SidePanelItem> Items { get; set; }
    }

    public class SidePanelItem
    {
        public DObject DObject { get; set; }
        public List<SidePanelItem> SubItems { get; set; } 
    }
}
