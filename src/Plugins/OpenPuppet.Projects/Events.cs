using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Projects
{
    public static class Events
    {
        public static void Subscribe()
        {
            SDK.Events.IEvent<EventArgs>.Subscribe("openpuppet.projects.create", CreateProject);
            SDK.Events.IEvent<EventArgs>.Subscribe("openpuppet.projects.open", OpenProject);
        }

        public static void CreateProject(object? sender, EventArgs e)
        {
            IUIDialog.Open("openpuppet.projects.createproject");
        }

        public static void OpenProject(object? sender, EventArgs e)
        {
            IUIDialog.Open("openpuppet.projects.openproject");
        }
    }
}
