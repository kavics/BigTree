using BigTreeCalc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using IO = System.IO;

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
            OffsetXText = "0";
            OffsetYText = "0";
        }

        static SolidColorBrush _redBrush = new SolidColorBrush { Color = Colors.Red };
        static SolidColorBrush _yellowBrush = new SolidColorBrush { Color = Colors.Orange };
        static SolidColorBrush _blueBrush = new SolidColorBrush { Color = Colors.Blue };
        static SolidColorBrush _greenBrush = new SolidColorBrush { Color = Colors.Green };
        static SolidColorBrush _blackBrush = new SolidColorBrush { Color = Colors.Black };
        static SolidColorBrush _grayBrush = new SolidColorBrush { Color = Colors.DarkGray };
        SolidColorBrush[] _colorBrushes = new[] { _redBrush, _yellowBrush, _blueBrush, _greenBrush, _blackBrush, _grayBrush };

        private Tree _tree;

        private string _offsetXText;
        public string OffsetXText
        {
            get { return _offsetXText; }
            set
            {
                if (value != _offsetXText)
                {
                    _offsetXText = value;
                    double d = 0.0;
                    double.TryParse(value, out d);
                    OffsetX = d;
                    OnPropertyChanged(nameof(OffsetXText));
                }
            }
        }
        public double OffsetX { get; set; }

        private string _offsetYText;
        public string OffsetYText
        {
            get { return _offsetYText; }
            set
            {
                if (value != _offsetYText)
                {
                    _offsetYText = value;
                    double d = 0.0;
                    double.TryParse(value, out d);
                    OffsetY = d;
                    OnPropertyChanged(nameof(OffsetYText));
                }
            }
        }
        public double OffsetY { get; set; }

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

        private bool _paused;
        public bool Paused
        {
            get { return _paused; }
            set
            {
                if (value != _paused)
                {
                    _paused = value;
                    OnPropertyChanged("Paused");
                }
            }
        }

        private string _startButtonText;
        public string StartButtonText
        {
            get { return _startButtonText; }
            set
            {
                if (value != _startButtonText)
                {
                    _startButtonText = value;
                    OnPropertyChanged("StartButtonText");
                }
            }
        }

        private string _quickProperties;
        public string QuickProperties
        {
            get { return _quickProperties; }
            set
            {
                if (value != _startButtonText)
                {
                    _quickProperties = value;
                    OnPropertyChanged("QuickProperties");
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
            if (Paused)
                Continue();
            else
                Pause();
        }
        private void Pause()
        {
            Paused = true;
            StartButtonText = "||";
        }
        private void Continue()
        {
            Paused = false;
            StartButtonText = ">";
        }


        private Tree CreateTree()
        {
            var directory = IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(
                    AppDomain.CurrentDomain.BaseDirectory)));

            var nodes = new TreeReader<SnContent>(
                new StreamReader(IO.Path.Combine(directory, "_Nodes_smalltree.txt")), SnContent.Parse)
                .ToList();

            var types = new TreeReader<SnContentType>(
                new StreamReader(IO.Path.Combine(directory, "_Types.txt")), SnContentType.Parse)
                .ToList();

            return TreeBuilder.Build(nodes, types);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tree = CreateTree();
            Redraw(_tree.Root);

            Continue();

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
            if (_paused)
                return;

            var timer = Stopwatch.StartNew();

            var active = canvas1.IsVisible ? canvas1 : canvas2;
            var inactive = canvas1.IsVisible ? canvas2 : canvas1;

            inactive.Children.Clear();
            var ctx = new DrawingContext(inactive, OffsetX, OffsetY);
            DrawHairLines(ctx);
            DrawTree(ctx, node);

            inactive.Visibility = Visibility.Visible;
            active.Visibility = Visibility.Hidden;

            timer.Stop();
            DrawTime = timer.Elapsed.ToString();
        }

        private void DrawHairLines(DrawingContext ctx)
        {
            var width = ctx.Canvas.ActualWidth.ToSingle();
            var height = ctx.Canvas.ActualHeight.ToSingle();
            var x0 = ctx.X0.ToSingle();
            var y0 = ctx.Y0.ToSingle();
            DrawLine(ctx, new System.Drawing.PointF(-width - x0, 0.0f), new System.Drawing.PointF(width - x0, 0.0f), Brushes.Gray);
            DrawLine(ctx, new System.Drawing.PointF(0.0f, -height - y0), new System.Drawing.PointF(0.0f, height - y0), Brushes.Gray);
        }
        private void DrawTree(DrawingContext ctx, Node root)
        {
            foreach (var child in root.Children)
            {
                DrawLine(ctx, root.Position, child.Position, Brushes.Black);
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
        private void DrawLine(DrawingContext ctx, System.Drawing.PointF p1, System.Drawing.PointF p2, SolidColorBrush color)
        {
            var line = new Line
            {
                X1 = p1.X + ctx.X0,
                Y1 = p1.Y + ctx.Y0,
                X2 = p2.X + ctx.X0,
                Y2 = p2.Y + ctx.Y0,
                Stroke = color,
                StrokeThickness = 1,
                SnapsToDevicePixels = true,
            };
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            ctx.Canvas.Children.Add(line);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(canvas1);
            var mx = mousePosition.X - canvas1.ActualWidth / 2 - OffsetX;
            var my = mousePosition.Y - canvas1.ActualHeight / 2 - OffsetY;

            var content = SearchContentByPosition(_tree.Root, mx, my) as SnContent;

            //QuickProperties = $"[{mx},{my}]: {content?.Name ?? string.Empty}";
            QuickProperties = content?.Name ?? string.Empty;
        }

        private INode SearchContentByPosition(INode node, double mx, double my)
        {
            if (Math.Abs(node.Position.X - mx) < 4 && Math.Abs(node.Position.Y - my) < 4)
                return node;
            INode result;
            foreach (var child in node.Children)
                if ((result = SearchContentByPosition(child, mx, my)) != null)
                    return result;
            return null;
        }
    }
}
