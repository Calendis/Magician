namespace Magician.Interactive
{
    public abstract class Sensor : CustomMap
    {
        public static IMap MouseOver(Multi m)
        {
            return new CustomMap(b => Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0);
        }

        public static IMap ScrollOver(Multi m)
        {
            return new CustomMap(b => Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0);
        }
    }
}
