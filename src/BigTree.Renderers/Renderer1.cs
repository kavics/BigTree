using System;
using System.Collections.Generic;
using System.Globalization;
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
        static readonly Typeface DefaultTypeface = new Typeface(
            new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        //static SolidColorBrush _blackBrush = new SolidColorBrush { Color = Colors.Black };
        //static SolidColorBrush _yellowBrush = new SolidColorBrush { Color = Colors.LightBlue, Opacity = 1.0d };
        //static SolidColorBrush _whiteBrush = new SolidColorBrush { Color = Colors.White, Opacity = 100 };

        public void Render(TreeNode node, DrawingContext context)
        {
            var zoom = context.Zoom;
            var size = context.GetNodeSize(node.NodeType);
            // DrawEllipse(context, node.Position.X * zoom, node.Position.Y * zoom, 4, 4);
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
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
            });
        }
        private void DrawText(DrawingContext ctx, string text, float x, float y)
        {
            var fontSize = ctx.ItemSize * ctx.Zoom;

            Border border1 = new Border();
            border1.Background = Brushes.LightBlue;
            border1.BorderBrush = Brushes.Black;
            border1.BorderThickness = new Thickness(1);

            var textBlock = new TextBlock();
            textBlock.FontSize = fontSize;
            textBlock.Text = text;
            textBlock.Foreground = Brushes.Black;

            var textBlockSize = GetTextSize(text, fontSize, DefaultTypeface);
            var width = textBlockSize.Width;
            var height = textBlockSize.Height;

            border1.Margin = new Thickness(x + ctx.X0 - width/2, y + ctx.Y0 - height/2, 0, 0);
            border1.Child = textBlock;

            Canvas.SetLeft(border1, 0);
            Canvas.SetTop(border1, 0);
            ctx.Canvas.Children.Add(border1);
        }
        private Size GetTextSize(string text, double fontSize, Typeface typeface)
        {
            var formattedText = new FormattedText(
                   text,
                   CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight,
                   typeface,
                   fontSize,
                   Brushes.Black,
                   new NumberSubstitution(), TextFormattingMode.Display);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
