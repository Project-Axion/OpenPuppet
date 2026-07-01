using OpenPuppet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SVG
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.SVG";

        public void OnInitialized()
        {
            Console.WriteLine("Hello plugin");
        }
    }
}