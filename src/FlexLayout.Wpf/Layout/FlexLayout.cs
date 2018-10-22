using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FlexibleLayout.Wpf.Layout
{
    public abstract class FlexLayout : Panel
    {
        /**
         * Dependency and attached Properties
         */

        /// <summary>
        /// Specify if control needs to fill the available space
        /// </summary>
        public static readonly DependencyProperty FlexProperty = DependencyProperty.RegisterAttached(
            "Flex",
            typeof(bool),
            typeof(FlexLayout),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
        );
        public static void SetFlex(UIElement element, bool value)
        {
            element.SetValue(FlexProperty, value);
        }
        public static bool GetFlex(UIElement element)
        {
            return (bool)element.GetValue(FlexProperty);
        }

        /// <summary>
        /// Controls the amount of space the control gets relative to other flex controls
        /// </summary>
        public static readonly DependencyProperty GrowProperty = DependencyProperty.RegisterAttached(
            "Grow",
            typeof(int),
            typeof(FlexLayout),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender)
        );
        public static void SetGrow(UIElement element, int value)
        {
            element.SetValue(GrowProperty, value);
        }
        public static int GetGrow(UIElement element)
        {
            return (int)element.GetValue(GrowProperty);
        }

        /// <summary>
        /// Set positioning method relative to parent
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached(
            "Position",
            typeof(Position),
            typeof(FlexLayout),
            new FrameworkPropertyMetadata(Position.Relative, FrameworkPropertyMetadataOptions.AffectsRender)
        );
        public static void SetPosition(UIElement element, Position value)
        {
            element.SetValue(PositionProperty, value);
        }
        public static Position GetPosition(UIElement element)
        {
            return (Position)element.GetValue(PositionProperty);
        }

        /// <summary>
        /// Specify spacing between child controls
        /// </summary>
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            "Spacing",
            typeof(int),
            typeof(FlexLayout),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public int Spacing
        {
            get => (int)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// Specify to skip measure on child control. Only recommended for childs that have no children themselfs.
        /// This could improve performance when measure takes a long time while isn't really used in Flex controls.
        /// </summary>
        public static readonly DependencyProperty SkipMeasureProperty = DependencyProperty.RegisterAttached(
            "SkipMeasure",
            typeof(bool),
            typeof(FlexLayout),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
        );
        public static void SetSkipMeasure(UIElement element, bool value)
        {
            element.SetValue(SkipMeasureProperty, value);
        }
        public static bool GetSkipMeasure(UIElement element)
        {
            return (bool)element.GetValue(SkipMeasureProperty);
        }

        protected virtual List<IChildAdapter> ChildAdapters { get => throw new NotImplementedException(); }

        /**
         * Methods
         */
        public void Add(UIElement widget)
        {
            Children.Add(widget);
        }

        /// <summary>
        /// Measures the desired space for the layout
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns>Desired space</returns>
        protected FlexSize MeasureOverride(FlexSize availableSize)
        {
            (var availableContentSize, var totalSpacing) = CalculateAvailableContentSize(availableSize);

            foreach (var adapter in ChildAdapters)
            {
                if (GetSkipMeasure(adapter.Child))
                {
                    continue;
                }

                adapter.Measure(availableContentSize);
            }

            // calculate total width and height from children
            var desiredContentSize = CalculateDesiredContentSize(availableContentSize);

            var totalLongitudinalSize = desiredContentSize.Longitudinal + totalSpacing;

            (var flexLongitunal, var flexTotalGrowCount, var flex) = CalculateFlexLongitudinal(availableContentSize.Longitudinal, totalSpacing);

            if (flex)
            {
                foreach (var adapter in ChildAdapters)
                {
                    var desiredLongitudinal = adapter.DesiredSize.Longitudinal;

                    if (GetPosition(adapter.Child) == Position.Absolute || GetSkipMeasure(adapter.Child)) 
                    {
                        continue;
                    }

                    var childIsFlex = GetFlex(adapter.Child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(adapter.Child);
                        if (flexTotalGrowCount > 0)
                            desiredLongitudinal = flexLongitunal * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new FlexSize(desiredLongitudinal, availableContentSize.Lateral);

                    adapter.Measure(availableChildSize);
                }
            }
            else
            {
                var scaleFactor = desiredContentSize.Longitudinal > 0 ? availableContentSize.Longitudinal / desiredContentSize.Longitudinal : 0;

                foreach (var adapter in ChildAdapters)
                {
                    if (GetPosition(adapter.Child) == Position.Absolute || GetSkipMeasure(adapter.Child))
                    {
                        continue;
                    }

                    var childLongitudinal = 0d;
                    if (scaleFactor > 0 && adapter.DesiredSize.Longitudinal > 0)
                    {
                        childLongitudinal = adapter.DesiredSize.Longitudinal * scaleFactor;
                    }
                    var availableChildSize = new FlexSize(childLongitudinal, availableContentSize.Lateral);

                    adapter.Measure(availableChildSize);
                }
            }
            
            return new FlexSize(totalLongitudinalSize, desiredContentSize.Lateral);
        }

        protected (double flexLongitudinal, int flexTotalGrowCount, bool flex) CalculateFlexLongitudinal(double finalLongitudinal, double spacing)
        {
            var flex = false;
            int flexTotalGrowCount = 0;

            foreach (var adapter in ChildAdapters)
            {
                if (GetPosition(adapter.Child) == Position.Absolute)
                {
                    continue;
                }

                if (!GetFlex(adapter.Child))
                {
                    if (finalLongitudinal > adapter.DesiredSize.Longitudinal)
                        finalLongitudinal -= adapter.DesiredSize.Longitudinal;
                    else
                        finalLongitudinal = 0;

                    continue;
                }

                flex = true;
                flexTotalGrowCount += GetGrow(adapter.Child);
            }

            return (finalLongitudinal, flexTotalGrowCount, flex);
        }

        private (FlexSize availableContentSize, int totalSpacing) CalculateAvailableContentSize(FlexSize availableSize)
        {
            var availableContentSize = new FlexSize(longitudinal: 0d, lateral: availableSize.Lateral);

            if (InternalChildren.Count <= 1)
                return (availableSize, 0);

            var totalSpacing = (InternalChildren.Count - 1) * Spacing;
            if (availableSize.Longitudinal > totalSpacing)
                availableContentSize.Longitudinal = availableSize.Longitudinal - totalSpacing;

            return (availableContentSize, totalSpacing);
        }

        /// <summary>
        /// Calculate content size
        /// </summary>
        private FlexSize CalculateDesiredContentSize(FlexSize availableContentSize)
        {
            var desiredSize = new FlexSize(0, 0);

            // calculate total width and height from children
            foreach (var adapter in ChildAdapters)
            {
                desiredSize.Longitudinal += adapter.DesiredSize.Longitudinal;

                if (adapter.DesiredSize.Lateral > desiredSize.Lateral)
                    desiredSize.Lateral = adapter.DesiredSize.Lateral;
            }

            // Limit to available size
            if (desiredSize.Longitudinal > availableContentSize.Longitudinal)
                desiredSize.Longitudinal = availableContentSize.Longitudinal;
            if (desiredSize.Lateral > availableContentSize.Lateral)
                desiredSize.Lateral = availableContentSize.Lateral;

            return desiredSize;
        }

        /// <summary>
        /// Arranges children based on space assigned to this layout
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected virtual FlexSize ArrangeOverride(FlexSize finalSize)
        {
            double l = 0;

            (var availableContentSize, var totalSpacing) = CalculateAvailableContentSize(finalSize);

            var desiredContentSize = CalculateDesiredContentSize(availableContentSize);

            (var flexLongitudinal, var flexTotalGrowCount, var flex) = CalculateFlexLongitudinal(availableContentSize.Longitudinal, totalSpacing);

            if (flex)
            {
                foreach (var adapter in ChildAdapters)
                {
                    if (GetPosition(adapter.Child) == Position.Absolute)
                    {
                        adapter.Arrange(availableContentSize, new FlexSize(0, 0));
                        continue;
                    }
                    var childLongitudinal = adapter.DesiredSize.Longitudinal;

                    var childIsFlex = GetFlex(adapter.Child);
                    if (childIsFlex)
                    {
                        var childFlexGrow = GetGrow(adapter.Child);
                        if (flexTotalGrowCount > 0)
                            childLongitudinal = flexLongitudinal * (childFlexGrow / (double)flexTotalGrowCount);
                    }

                    var availableChildSize = new FlexSize(childLongitudinal, availableContentSize.Lateral);

                    adapter.Arrange(availableChildSize, new FlexSize(l, 0));
                    l += childLongitudinal + Spacing;
                }
            }
            else
            {
                var scaleFactor = desiredContentSize.Longitudinal > 0 ? availableContentSize.Longitudinal / desiredContentSize.Longitudinal : 0;

                foreach (var adapter in ChildAdapters)
                {
                    if (GetPosition(adapter.Child) == Position.Absolute)
                    {
                        adapter.Arrange(availableContentSize, new FlexSize(0d, 0d));
                        continue;
                    }
                    var childLongitudinal = adapter.DesiredSize.Longitudinal * scaleFactor;
                    var availableSize = new FlexSize(childLongitudinal, availableContentSize.Lateral);

                    adapter.Arrange(availableSize, new FlexSize(l, 0d));
                    l += childLongitudinal + Spacing;
                }
            }

            return finalSize;
        }
    }
}
