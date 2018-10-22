namespace FlexibleLayout.Wpf.Layout
{
    public struct FlexSize
    {
        public double Longitudinal;
        public double Lateral;

        public FlexSize(double longitudinal, double lateral)
        {
            Longitudinal = longitudinal;
            Lateral = lateral;
        }
    }
}