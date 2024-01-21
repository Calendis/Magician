namespace Magician.Demos.Tests;
using Geo;
using Core.Caster;
using Core.Maps;

public class Spinner10K : Spell
{
    public override void PreLoop()
    {
        Origin["sqs"] = Create.Along(new Parametric(x => x % 1000, y => 20 * Math.Floor(y / 1000)), 0, 20000, 20, Create.RegularPolygon(4, 10));

        // TODO: re-implement driving
        //Origin["sqs"].Sub(d => d.Sub(m => m
        //    .DrivenPM(p => p + 0.1, m => m)
        //)).Positioned(-500, -300);
    }

    public override void Loop()
    {
        Paint.Renderer.Clear();
        //Scribe.Info(Origin);
    }
}