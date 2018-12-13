using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree.Calc
{
    public class Calc2<T> where T : ITreeNode
    {
        const float FORCELIMIT = 10.0f;

        static float _repulsionPow = -2; //UNDONE: move to the tree state
        static float _repulsionMul = 500; //UNDONE: move to the tree state
        static float _rubberPow = 2; //UNDONE: move to the tree state
        static float _rubberMul = 800; //UNDONE: move to the tree state

        static float _dampingFactor = 0.40f; // Reduces oscillation

        public static void NextState(ITree<T> tree)
        {
            tree.State.ForceMax = 0;
            tree.State.RepulsionMax = 0;
            tree.State.RubberMax = 0;

            //var timer = Stopwatch.StartNew();
            CalculateRepulsion(tree);
            //Debug.WriteLine("Repulsion: " + timer.Elapsed);
            //timer.Reset();
            CalculateRubber(tree);
            //Debug.WriteLine("Rubber:    " + timer.Elapsed);
            //timer.Reset();
            CalculateNextPosition(tree);
            //Debug.WriteLine("Position:  " + timer.Elapsed);
            //timer.Stop();
        }

        private static void CalculateRepulsion(ITree<T> tree)
        {
            var nodes = tree.Nodes.Values.ToArray();
            //for (int i = 0; i < nodes.Length - 1; i++)
            //    for (int j = i + 1; j < nodes.Length; j++)
            //        CalculateRepulsion(nodes[i], nodes[j]);
            Parallel.For(0, nodes.Length, i =>
            {
                for (int j = i + 1; j < nodes.Length; j++)
                    CalculateRepulsion(nodes[i], nodes[j]);
            });
        }
        private static void CalculateRepulsion(ITreeNode n1, ITreeNode n2)
        {
            if (n1 != n2)
            {
                var dx = n2.Position.X - n1.Position.X;
                var dy = n2.Position.Y - n1.Position.Y;
                if (dx > 80 || dy > 80) // calculation is enabled only in the bounding box
                    return;
                var r = Math.Sqrt(dx * dx + dy * dy).ToSingle();
                if (r == 0.0)
                {
                    dx += (RNG.NextRandom() - 0.5f) / 100000.0f;
                    dy += (RNG.NextRandom() - 0.5f) / 100000.0f;
                    r = Math.Sqrt(dx * dx + dy * dy).ToSingle();
                }
                var f = Math.Pow(r, _repulsionPow).ToSingle() * _repulsionMul;
                var df = f / r;
                PointF f1 = new PointF(-df * dx, -df * dy);
                PointF f2 = new PointF(df * dx, df * dy);

                lock (n1.State) //???
                    n1.State.RepulsionForce = new PointF(n1.State.RepulsionForce.X + f1.X, n1.State.RepulsionForce.Y + f1.Y);
                lock (n2.State) //???
                    n2.State.RepulsionForce = new PointF(n2.State.RepulsionForce.X + f2.X, n2.State.RepulsionForce.Y + f2.Y);
            }
        }

        private static void CalculateRubber(ITree<T> tree)
        {
            //foreach (var node in tree.Nodes.Values)
            //    if (node.Parent != null)
            //        CalculateRubber(node, node.Parent);
            Parallel.ForEach<T>(tree.Nodes.Values, new ParallelOptions { MaxDegreeOfParallelism = 4 }, CalculateRubber); //???
        }
        private static void CalculateRubber(T n1)
        {
            var n2 = n1.Parent;
            if (n2 == null || (ITreeNode)n1 == n2)
                return;

            var dx = n2.Position.X - n1.Position.X;
            var dy = n2.Position.Y - n1.Position.Y;
            var r = Math.Sqrt(dx * dx + dy * dy);
            var f = Math.Pow(r, _rubberPow) / _rubberMul;
            var df = f / r;
            var f1 = new PointF((df * dx).ToSingle(), (df * dy).ToSingle());
            var f2 = new PointF((-df * dx).ToSingle(), (-df * dy).ToSingle());

            lock(n1) //???
                n1.State.RubberForce = new PointF(n1.State.RubberForce.X + f1.X, n1.State.RubberForce.Y + f1.Y);
            lock(n2) //???
                n2.State.RubberForce = new PointF(n2.State.RubberForce.X + f2.X, n2.State.RubberForce.Y + f2.Y);
        }
        private static void CalculateNextPosition(ITree<T> tree)
        {
            var tState = tree.State;
            foreach (var node in tree.Nodes.Values)
                CalculateNextPosition(node, tState);
        }
        private static void CalculateNextPosition(T node, TreeCalculationState tState)
        {
            var nState = node.State;

            float dx = nState.RepulsionForce.X + nState.RubberForce.X;
            float dy = nState.RepulsionForce.Y + nState.RubberForce.Y;

            Statistics(tState, nState, dx, dy);

            if (dx > FORCELIMIT) dx = FORCELIMIT;
            if (dy > FORCELIMIT) dy = FORCELIMIT;
            if (dx < -FORCELIMIT) dx = -FORCELIMIT;
            if (dy < -FORCELIMIT) dy = -FORCELIMIT;

            var nextPosition = new PointF(node.Position.X + dx * _dampingFactor, node.Position.Y + dy * _dampingFactor);
            node.Position = nextPosition;
        }
        private static void Statistics(TreeCalculationState tState, NodeCalculationState nState, float dx, float dy)
        {
            var repulsion = Math.Max(Math.Abs(nState.RepulsionForce.X), Math.Abs(nState.RepulsionForce.X));
            if (repulsion > tState.RepulsionMax)
                lock(tState) //???
                    tState.RepulsionMax = repulsion;

            var rubber = Math.Max(Math.Abs(nState.RubberForce.X), Math.Abs(nState.RubberForce.X));
            if (rubber > tState.RubberMax)
                lock (tState) //???
                    tState.RubberMax = rubber;

            var force = Math.Max( Math.Abs(dx), Math.Abs(dy));
            if (force > tState.ForceMax)
                lock (tState) //???
                    tState.ForceMax = force;

            nState.RepulsionForce = PointF.Empty;
            nState.RubberForce = PointF.Empty;
        }
    }
}
