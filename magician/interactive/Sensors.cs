namespace Magician.Interactive;
using Core.Maps;
using Geo;

public static class Sensor
{
    public class MouseOver : DirectMap
    {
        public MouseOver(Node m) : base(b => Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0) {}
    }

    public class ScrollOver : DirectMap
    {
        public ScrollOver(Node m) : base(b => Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0) {}
    }
}