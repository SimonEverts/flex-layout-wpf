using System;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    public class HorizontalLayout : FlexLayout
    {
        /// <summary>
        /// Measures the desired space for the layout
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns>Desired space</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            (var availableContentSize, var spacing) = CalculateAvailableContentSize(availableSize);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableContentSize);
            }

            // calculate total width and height from children
            var desiredSize = CalculateDesiredSize(availableContentSize, spacing);

            var flex = CalculateFlexWidth(availableContentSize.Width, out var flexTotalGrowCount, out var flexWidth);

            if (flex)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var childWidth = child.DesiredSize.Width;

                    var childIsFlex = GetFlex(child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(child);
                        if (flexTotalGrowCount > 0)
                            childWidth = flexWidth * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new Size(childWidth, availableContentSize.Height);

                    child.Measure(availableChildSize);
                }
            }
            else
            {
                var scaleFactor = availableContentSize.Width / desiredSize.Width;

                foreach (UIElement child in InternalChildren)
                {
                    var childWidth = child.DesiredSize.Width * scaleFactor;
                    var availableChildSize = new Size(childWidth, availableContentSize.Height);

                    child.Measure(availableChildSize);
                }
            }

            return desiredSize;
        }

        private bool CalculateFlexWidth(double finalChildrenWidth, out int flexTotalGrowCount, out double flexWidth)
        {
            var flex = false;
            flexTotalGrowCount = 0;
            flexWidth = finalChildrenWidth;
            foreach (UIElement child in InternalChildren)
            {
                if (!GetFlex(child))
                {
                    if (flexWidth > child.DesiredSize.Width)
                        flexWidth -= child.DesiredSize.Width;
                    else
                        flexWidth = 0;

                    continue;
                }

                flex = true;
                flexTotalGrowCount += GetGrow(child);
            }

            return flex;
        }

        /// <summary>
        /// Arranges children based on space assigned to this layout
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;

            (var availableContentSize, var spacing) = CalculateAvailableContentSize(finalSize);

            var desiredSize = CalculateDesiredSize(availableContentSize, spacing);

            if (desiredSize.Width == 0)
                return finalSize;

            var flex = CalculateFlexWidth(availableContentSize.Width, out var flexTotalGrowCount, out var flexWidth);

            if (flex)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var childWidth = child.DesiredSize.Width;

                    var childIsFlex = GetFlex(child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(child);
                        if (flexTotalGrowCount > 0)
                            childWidth = flexWidth * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new Size(childWidth, availableContentSize.Height);

                    ArrangeChild(child, availableChildSize, new Point(x, 0));
                    x += childWidth + Spacing;
                }
            }
            else
            {
                var scaleFactor = availableContentSize.Width / desiredSize.Width;

                foreach (UIElement child in InternalChildren)
                {
                    var childWidth = child.DesiredSize.Width * scaleFactor;
                    var availableChildSize = new Size(childWidth, availableContentSize.Height);

                    ArrangeChild(child, availableChildSize, new Point(x, 0));
                    x += childWidth + Spacing;
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Arrange single child based on available size and offset
        /// </summary>
        /// <param name="child"></param>
        /// <param name="availableSize"></param>
        /// <param name="offset"></param>
        private static void ArrangeChild(UIElement child, Size availableSize, Point offset)
        {
            var height = child.DesiredSize.Height;
            if (height > availableSize.Height)
                height = availableSize.Height;

            double y = 0;

            if (child is FrameworkElement frameworkChild)
            {
                switch (frameworkChild.VerticalAlignment)
                {
                    default:
                    case VerticalAlignment.Stretch:
                        height = availableSize.Height;
                        break;
                    case VerticalAlignment.Center:
                        y = (availableSize.Height - height) / 2;
                        break;
                    case VerticalAlignment.Top:
                        break;
                    case VerticalAlignment.Bottom:
                        y = availableSize.Height - height;
                        break;
                }
            }

            child.Arrange(new Rect(new Point(offset.X, y), new Size(availableSize.Width, height)));
        }

        /// <summary>
        /// Calculate content size
        /// </summary>
        private Size CalculateDesiredSize(Size availableContentSize, int spacing)
        { 
            var desiredSize = new Size(0, 0);

            // calculate total width and height from children
            foreach (UIElement child in InternalChildren)
            {
                desiredSize.Width += child.DesiredSize.Width;

                if (child.DesiredSize.Height > desiredSize.Height)
                    desiredSize.Height = child.DesiredSize.Height;
            }

            // Include spacing between children
            if (InternalChildren.Count > 1)
            {
                desiredSize.Width += spacing;
            }

            // Limit to available size
            if (desiredSize.Height > availableContentSize.Height)
                desiredSize.Height = availableContentSize.Height;
            if (desiredSize.Width > availableContentSize.Width)
                desiredSize.Width = availableContentSize.Width;

            return desiredSize;
        }

        private (Size availableContentSize, int spacing) CalculateAvailableContentSize(Size availableSize)
        {
            Size availableContentSize = new Size(0, availableSize.Height);

            if (InternalChildren.Count <= 1)
                return (availableSize, 0);

            var spacing = (InternalChildren.Count - 1) * Spacing;
            if (availableSize.Width > spacing)
                availableContentSize.Width = availableSize.Width - spacing;

            return (availableContentSize, spacing);
        }
    }
}