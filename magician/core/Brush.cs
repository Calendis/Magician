namespace Magician;
using Core;
using Core.Maps;
using Geo;

public class Brush : ParamMap
{
    public Brush(DirectMap dm0, DirectMap dm1) : base(dm0, dm1) {}

    public Multi Paint(double t, Multi m)
    {
        IMultival pos = Evaluate(t);
        if (t > 0)
        {
            return m.Copy().To(pos.ToVariable()).Tagged($"{pos}paint");
        }
        return new Multi().Tagged("empty paint");
    }

    public void PaintPolygon(double t, int sides, double mag, Color? c = null)
    {
        c ??= HSLA.RandomVisible();
        Paint(t, Geo.Create.RegularPolygon(sides, mag).Colored(c));
    }
}