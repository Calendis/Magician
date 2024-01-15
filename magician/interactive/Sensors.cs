namespace Magician.Interactive;
using Core;
using Core.Maps;
using Geo;

public static class Sensor
{
    public static DirectMap MouseOver(Node m)
    {
        return new DirectMap(b => IVal.FromLiteral(Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0));
    }

    public static DirectMap ScrollOver(Node m)
    {
        return new DirectMap(b => IVal.FromLiteral(Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0));
    }
}