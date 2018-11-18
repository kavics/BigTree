using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using BigTree.Calc;

namespace BigTree.Renderers
{
    public class DefaultRenderer : INodeRenderer
    {
        static SolidColorBrush _redBrush = new SolidColorBrush { Color = Colors.Red };
        static SolidColorBrush _yellowBrush = new SolidColorBrush { Color = Colors.Orange };
        static SolidColorBrush _blueBrush = new SolidColorBrush { Color = Colors.Blue };
        static SolidColorBrush _greenBrush = new SolidColorBrush { Color = Colors.Green };
        static SolidColorBrush _blackBrush = new SolidColorBrush { Color = Colors.Black };
        static SolidColorBrush _grayBrush = new SolidColorBrush { Color = Colors.DarkGray };
        SolidColorBrush[] _colorBrushes = new[] { _redBrush, _yellowBrush, _blueBrush, _greenBrush, _blackBrush, _grayBrush };

        public void Render(TreeNode node, DrawingContext context)
        {
            var zoom = context.Zoom;
            var size = context.GetNodeSize(node.NodeType);
            var fill = node.NodeType > _colorBrushes.Length ? _colorBrushes.Last() : _colorBrushes[node.NodeType];
            DrawEllipse(context, node.Position.X * zoom, node.Position.Y * zoom, size, size, fill);
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
    }
}
