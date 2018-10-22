using System;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    class VerticalChildAdapter : IChildAdapter
    {
        public UIElement Child { get; }

        public FlexSize DesiredSize => new FlexSize(longitudinal: Child.DesiredSize.Height, lateral: Child.DesiredSize.Width);

        public VerticalChildAdapter(UIElement child)
        {
            Child = child;
        }

        public void Measure(FlexSize availableSize)
        {
            var size = new Size(availableSize.Lateral, availableSize.Longitudinal);
            Child.Measure(size);
        }

        public void Arrange(FlexSize availableSize, FlexSize offset)
        {
            double x = 0;
            double width = availableSize.Lateral;

            if (Child is FrameworkElement frameworkChild)
            {
                switch (frameworkChild.HorizontalAlignment)
                {
                    default:
                    case HorizontalAlignment.Stretch:
                        width = availableSize.Lateral;
                        break;
                    case HorizontalAlignment.Center:
                        x = (availableSize.Lateral - width) / 2;
                        break;
                    case HorizontalAlignment.Left:
                        break;
                    case HorizontalAlignment.Right:
                        x = availableSize.Lateral - width;
                        break;
                }
            }

            Child.Arrange(new Rect(new Point(x, offset.Longitudinal), new Size(width, availableSize.Longitudinal)));
        }
    }
}
