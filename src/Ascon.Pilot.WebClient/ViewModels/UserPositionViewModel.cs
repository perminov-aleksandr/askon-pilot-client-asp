using System;
using System.Collections.Generic;
using Ascon.Pilot.WebClient.Controllers;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class UserPositionViewModel
    {
        public Guid CurrentFolderId { get; set; }

        public FilesPanelType FilesPanelType { get; set; }

        public SidePanelViewModel SidePanel { get; set; }

        public List<FileViewModel> Files { get; set; }
    }
}