using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BigTree.Renderers
{
    public class Renderer1 : INodeRenderer
    {
        static SolidColorBrush _blackBrush = new SolidColorBrush { Color = Colors.Black };

        public void Render(TreeNode node, DrawingContext context)
        {
            var zoom = context.Zoom;
            var size = context.GetNodeSize(node.NodeType);
            DrawEllipse(context, node.Position.X * zoom, node.Position.Y * zoom, 4, 4);
            DrawText(context, node.Name, node.Position.X * zoom, node.Position.Y * zoom);
        }
        private void DrawEllipse(DrawingContext ctx, float x, float y, float width, float height)
        {
            ctx.Canvas.Children.Add(new Ellipse
            {
                Width = width,
                Height = height,
                Margin = new Thickness(x - width / 2 + ctx.X0, y - height / 2 + ctx.Y0, 0, 0),
                StrokeThickness = 1,
                Stroke = _blackBrush,
                Fill = _blackBrush,
            });
        }
        private void DrawText(DrawingContext ctx, string text, float x, float y)
        {
            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = _blackBrush;

            var width = textBlock.Width;
            var height = textBlock.Height;

            //textBlock.Margin = new Thickness(x - 100 / 2 + ctx.X0, y - 32 / 2 + ctx.Y0, 0, 0);
            textBlock.Margin = new Thickness(x + ctx.X0, y + ctx.Y0, 0, 0);

            Canvas.SetLeft(textBlock, 0);
            Canvas.SetTop(textBlock, 0);
            ctx.Canvas.Children.Add(textBlock);
        }
    }
}
