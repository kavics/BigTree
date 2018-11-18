using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree.Calc
{
    public interface ITreeNode
    {
        PointF Position { get; set; }
        ITreeNode Parent { get; }
        IEnumerable<ITreeNode> Children { get; }

        NodeCalculationState State { get; }
    }
}
