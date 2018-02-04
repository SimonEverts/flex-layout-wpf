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
            var availableContentSize = CalculateAvailableContentSize(availableSize);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableContentSize);
            }

            var desiredContentSize = CalculateDesiredContentSize();

            var desiredSize = desiredContentSize;
            if (InternalChildren.Count > 1)
            {
                desiredSize.Height += (InternalChildren.Count - 1) * Spacing;
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
            double y = 0;

            double finalChildrenHeight = CalculateFinalContentHeight(finalSize);

            var desiredContentSize = CalculateDesiredContentSize();

            if (desiredContentSize.Width > finalSize.Width)
                desiredContentSize.Width = finalSize.Width;

            if (desiredContentSize.Height == 0)
                return finalSize;

            var flex = false;
            var flexTotalGrowCount = 0;
            var flexHeight = finalChildrenHeight;
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

                    var availableChildSize = new Size(finalSize.Width, childHeight);

                    ArrangeChild(child, availableChildSize, new Point(0, y));
                    y += childHeight + Spacing;
                }
            }
            else
            {
                var scaleFactor = finalChildrenHeight / desiredContentSize.Height;

                foreach (UIElement child in InternalChildren)
                {
                    var childHeight = child.DesiredSize.Height * scaleFactor;
                    var availableChildSize = new Size(finalSize.Width, childHeight);
                
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
        /// Calculate size available for the children
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private Size CalculateAvailableContentSize(Size availableSize)
        {
            if (InternalChildren.Count <= 1)
                return availableSize;

            Size availableContentSize = new Size(availableSize.Width, 0);
            
            var spacingHeight = (InternalChildren.Count - 1) * Spacing;
            if (availableSize.Height > spacingHeight)
                availableContentSize.Height = availableSize.Height - spacingHeight;

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
                desiredSize.Height += child.DesiredSize.Height;

                if (child.DesiredSize.Width > desiredSize.Width)
                    desiredSize.Width = child.DesiredSize.Width;
            }

            return desiredSize;
        }

        private double CalculateFinalContentHeight(Size finalSize)
        {
            double childrenFinalHeight = finalSize.Height;
            if (InternalChildren.Count > 1)
            {
                var spacingHeight = (InternalChildren.Count - 1) * Spacing;
                if (spacingHeight <= childrenFinalHeight)
                    childrenFinalHeight -= spacingHeight;
            }

            return childrenFinalHeight;
        }
    }
}