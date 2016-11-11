using BigTreeCalc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BigTree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            ForceMax = "";
            mainGrid.DataContext = this;
        }

        static SolidColorBrush _redBrush = new SolidColorBrush { Color = Colors.Red };
        static SolidColorBrush _yellowBrush = new SolidColorBrush { Color = Colors.Orange };
        static SolidColorBrush _blueBrush = new SolidColorBrush { Color = Colors.Blue };
        static SolidColorBrush _greenBrush = new SolidColorBrush { Color = Colors.Green };
        static SolidColorBrush _blackBrush = new SolidColorBrush { Color = Colors.Black };
        static SolidColorBrush _grayBrush = new SolidColorBrush { Color = Colors.DarkGray };
        SolidColorBrush[] _colorBrushes = new[] { _redBrush, _yellowBrush, _blueBrush, _greenBrush, _blackBrush, _grayBrush };

        private Tree _tree;

        private int _iteration;
        public int Iteration
        {
            get { return _iteration; }
            set
            {
                if (value != _iteration)
                {
                    _iteration = value;
                    OnPropertyChanged("Iteration");
                }
            }
        }

        private string _forceMax;
        public string ForceMax
        {
            get { return _forceMax; }
            set
            {
                if (value != _forceMax)
                {
                    _forceMax = value;
                    OnPropertyChanged("ForceMax");
                }
            }
        }

        private string _calcTime;
        public string CalcTime
        {
            get { return _calcTime; }
            set
            {
                if (value != _calcTime)
                {
                    _calcTime = value;
                    OnPropertyChanged("CalcTime");
                }
            }
        }

        private string _drawTime;
        public string DrawTime
        {
            get { return _drawTime; }
            set
            {
                if (value != _drawTime)
                {
                    _drawTime = value;
                    OnPropertyChanged("DrawTime");
                }
            }
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private Tree CreateTree()
        {
            var nodes = new TreeReader<SnContent>(
                new StreamReader(@"D:\Projects\Source\Repos\First\BigTree\BigTree\_Nodes_smalltree.txt"), SnContent.Parse)
                .ToList();
            var types = new TreeReader<SnContentType>(
                new StreamReader(@"D:\Projects\Source\Repos\First\BigTree\BigTree\_Types.txt"), SnContentType.Parse)
                .ToList();
            return TreeBuilder.Build(nodes, types);

            //var root = new Node { Position = new System.Drawing.PointF( 0, 0), NodeType = 0 };
            //var tree = new Tree(root);
            //tree.CreateNode(root, 100, -80, 2);
            //tree.CreateNode(root, -80, 100, 2);
            //tree.CreateNode(root, -60, -120, 2);
            //var n = tree.CreateNode(root, 130, 40, 1);
            //tree.CreateNode(n, 190, 20, 2);
            //tree.CreateNode(n, 170, 90, 2);
            //tree.CreateNode(n, 110, 110, 2);
            //return tree;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tree = CreateTree();
            Redraw(_tree.Root);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Recalc(_tree);
            this.ForceMax = _tree.State.ForceMax.ToString("n3");
            Redraw(_tree.Root);
        }

        private void Recalc(Tree tree)
        {
            var timer = Stopwatch.StartNew();
            Calc<SnContent>.NextState(tree);
            timer.Stop();
            Iteration++;
            CalcTime = timer.Elapsed.ToString();
        }

        private void Redraw(Node node)
        {
            var timer = Stopwatch.StartNew();

            var active = canvas1.IsVisible ? canvas1 : canvas2;
            var inactive = canvas1.IsVisible ? canvas2 : canvas1;

            inactive.Children.Clear();
            DrawTree(new DrawingContext(inactive), node);

            inactive.Visibility = Visibility.Visible;
            active.Visibility = Visibility.Hidden;

            timer.Stop();
            DrawTime = timer.Elapsed.ToString();
        }

        private void DrawTree(DrawingContext ctx, Node root)
        {
            foreach (var child in root.Children)
            {
                DrawLine(ctx, root.Position, child.Position);
                DrawTree(ctx, child);
            }
            DrawNode(ctx, root);
        }
        private void DrawNode(DrawingContext ctx, Node node)
        {
            var size = ctx.GetNodeSize(node.NodeType);
            var fill = node.NodeType > _colorBrushes.Length ? _colorBrushes.Last() : _colorBrushes[node.NodeType];
            DrawEllipse(ctx, node.Position.X, node.Position.Y, size, size, fill);
        }
        private void DrawEllipse(DrawingContext ctx, float x, float y, float width, float height, Brush fill)
        {
            ctx.Canvas.Children.Add(new Ellipse
            {
                Width = width,
                Height = height,
                Margin = new Thickness(x - width / 2 + ctx.X0, y - height / 2 + ctx.Y0, 0, 0),
                StrokeThickness = 1,
                Stroke = fill, // _blackBrush,
                Fill = fill,
            });
        }
        private void DrawLine(DrawingContext ctx, System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            var line = new Line
            {
                X1 = p1.X + ctx.X0,
                Y1 = p1.Y + ctx.Y0,
                X2 = p2.X + ctx.X0,
                Y2 = p2.Y + ctx.Y0,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                SnapsToDevicePixels = true,
            };
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            ctx.Canvas.Children.Add(line);
        }

    }
}
