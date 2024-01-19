namespace Magician.Alg;

public struct PlotOptions
{
    AxisSpecifier axis;
    Range range;
    public AxisSpecifier Axis => axis;
    public Range Range => range;

    public PlotOptions(AxisSpecifier a, Range r)
    {
        axis = a;
        range = r;
    }

}

public struct Range
{
    double min;
    double max;
    double res;
    public double Min => min;
    public double Max => max;
    public double Res => res;
    public Range(double mn, double mx, double rs)
    {
        min = mn;
        max = mx;
        res = rs;
    }
}