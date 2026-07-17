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
    public class Vector3Drawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (Vector3)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.DragFloat3("##" + name,ref vecdata))
            {
                data = vecdata;
                return true;
            }

            return false;
        }
    }
    public class Vector2Drawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (Vector2)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.DragFloat2("##" + name, ref vecdata))
            {
                data = vecdata;
                return true;
            }

            return false;
        }
    }
    public class Vector4Drawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (Vector4)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.DragFloat4("##" + name, ref vecdata))
            {
                data = vecdata;
                return true;
            }

            return false;
        }
    }
    public class Color3Drawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (Vector3)(Color3)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.ColorEdit3("##" + name, ref vecdata))
            {
                data = (Color3)vecdata;
                return true;
            }

            return false;
        }
    }
    public class Color4Drawer : IPropertyDrawer
    {
        public bool Draw(string name, ref object data)
        {
            var vecdata = (Vector4)(Color4)data;

            ImGui.Text(name);

            ImGui.SameLine();

            if (ImGui.ColorEdit4("##" + name, ref vecdata))
            {
                data = (Color4)vecdata;
                return true;
            }

            return false;
        }
    }
}
