namespace Magician.Interactive
{
    public abstract class Sensor : CustomMap
    {
        public static IMap MouseOver(Multi m)
        {
            return new CustomMap(b => Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0);
        }
    }

    public class ScrollOver : Sensor
    {
        Multi m;
        public ScrollOver(Multi m)
        {
            this.m = m;
        }

        public new double Evaluate(double x)
        {
            return Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0;
        }
    }
}
