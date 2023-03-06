using System;

namespace Magician
{
    public class Brush : IOMap
    {
        public Brush(IMap im0, IMap im1) : base(1, im0, im1)
        {
            //
        }

        public Multi Paint(double t, Multi m)
        {
            double[] pos = base.Evaluate(new double[]{t});
            if (t > 0)
            {
                return m.Copy().Positioned(pos[0], pos[1]).Tagged($"{pos[0]}{pos[1]}paint");
            }
            return new Multi().Tagged("empty paint");
        }

        public void PaintPolygon(double t, int sides, double mag, Color? c=null)
        {
            c ??= HSLA.RandomVisible();
            Paint(t, Geo.Create.RegularPolygon(sides, mag).Colored(c));
        }
    }
}
