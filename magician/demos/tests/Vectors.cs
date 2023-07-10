namespace Magician.Demos.Tests;

using Magician.Library;
using Magician.Geo;

public class Vectors : Spell
{
    Vec flatVector = new(0,0,0);
    public override void Loop()
    {
        flatVector = flatVector.YawPitchRotated(0.1, 0);
        Origin["flatVector"] = flatVector.Render();
    }

    public override void PreLoop()
    {
        flatVector = new(100, 100, -110);
    }
}