using System;
using System.Collections.Generic;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    public class VerticalLayout : FlexLayout
    {
        protected override List<IChildAdapter> ChildAdapters
        {
            get {
                var list = new List<IChildAdapter>();
                foreach (var child in InternalChildren)
                {
                    list.Add(new VerticalChildAdapter((UIElement)child));
                }

                return list;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var flexSize = MeasureOverride(new FlexSize(availableSize.Height, availableSize.Width));
            return new Size(flexSize.Lateral, flexSize.Longitudinal);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var flexSize = ArrangeOverride(new FlexSize(finalSize.Height, finalSize.Width));
            return new Size(flexSize.Lateral, flexSize.Longitudinal);
        }
    }
}