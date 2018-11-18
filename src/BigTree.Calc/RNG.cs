using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree.Calc
{
    /// <summary>
    /// Random number generator helpers.
    /// </summary>
    public static class RNG
    {
        static Random _random = new Random();

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns></returns>
        public static float NextRandom()
        {
            return Convert.ToSingle(_random.NextDouble());
        }
    }
}
