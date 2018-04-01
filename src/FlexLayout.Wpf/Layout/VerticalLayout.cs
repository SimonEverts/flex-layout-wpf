using System;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    public class VerticalLayout : FlexLayout
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

            var flex = CalculateFlexHeight(availableContentSize.Height, out var flexTotalGrowCount, out var flexHeight);

            if (flex)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var childHeight = child.DesiredSize.Height;

                    var childIsFlex = GetFlex(child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(child);
                        if (flexTotalGrowCount > 0)
                            childHeight = flexHeight * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new Size(availableContentSize.Width, childHeight);

                    child.Measure(availableChildSize);
                }
            }
            else
            {
                var scaleFactor = availableContentSize.Height / desiredSize.Height;

                foreach (UIElement child in InternalChildren)
                {
                    var childHeight = child.DesiredSize.Height * scaleFactor;
                    var availableChildSize = new Size(availableContentSize.Width, childHeight);

                    child.Measure(availableChildSize);
                }
            }

            return desiredSize;
        }

        private bool CalculateFlexHeight(double finalChildrenHeight, out int flexTotalGrowCount, out double flexHeight)
        {
            var flex = false;
            flexTotalGrowCount = 0;
            flexHeight = finalChildrenHeight;
            foreach (UIElement child in InternalChildren)
            {
                if (!GetFlex(child))
                {
                    if (flexHeight > child.DesiredSize.Height)
                        flexHeight -= child.DesiredSize.Height;
                    else
                        flexHeight = 0;

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
            double y = 0;

            (var availableContentSize, var spacing) = CalculateAvailableContentSize(finalSize);

            var desiredSize = CalculateDesiredSize(availableContentSize, spacing);

            if (desiredSize.Height == 0)
                return finalSize;

            var flex = CalculateFlexHeight(availableContentSize.Height, out var flexTotalGrowCount, out var flexHeight);

            if (flex)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var childHeight = child.DesiredSize.Height;

                    var childIsFlex = GetFlex(child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(child);
                        if (flexTotalGrowCount > 0)
                            childHeight = flexHeight * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new Size(availableContentSize.Width, childHeight);

                    ArrangeChild(child, availableChildSize, new Point(0, y));
                    y += childHeight + Spacing;
                }
            }
            else
            {
                var scaleFactor = availableContentSize.Height / desiredSize.Height;

                foreach (UIElement child in InternalChildren)
                {
                    var childHeight = child.DesiredSize.Height * scaleFactor;
                    var availableChildSize = new Size(availableContentSize.Width, childHeight);
                
                    ArrangeChild(child, availableChildSize, new Point(0, y));
                    y += childHeight + Spacing;
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
            var width = child.DesiredSize.Width;
            if (width > availableSize.Width)
                width = availableSize.Width;

            double x = 0;

            if (child is FrameworkElement frameworkChild)
            {
                switch (frameworkChild.HorizontalAlignment)
                {
                    default:
                    case HorizontalAlignment.Stretch:
                        width = availableSize.Width;
                        break;
                    case HorizontalAlignment.Center:
                        x = (availableSize.Width - width) / 2;
                        break;
                    case HorizontalAlignment.Left:
                        break;
                    case HorizontalAlignment.Right:
                        x = availableSize.Width - width;
                        break;
                }
            }

            child.Arrange(new Rect(new Point(x, offset.Y), new Size(width, availableSize.Height)));
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
                desiredSize.Height += child.DesiredSize.Height;

                if (child.DesiredSize.Width > desiredSize.Width)
                    desiredSize.Width = child.DesiredSize.Width;
            }

            // Include spacing between children
            if (InternalChildren.Count > 1)
            {
                desiredSize.Height += spacing;
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
            Size availableContentSize = new Size(availableSize.Width, 0);

            if (InternalChildren.Count <= 1)
                return (availableSize, 0);

            var spacing = (InternalChildren.Count - 1) * Spacing;
            if (availableSize.Height > spacing)
                availableContentSize.Height = availableSize.Height - spacing;

            return(availableContentSize, spacing);
        }
    }
}