using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BigTree
{
    class DrawingContext
    {
        public Canvas Canvas { get; private set; }
        public double X0 { get; private set; }
        public double Y0 { get; private set; }

        public DrawingContext(Canvas canvas)
        {
            Canvas = canvas;
            X0 = canvas.ActualWidth / 2;
            Y0 = canvas.ActualHeight / 2;
        }

        public float GetNodeSize(int nodeType)
        {
            switch (nodeType)
            {
                case 0: return 16; // root
                case 1: return 12;  // big node
                case 2: return 9;  // node
                default: return 5; // leaf?
            }
        }
    }
}
