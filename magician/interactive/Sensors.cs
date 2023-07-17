namespace Magician.Interactive;
public static class Sensor
{
    public static DirectMap Click = new DirectMap(b => Events.Click ? 1 : 0);


    public static DirectMap MouseOver(Multi m)
    {
        return new DirectMap(b => Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? 1 : 0);
    }

    public static DirectMap ScrollOver(Multi m)
    {
        return new DirectMap(b => Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m) ? Events.ScrollY : 0);
    }
}