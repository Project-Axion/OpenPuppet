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

        static void ProcessItem(string path, Action onClick, ContextMenuList currentNode)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                currentNode.Nodes.Add(new ContextMenuItem { Name = parts[0].ToSentenceCase(), OnClick = onClick });
            }
            else
            {
                var nextNode = currentNode.Nodes.OfType<ContextMenuList>().FirstOrDefault(n => n.Name == parts[0].ToSentenceCase());
                if (nextNode == null)
                {
                    nextNode = new ContextMenuList { Name = parts[0].ToSentenceCase() };
                    currentNode.Nodes.Add(nextNode);
                }
                ProcessItem(string.Join('.', parts.Skip(1)), onClick, nextNode);
            }
        }

        static void ProcessDummyItem(string path, ContextMenuList currentNode)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                currentNode.Nodes.Add(new ContextDummyItem { Name = parts[0].ToSentenceCase() });
            }
            else
            {
                var nextNode = currentNode.Nodes.OfType<ContextMenuList>().FirstOrDefault(n => n.Name == parts[0].ToSentenceCase());
                if (nextNode == null)
                {
                    nextNode = new ContextMenuList { Name = parts[0].ToSentenceCase() };
                    currentNode.Nodes.Add(nextNode);
                }
                ProcessDummyItem(string.Join('.', parts.Skip(1)), nextNode);
            }
        }

        public static void AddMenuDummy(string path) => ProcessDummyItem(path, Root);
        public static void AddMenuItem(string path, Action onClick) => ProcessItem(path, onClick, Root);
    }

    public interface IContextMenuNode
    {
        public string Name { get; set; }
    }

    public class ContextMenuList : IContextMenuNode
    {
        public string Name { get; set; }
        public List<IContextMenuNode> Nodes { get; set; } = new();
    }

    public class ContextMenuItem : IContextMenuNode
    {
        public string Name { get; set; }
        public Action OnClick { get; set; }
    }

    public class ContextDummyItem : IContextMenuNode
    {
        public string Name { get; set; }
    }
}
