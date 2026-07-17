using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.PropertyDrawers
{
    public class FloatDrawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (float)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.DragFloat("##" + name, ref vecdata))
            {
                data = vecdata;
                return true;
            }

            return false;
        }
    }
    public class IntDrawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (int)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.DragInt("##" + name, ref vecdata))
            {
                data = vecdata;
                return true;
            }

            return false;
        }
    }
}
