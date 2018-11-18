using BigTree.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree
{
    class TreeBuilder
    {
        internal static Tree Build(IEnumerable<SnContent> nodeList, IEnumerable<SnContentType> contentTypes)
        {
            var types = contentTypes.ToDictionary(x => x.Id, x => x);
            foreach(var child in types.Values)
                if (child.ParentId != 0)
                    types[child.ParentId].AddChild(child);

            var nodes = new Dictionary<int, SnContent>();
            foreach (var item in nodeList)
                nodes.Add(item.Id, item);

            foreach (var child in nodes.Values)
                if (child.ParentId != 0)
                    nodes[child.ParentId].AddChild(child);

            foreach (SnContent content in nodes.Values)
                content.ContentType = types[content.NodeType];
            foreach (SnContent content in nodes.Values)
                content.NodeType = GetVisualNodeType(content);

            foreach (var node in nodes.Values)
                node.Position = new System.Drawing.PointF(RNG.NextRandom() * 500 - 250, RNG.NextRandom() * 500 - 250);

            var root = nodes.Values.First();
            while (root.Parent != null)
                root = (SnContent)root.Parent;

            return new Tree(root, nodes);
        }

        private static int GetVisualNodeType(SnContent content)
        {
            if (UnderDocumentLibrary(content))
                return VisualNodeType.Document;
            if (content.ContentType.IsInstanceOrDerivedFrom("Workspace"))
                return VisualNodeType.Workspace;
            if (content.ContentType.Name == "PortalRoot")
                return VisualNodeType.Root;
            if (content.ContentType.Name=="ContentType")
                return VisualNodeType.ContentType;
            if (content.IsSystem)
                return VisualNodeType.System;
            return VisualNodeType.GenericContent;
        }

        private static bool UnderDocumentLibrary(SnContent content)
        {
            while (content != null)
            {
                if (content.ContentType.IsInstanceOrDerivedFrom("DocumentLibrary"))
                    return true;
                content = (SnContent)content.Parent;
            }
            return false;
        }

    }
    public class VisualNodeType
    {
        public static int GenericContent = 0;
        public static int ContentType = GenericContent + 1;
        public static int Root = ContentType + 1;
        public static int System = Root + 1;
        public static int Workspace = System + 1;
        public static int Document = Workspace + 1;
    }
}
