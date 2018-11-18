using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree.Calc
{
    public static class Extensions
    {
        public static float ToSingle(this double dbl)
        {
            return Convert.ToSingle(dbl);
        }
    }
}
