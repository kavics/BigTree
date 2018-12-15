using BigTree.Calc;
using SenseNet.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
        private TraceWindow _traceWindow = new TraceWindow();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            _traceWindow.ForceMax = "";
            mainGrid.DataContext = this;
            OffsetXText = "0";
            OffsetYText = "0";
            ZoomText = "100";
            ItemSizeText = "6";
        }

        private Tree _tree;
        private INodeRenderer _nodeRenderer;

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
                    double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out d);
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
                    double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out d);
                    OffsetY = d;
                    OnPropertyChanged(nameof(OffsetYText));
                }
            }
        }
        public double OffsetY { get; set; }

        private string _zoomText;
        public string ZoomText
        {
            get { return _zoomText; }
            set
            {
                if (value != _zoomText)
                {
                    _zoomText = value;
                    double d;
                    if (!double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out d))
                        d = 100d;
                    Zoom = d / 100d;
                    OnPropertyChanged(nameof(ZoomText));
                }
            }
        }
        public double Zoom { get; set; }

        private string _itemSizeText;
        public string ItemSizeText
        {
            get { return _itemSizeText; }
            set
            {
                if (value != _itemSizeText)
                {
                    _itemSizeText = value;
                    double d;
                    if (!double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out d))
                        d = 6d;
                    ItemSize = d;
                    OnPropertyChanged(nameof(ItemSizeText));
                }
            }
        }
        public double ItemSize { get; set; }


        private const float ForceEndPoint = 0.08f;

        private DateTime _startTime;

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
                new StreamReader(IO.Path.Combine(directory, "_nodes_smalltree.txt")), SnContent.Parse)
                .ToList();

            var types = new TreeReader<SnContentType>(
                new StreamReader(IO.Path.Combine(directory, "_Types.txt")), SnContentType.Parse)
                .ToList();

            return TreeBuilder.Build(nodes, types);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tree = CreateTree();

            var rendererTypes = TypeResolver.GetTypesByInterface(typeof(INodeRenderer))
                .ToDictionary(t => t.Name, t => t);
            _nodeRenderer = (INodeRenderer)Activator.CreateInstance(rendererTypes["Renderer1"]);

            // Zoom = 3.0d;
            RecalcAsync(_tree);
            Redraw(_tree.Root);

            Continue();

            _startTime = DateTime.Now;

            DispatcherTimer measuringTimer = new DispatcherTimer();
            measuringTimer.Tick += new EventHandler(measuringTimer_Tick);
            measuringTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            measuringTimer.Start();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();

            _traceWindow.Show();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _traceWindow.ForceMax = _tree.State.ForceMax.ToString("n3");
            Redraw(_tree.Root);
        }
        private void measuringTimer_Tick(object sender, EventArgs e)
        {
            var time = DateTime.Now - _startTime;
            _traceWindow.MeasuringTimeText = time.ToString();
            var f = _tree.State.ForceMax;
            if (0 < f && f < ForceEndPoint)
                ((DispatcherTimer)sender)?.Stop();
        }

        private async Task RecalcAsync(Tree tree)
        {
            while (true)
            {
                if (!_paused)
                    await Task.Run(() => { RecalcOne(tree); });
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
        private void RecalcOne(Tree tree)
        {
            var timer = Stopwatch.StartNew();
            Calc<SnContent>.NextState(tree);
            timer.Stop();
            _traceWindow.Iteration++;
            _traceWindow.CalcTime = timer.Elapsed.ToString();
        }

        private void Redraw(TreeNode node)
        {
            if (_paused)
                return;

            var timer = Stopwatch.StartNew();

            var active = canvas1.IsVisible ? canvas1 : canvas2;
            var inactive = canvas1.IsVisible ? canvas2 : canvas1;

            var ctx = new DrawingContext(inactive, OffsetX, OffsetY, Zoom.ToSingle(), ItemSize.ToSingle());

            inactive.Children.Clear();
            DrawHairLines(ctx);
            DrawTree(ctx, node);

            inactive.Visibility = Visibility.Visible;
            active.Visibility = Visibility.Hidden;

            timer.Stop();
            _traceWindow.DrawTime = timer.Elapsed.ToString();
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
        private void DrawTree(DrawingContext ctx, TreeNode root)
        {
            foreach (var child in root.Children)
            {
                DrawLine(ctx, root.Position, child.Position, Brushes.Black);
                DrawTree(ctx, child);
            }
            DrawNode(ctx, root);
        }
        private void DrawNode(DrawingContext ctx, TreeNode node)
        {
            _nodeRenderer.Render(node, ctx);
        }
        private void DrawLine(DrawingContext ctx, System.Drawing.PointF p1, System.Drawing.PointF p2, SolidColorBrush color)
        {
            var zoom = ctx.Zoom;
            var line = new Line
            {
                X1 = p1.X * zoom + ctx.X0,
                Y1 = p1.Y * zoom + ctx.Y0,
                X2 = p2.X * zoom + ctx.X0,
                Y2 = p2.Y * zoom + ctx.Y0,
                Stroke = color,
                StrokeThickness = 1,
                SnapsToDevicePixels = true,
            };
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            ctx.Canvas.Children.Add(line);
        }

        private bool _offsetModeEnabled;
        private object _offsetLock = new object();
        private double _offsetStartX;
        private double _offsetStartY;

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_offsetModeEnabled)
            {
                var mousePosition = e.GetPosition(canvas1);
                var mx = mousePosition.X - canvas1.ActualWidth / 2 - OffsetX;
                var my = mousePosition.Y - canvas1.ActualHeight / 2 - OffsetY;

                // FIND NODE BY POSITION
                var content = SearchContentByPosition(_tree.Root, mx / Zoom, my / Zoom) as SnContent;

                //QuickProperties = $"[{mx},{my}]: {content?.Name ?? string.Empty}";
                QuickProperties = content?.Name ?? string.Empty;
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            _traceWindow.MouseCoords = new Point
                ((mousePosition.X - canvas1.ActualWidth / 2 - OffsetX)/Zoom,
                 (mousePosition.Y - canvas1.ActualHeight / 2 - OffsetY)/Zoom);

            if (!_offsetModeEnabled)
                return;

            lock (_offsetLock)
            {
                var mx = mousePosition.X;
                var my = mousePosition.Y;

                // MOVE SCREEN BY MOUSE DRAG
                if (double.IsNaN(_offsetStartX))
                {
                    _offsetStartX = mx - OffsetX;
                    _offsetStartY = my - OffsetY;
                    Debug.WriteLine("Reset");
                }
                else
                {
                    var dx = (mx - _offsetStartX);
                    var dy = (my - _offsetStartY);

                    OffsetXText = dx.ToString(CultureInfo.CurrentCulture);
                    OffsetYText = dy.ToString(CultureInfo.CurrentCulture);
                    _traceWindow.CenterCoords = new Point(dx, dy);
                    //_traceWindow.MouseCoords = new Point(mx, my);
                }
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                _offsetModeEnabled = true;
                _offsetStartX = double.NaN;
                _offsetStartY = double.NaN;
            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState != MouseButtonState.Pressed)
            {
                _offsetModeEnabled = false;
                _offsetStartX = double.NaN;
                _offsetStartY = double.NaN;
            }
        }

        private ITreeNode SearchContentByPosition(ITreeNode node, double mx, double my)
        {
            if (Math.Abs(node.Position.X - mx) < 4 && Math.Abs(node.Position.Y - my) < 4)
                return node;
            ITreeNode result;
            foreach (var child in node.Children)
                if ((result = SearchContentByPosition(child, mx, my)) != null)
                    return result;
            return null;
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateZoomText(20);
        }
        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateZoomText(-20);
        }
        private void UpdateZoomText(int delta)
        {
            var zoom = Convert.ToInt32(Zoom * 100 + delta);
            zoom = zoom / 10;
            zoom = zoom * 10;
            zoom = Math.Max(zoom, 10);
            ZoomText = zoom.ToString();
        }
        private void sizePlusButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateItemSizeText(0.5);
        }

        private void sizeMinusButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateItemSizeText(-0.5);
        }
        private void UpdateItemSizeText(double delta)
        {
            var itemSize = ItemSize + delta;
            itemSize = Math.Max(itemSize, 1.0d);
            ItemSizeText = itemSize.ToString();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Debug.WriteLine($"Wheel: {e.Delta}");
            UpdateZoomText(Math.Sign(e.Delta) * 20);

            var sign = Math.Sign(e.Delta);

            var mousePosition = e.GetPosition(this);
            var mx = (mousePosition.X - canvas1.ActualWidth / 2 - OffsetX) / Zoom;
            var my = (mousePosition.Y - canvas1.ActualHeight / 2 - OffsetY) / Zoom;

            var dx = mx / 5;
            var dy = my / 5;

            OffsetXText = (OffsetX - sign * dx).ToString(CultureInfo.CurrentCulture);
            OffsetYText = (OffsetY - sign * dy).ToString(CultureInfo.CurrentCulture);
        }

        private void traceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_traceWindow.IsVisible)
                _traceWindow.Hide();
            else
                _traceWindow.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _traceWindow.Exit();
            _traceWindow.Close();
        }

    }
}
