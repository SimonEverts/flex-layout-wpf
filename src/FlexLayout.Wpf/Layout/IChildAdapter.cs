using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    public interface IChildAdapter
    {
        UIElement Child { get; }

        FlexSize DesiredSize { get; }

        void Measure(FlexSize availableSize);

        void Arrange(FlexSize availableSize, FlexSize offset);
    }
}