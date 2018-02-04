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
        /// Space the control gets relative to other flex controls
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
        /// Space between controls
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

        /**
         * Methods
         */

        public void Add(UIElement widget)
        {
            Children.Add(widget);
        }        
    }
}
