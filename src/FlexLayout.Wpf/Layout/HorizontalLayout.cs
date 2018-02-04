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
            var availableContentSize = CalculateAvailableContentSize(availableSize);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableContentSize);
            }

            var desiredContentSize = CalculateDesiredContentSize();

            var desiredSize = desiredContentSize;
            if (InternalChildren.Count > 1)
            {
                desiredSize.Width += (InternalChildren.Count - 1) * Spacing;
            }

            return desiredSize;
        }

        /// <summary>
        /// Arranges children based on space assigned to this layout
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;

            double finalChildrenWidth = CalculateFinalContentWidth(finalSize);

            var desiredContentSize = CalculateDesiredContentSize();

            if (desiredContentSize.Height > finalSize.Height)
                desiredContentSize.Height = finalSize.Height;

            if (desiredContentSize.Width == 0)
                return finalSize;

            var flex = false;
            var flexTotalGrowCount = 0;
            var flexWidth = finalChildrenWidth;
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

                    var availableChildSize = new Size(childWidth, finalSize.Height);

                    ArrangeChild(child, availableChildSize, new Point(x, 0));
                    x += childWidth + Spacing;
                }
            }
            else
            {
                var scaleFactor = finalChildrenWidth / desiredContentSize.Width;

                foreach (UIElement child in InternalChildren)
                {
                    var childWidth = child.DesiredSize.Width * scaleFactor;
                    var availableChildSize = new Size(finalSize.Width, childWidth);
                
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
        /// Calculate size available for the children
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private Size CalculateAvailableContentSize(Size availableSize)
        {
            if (InternalChildren.Count <= 1)
                return availableSize;

            Size availableContentSize = new Size(0, availableSize.Height);
            
            var spacingSize = (InternalChildren.Count - 1) * Spacing;
            if (availableSize.Width > spacingSize)
                availableContentSize.Width = availableSize.Width - spacingSize;

            return availableContentSize;
        }

        /// <summary>
        /// Calculated desired size of the children
        /// </summary>
        /// <returns></returns>
        private Size CalculateDesiredContentSize()
        {
            Size desiredSize = new Size(0,0);

            foreach (UIElement child in InternalChildren)
            {
                desiredSize.Width += child.DesiredSize.Width;

                if (child.DesiredSize.Height > desiredSize.Height)
                    desiredSize.Height = child.DesiredSize.Height;
            }

            return desiredSize;
        }

        private double CalculateFinalContentWidth(Size finalSize)
        {
            double childrenFinalWidth = finalSize.Width;
            if (InternalChildren.Count > 1)
            {
                var spacingSize = (InternalChildren.Count - 1) * Spacing;
                if (spacingSize <= childrenFinalWidth)
                    childrenFinalWidth -= spacingSize;
            }

            return childrenFinalWidth;
        }
    }
}