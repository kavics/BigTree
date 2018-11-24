using BigTree.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BigTree
{
    public class TreeNode : ITreeNode
    {
        public PointF Position { get; set; }
        public int NodeType { get; set; }
        public string Name { get; set; }
        public TreeNode Parent { get; private set; }
        public List<TreeNode> Children { get; private set; }

        public NodeCalculationState State { get; private set; }

        IEnumerable<ITreeNode> ITreeNode.Children
        {
            get { return Children; }
        }
        ITreeNode ITreeNode.Parent
        {
            get { return Parent; }
        }

        public TreeNode()
        {
            Children = new List<TreeNode>();
            State = new NodeCalculationState();
        }

        public void AddChild(TreeNode node)
        {
            Children.Add(node);
            node.Parent = this;
        }
    }
}
