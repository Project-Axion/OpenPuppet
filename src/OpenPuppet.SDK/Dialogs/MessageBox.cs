using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Dialogs
{
    public class MessageBox : IUIDialog
    {
        public string Title { get; set; }
        public ImGuiWindowFlags? Flags { get; set; }
        public Vector2? Size { get; set; }

        public void OnClose() { }

        public void OnLoad() { }

        public void OnPreRender() { }

        public void OnRender()
        {

        }
    }
}
