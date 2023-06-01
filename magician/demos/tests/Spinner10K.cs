using Magician.Geo;
using Magician.Library;

namespace Magician.Demos.Tests;
public class Spinner10K : Spell
{
    public override void PreLoop()
    {
        Origin["sqs"] = (new IOMap(x => x % 1000, y => 20 * Math.Floor(y / 1000))
        .MultisAlong(0, 20000, 20, Create.RegularPolygon(4, 10))
        );
        Origin["sqs"].Sub(d => d.Sub(m => m
            .DrivenPM(p => p + 0.1, m => m)
        )).Positioned(-500, -300);
    }

    public override void Loop()
    {
        Renderer.RControl.Clear();
        //Scribe.Info(Origin);
    }
}