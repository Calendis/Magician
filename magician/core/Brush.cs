namespace Magician;
using Core;
using Core.Maps;
using Geo;

public class Brush : Parametric
{
    public Brush(Direct dm0, Direct dm1) : base(dm0, dm1) {}

    public Node Paint(double t, Node m)
    {
        IVal pos = Evaluate(t);
        if (t > 0)
        {
            return m.Copy().To(pos).Tagged($"{pos}paint");
        }
        return new Node().Tagged("empty paint");
    }

    public void PaintPolygon(double t, int sides, double mag, Color? c = null)
    {
        c ??= HSLA.RandomVisible();
        Paint(t, Geo.Create.RegularPolygon(sides, mag).Colored(c));
    }
}