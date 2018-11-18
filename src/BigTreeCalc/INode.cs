using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTreeCalc
{
    public interface INode
    {
        PointF Position { get; set; }
        INode Parent { get; }
        IEnumerable<INode> Children { get; }

        NodeCalculationState State { get; }
    }
}
