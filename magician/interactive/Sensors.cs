namespace Magician.Interactive;
using Core.Maps;
using Geo;

public static class Sensor
{
    // TODO: don't return a new directmap every time. completely unnecessary!!
    //       make mouseover and scrollover classes inheriting directmap instead
    public static DirectMap MouseOver(Node m)
    {
        return new DirectMap(b => Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0);
    }

    public static DirectMap ScrollOver(Node m)
    {
        return new DirectMap(b => Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0);
    }
}