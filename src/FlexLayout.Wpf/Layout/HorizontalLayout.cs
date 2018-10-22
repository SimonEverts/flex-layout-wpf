using System;
using System.Collections.Generic;
using System.Windows;

namespace FlexibleLayout.Wpf.Layout
{
    public class HorizontalLayout : FlexLayout
    {
        protected override List<IChildAdapter> ChildAdapters
        {
            get { 
                var list = new List<IChildAdapter>();
                foreach (var child in InternalChildren)
                {
                    list.Add(new HorizontalChildAdapter((UIElement)child));
                }

                return list;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var flexSize = MeasureOverride(new FlexSize(availableSize.Width, availableSize.Height));
            return new Size(flexSize.Longitudinal, flexSize.Lateral);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var flexSize = ArrangeOverride(new FlexSize(finalSize.Width, finalSize.Height));
            return new Size(flexSize.Longitudinal, flexSize.Lateral);
        }
    }
}