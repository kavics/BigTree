using BigTree.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BigTree
{
    class Node : INode
    {
        public PointF Position { get; set; }
        public int NodeType { get; set; }
        public Node Parent { get; private set; }
        public List<Node> Children { get; private set; }

        public NodeCalculationState State { get; private set; }

        IEnumerable<INode> INode.Children
        {
            get { return Children; }
        }
        INode INode.Parent
        {
            get { return Parent; }
        }

        public Node()
        {
            Children = new List<Node>();
            State = new NodeCalculationState();
        }

        public void AddChild(Node node)
        {
            Children.Add(node);
            node.Parent = this;
        }
    }
}
