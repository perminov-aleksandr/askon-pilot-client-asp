using System.Collections.Generic;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class UserPositionViewModel
    {
        public List<string> Path { get; set; }

        public SidePanelViewModel SidePanel { get; set; }

        public List<FileViewModel> Files { get; set; }
    }
}