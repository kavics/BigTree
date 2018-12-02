using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using BigTree;
using BigTree.Calc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BigTreeTests
{
    [TestClass]
    public class UnitTest1
    {
        private class Node : ITreeNode
        {
            public PointF Position { get; set; }
            public int Id { get; private set; }
            public int ParentId { get; private set; }
            //public int NodeType { get; set; }
            public string Name { get; private set; }
            public Node Parent { get; private set; }
            public List<Node> Children { get; private set; }

            public NodeCalculationState State { get; private set; }

            IEnumerable<ITreeNode> ITreeNode.Children
            {
                get { return Children; }
            }
            ITreeNode ITreeNode.Parent
            {
                get { return Parent; }
            }

            public Node()
            {
                Children = new List<Node>();
                State = new NodeCalculationState();
            }

            public void AddChild(Node node)
            {
                Children.Add(node);
                node.Parent = this;
            }


            private static readonly char[] _split = "\t,;".ToCharArray();

            public static Node Parse(string src)
            {
                // NodeId   ParentNodeId    IsSystem    NodeTypeId  Type    Name
                var result = new Node();

                var cols = src.Split(_split, StringSplitOptions.RemoveEmptyEntries);
                if (cols[1] == "NULL") cols[1] = "0";

                int id;
                if (!int.TryParse(cols[0], out id)) return null; result.Id = id;
                if (!int.TryParse(cols[1], out id)) return null; result.ParentId = id;
                result.Name = cols[2];

                return result;
            }
        }
        class Tree : ITree<Node>
        {
            public IDictionary<int, Node> Nodes { get; private set; }
            public Node Root { get; private set; }
            public TreeCalculationState State { get; private set; }

            public Tree(Node root, IDictionary<int, Node> nodes)
            {
                Root = root;
                Nodes = nodes;
                State = new TreeCalculationState();
            }
        }

        private class TreeBuilder
        {
            internal static Tree Build(IEnumerable<Node> nodeList)
            {
                var nodes = nodeList.ToDictionary(x => x.Id, x => x);

                foreach (var child in nodes.Values)
                    if (child.ParentId != 0)
                        nodes[child.ParentId].AddChild(child);

                foreach (var node in nodes.Values)
                    node.Position = new PointF(RNG.NextRandom() * 500 - 250, RNG.NextRandom() * 500 - 250);

                var root = nodes.Values.First();
                while (root.Parent != null)
                    root = root.Parent;

                return new Tree(root, nodes);
            }
        }



        [TestMethod]
        public void Performance1()
        {
            var forceEndPoint = 0.08f;
            var count = 40;
            var times = new TimeSpan[count];
            var iterations = new int[count];

            for (int i = 0; i < count; i++)
            {
                var start = DateTime.Now;
                var tree = CreateTree();
                var iteration = 0;

                while (true)
                {
                    Calc<Node>.NextState(tree);
                    iteration++;

                    var f = tree.State.ForceMax;
                    if (0 < f && f < forceEndPoint)
                        break;
                }
                var time = DateTime.Now - start;
                times[i] = time;
                iterations[i] = iteration;
                Debug.WriteLine($"{i + 1}\t{iteration}\t{time}");
            }
            PrintAverage("Performance1", iterations, times);
        }
        [TestMethod]
        public void Performance2()
        {
            var forceEndPoint = 0.08f;
            var count = 40;
            var times = new TimeSpan[count];
            var iterations = new int[count];

            for (int i = 0; i < count; i++)
            {
                var start = DateTime.Now;
                var tree = CreateTree();
                var iteration = 0;

                while (true)
                {
                    Calc2<Node>.NextState(tree);
                    iteration++;

                    var f = tree.State.ForceMax;
                    if (0 < f && f < forceEndPoint)
                        break;
                }
                var time = DateTime.Now - start;
                times[i] = time;
                iterations[i] = iteration;
                Debug.WriteLine($"{i + 1}\t{iteration}\t{time}");
            }
            PrintAverage("Performance2", iterations, times);
        }

        /* =========================================================================== */

        private void PrintAverage(string prefix, int[] iterations, TimeSpan[] times)
        {
            var Iavg = iterations.Skip(3).Average();
            var Tavg = TimeSpan.FromTicks(Convert.ToInt64(times.Select(t => t.Ticks).Skip(3).Average()));
            Debug.WriteLine($"{prefix}\t{Iavg}\t{Tavg}");
        }

        private ITree<Node> CreateTree()
        {
            var src = @"NodeId	ParentNodeId	Name
1000	NULL	System
1001	1000	Schema
1002	1001	ContentTypes
1003	1000	Settings
1004	1002	ContentType
1005	1002	GenericContent
1006	1005	Folder
1007	1006	ADFolder
1008	1005	File
1009	1008	Settings
1010	1009	ADSettings
1011	1008	SystemFile
1012	1011	ApplicationCacheFile
1013	1005	Application
1014	1013	ApplicationOverride
1015	1005	Workflow
1016	1015	ApprovalWorkflow
1017	1005	ListItem
1018	1017	Task
1019	1018	ApprovalWorkflowTask
1020	1017	WebContent
1021	1020	Article
1022	1006	ArticleSection
1023	1006	ContentList
1024	1023	Aspect";

            var nodes = new TreeReader<Node>(new StringReader(src), Node.Parse)
                .ToList();

            return TreeBuilder.Build(nodes);
        }
    }
}
