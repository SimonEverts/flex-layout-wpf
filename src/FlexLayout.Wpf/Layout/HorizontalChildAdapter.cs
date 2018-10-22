using System;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    class HorizontalChildAdapter : IChildAdapter
    {
        public UIElement Child { get; }

        public FlexSize DesiredSize => new FlexSize(longitudinal: Child.DesiredSize.Width, lateral: Child.DesiredSize.Height);

        public HorizontalChildAdapter(UIElement child)
        {
            Child = child;
        }

        public void Measure(FlexSize availableSize)
        {
            var size = new Size(availableSize.Longitudinal, availableSize.Lateral);
            Child.Measure(size);
        }

        public void Arrange(FlexSize availableSize, FlexSize offset)
        {
            double y = 0;
            double height = availableSize.Lateral;

            if (Child is FrameworkElement frameworkChild)
            {
                switch (frameworkChild.VerticalAlignment)
                {
                    default:
                    case VerticalAlignment.Stretch:
                        height = availableSize.Lateral;
                        break;
                    case VerticalAlignment.Center:
                        y = (availableSize.Lateral - height) / 2;
                        break;
                    case VerticalAlignment.Top:
                        break;
                    case VerticalAlignment.Bottom:
                        y = availableSize.Lateral - height;
                        break;
                }
            }

            Child.Arrange(new Rect(new Point(offset.Longitudinal, y), new Size(availableSize.Longitudinal, height)));
        }
    }
}
