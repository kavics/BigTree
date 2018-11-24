using BigTree.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree
{
    public interface INodeRenderer
    {
        void Render(TreeNode node, DrawingContext context);
    }
}
