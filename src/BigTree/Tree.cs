using BigTreeCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree
{
    class Tree : ITree<SnContent>
    {
        public IDictionary<int, SnContent> Nodes { get; private set; }
        public SnContent Root { get; private set; }
        public TreeCalculationState State { get; private set; }

        public Tree(SnContent root, IDictionary<int, SnContent> nodes)
        {
            Root = root;
            Nodes = nodes;
            State = new TreeCalculationState();
        }
    }
}
