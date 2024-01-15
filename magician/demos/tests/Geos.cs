namespace Magician.Demos.Tests;
using Library;
using Geo;

public class Geos : Spell
{
    public override void Loop()
    {
        if (Geo.Check.PointInRectVolume(Origin["lineToClip"][1], (0, 500), (-300, 0), (-600, 600)))
        {
            Origin["boundBox"].Colored(new HSLA(Origin["boundBox"].Col.H + 0.05, 1, 1, 255));
            // Find the intersection point
            //Geo.Vec intersect = Geo.Find.Intersection(Origin["lineToClip"], 0, 0);
            //Origin["intersect"] = Geo.Create.RegularPolygon(6, 50).Positioned(intersect.x.Evaluate(), intersect.y.Evaluate(), intersect.z.Evaluate());
        }
        else
        {
        }
    }

    public override void PreLoop()
    {
        Origin["lineToClip"] = new Node(Geo.Create.Point(-300, 200), Geo.Create.Point(-200, 200)).Colored(HSLA.RandomVisible());
        // TODO: re-implement this
        //Origin["lineToClip"].DrivenXY(x => Events.MouseX, y => Events.MouseY);
        //Origin["lineToClip"][1].DrivenPM(p => p + 0.01, m => m);
        Origin["boundBox"] = Geo.Create.Rect(0, 0, 500, 300);
    }
}