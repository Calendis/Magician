using Magician.Maps;

namespace Magician;
public class Brush : ParamMap
{
    public Brush(DirectMap dm0, DirectMap dm1) : base(dm0, dm1) {}

    public Multi Paint(double t, Multi m)
    {
        double[] pos = base.Evaluate(new double[] { t });
        if (t > 0)
        {
            return m.Copy().Positioned(pos[0], pos[1]).Tagged($"{pos[0]}{pos[1]}paint");
        }
        return new Multi().Tagged("empty paint");
    }

    public void PaintPolygon(double t, int sides, double mag, Color? c = null)
    {
        c ??= HSLA.RandomVisible();
        Paint(t, Geo.Create.RegularPolygon(sides, mag).Colored(c));
    }
}