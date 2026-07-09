using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public static class ContextMenu
    {
        public static ContextMenuList Root { get; } = new();

        static void ProcessItem(string path, Action onClick, ContextMenuList currentNode, string? name, bool enabled)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                currentNode.Nodes.Add(
                    new ContextMenuItem {
                        Name = parts[0].ToSentenceCase(),
                        OnClick = onClick,
                        DisplayName = name ?? parts[0].ToSentenceCase(),
                        Enabled = enabled
                    }
                );
            }
            else
            {
                var nextNode = currentNode.Nodes.OfType<ContextMenuList>().FirstOrDefault(n => n.Name == parts[0].ToSentenceCase());
                if (nextNode == null)
                {
                    nextNode = new ContextMenuList {
                        Name = parts[0].ToSentenceCase(),
                        DisplayName = parts[0].ToSentenceCase()
                    };
                    currentNode.Nodes.Add(nextNode);
                }
                ProcessItem(string.Join('.', parts.Skip(1)), onClick, nextNode, name, enabled);
            }
        }

        static void ProcessList(string path, ContextMenuList currentNode, string? name, bool enabled)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                //if (separator)
                //    currentNode.Nodes.Add(new ContextMenuSeparatorItem { Name = parts[0].ToSentenceCase() });
                //else
                    currentNode.Nodes.Add(
                        new ContextMenuList {
                            Name = parts[0].ToSentenceCase(),
                            DisplayName = name ?? parts[0].ToSentenceCase(),
                            Enabled = enabled
                        }
                    );
            }
            else
            {
                var nextNode = currentNode.Nodes.OfType<ContextMenuList>().FirstOrDefault(n => n.Name == parts[0].ToSentenceCase());
                if (nextNode == null)
                {
                    nextNode = new ContextMenuList {
                        Name = parts[0].ToSentenceCase(),
                        DisplayName = parts[0].ToSentenceCase()
                    };
                    currentNode.Nodes.Add(nextNode);
                }
                ProcessList(string.Join('.', parts.Skip(1)), nextNode, name, enabled);
            }
        }

        static bool ProcessE(string path, ContextMenuList currentNode, bool enabled)
        {
            var parts = path.Split('.');
            if(parts.Length == 1)
            {
                IContextMenuNode? node = currentNode.Nodes.Find(m => m.Name == path);
                if (node == null) throw new ArgumentException($"{path} does not exist on the context menu");
                node.Enabled = enabled;
                return true;
            }
            else
            {
                foreach (var node in currentNode.Nodes)
                {
                    if(node.Name == path)
                    {
                        node.Enabled = enabled;
                        return true;
                    }
                    if (node is ContextMenuList list)
                        if (ProcessE(path, list, enabled)) return true;
                }
            }
            return false;
        }

        static bool ProcessR(string path, ContextMenuList currentNode, string name)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                IContextMenuNode? node = currentNode.Nodes.Find(m => m.Name == path);
                if (node == null) throw new ArgumentException($"{path} does not exist on the context menu");
                node.Name = name;
                return true;
            }
            else
            {
                foreach (var node in currentNode.Nodes)
                {
                    if (node.Name == path)
                    {
                        node.Name = name;
                        return true;
                    }
                    if (node is ContextMenuList list)
                        if (ProcessR(path, list, name)) return true;
                }
            }
            return false;
        }
        static bool ProcessRm(string path, ContextMenuList currentNode)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                IContextMenuNode? node = currentNode.Nodes.Find(m => m.Name == path);
                if (node == null) throw new ArgumentException($"{path} does not exist on the context menu");
                currentNode.Nodes.Remove(node);
                return true;
            }
            else
            {
                foreach (var node in currentNode.Nodes)
                {
                    if (node.Name == path)
                    {
                        currentNode.Nodes.Remove(node);
                        return true;
                    }
                    if (node is ContextMenuList list)
                        if (ProcessRm(path, list)) return true;
                }
            }
            return false;
        }

        public static void AddMenuList(string path, string? name = null, bool enabled = true) => ProcessList(path, Root, name, enabled);
        public static void AddMenuItem(string path, Action onClick, string? name = null, bool enabled = true) => ProcessItem(path, onClick, Root, name, enabled);
        //public static void AddMenuSeparator(string path) => ProcessList(path, Root, true);

        public static void SetEnabled(string path, bool enabled) => ProcessE(path, Root, enabled);
        public static void Rename(string path, string name) => ProcessR(path, Root, name);
        public static void Remove(string path) => ProcessRm(path, Root);
    }

    public interface IContextMenuNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; }
    }

    public class ContextMenuList : IContextMenuNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; } = true;
        public List<IContextMenuNode> Nodes { get; set; } = new();
    }

    public class ContextMenuItem : IContextMenuNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; } = true;
        public Action OnClick { get; set; }
    }

    public class ContextDummyItem : IContextMenuNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class ContextMenuSeparatorItem : IContextMenuNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
