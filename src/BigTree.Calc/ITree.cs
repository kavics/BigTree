using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree.Calc
{
    public interface ITree<T> where T : ITreeNode
    {
        IDictionary<int, T> Nodes { get; }
        T Root { get; }

        TreeCalculationState State { get; }
    }
}
