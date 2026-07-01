using OpenPuppet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.SVG
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.Core.SVG";

        public void OnInitialized()
        {
            Console.WriteLine("Hello plugin");
        }
    }
}