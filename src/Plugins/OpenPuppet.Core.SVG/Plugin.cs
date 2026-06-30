using OpenPuppet.DataTypes.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.SVG
{
    public class Plugin : IPlugin
    {
        public string PluginID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void OnInitialized()
        {
            throw new NotImplementedException();
        }
    }
}