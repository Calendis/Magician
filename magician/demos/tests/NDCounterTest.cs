using Magician.Library;
using Magician.Symbols;

namespace Magician.Demos.Tests;

public class NDCounterTest : Spell
{
    public override void Loop()
    {
        Origin["A"] = Geo.Create.Rect(0, 320, 300, 200).Colored(new HSLA(90, 1, 0.7, 255));
    }

    public override void PreLoop()
    {
        int C = 0;
        // create a 2x2x5 counter, with finer resolution on the last axis
        NDCounter ndc = new((-2,3,5), (-5,0,5), (0,2,0.5));
        do
        {
            Scribe.Info($"{C++}, {ndc.Val}: {ndc.Done}");
        } while (!ndc.Increment());
    }
}