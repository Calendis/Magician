using static Magician.Geo.Ref;
using Magician.Library;

namespace Magician.Demos
{
    public class WavingText : Spell
    {

        public override void PreLoop()
        {
            Renderer.Control.Clear();

            // Add a grid
            Origin["grid"] = uiGrid.Render();

        }

        // For stuff that needs to redefined every frame
        public override void Loop()
        {
            Renderer.Control.Clear();
            Origin["parametric"] = new Multimap(1,
                x => 180 * Math.Cos(x / 3) + 10 * Math.Sin(Time / 2),
                y => 180 * Math.Sin(y / 7 + Time)
            ).TextAlong(-49, 49, 0.3, "Here's an example of a Multimap with 1 input and two outputs being used to draw text parametrically"
            , new RGBA(0x00ff9080));
        }
    }

    public class SpinningStuff : Spell
    {
        public override void PreLoop()
        {
            Origin["myMulti"] = Create.RegularPolygon(5, 120).Colored(HSLA.Random(saturation: 1, lightness: 1, alpha: 100))
            .Sub(
                m => m
                .DrivenPM(
                    ph => ph + 0.025,
                    m => m
                )
            );
        }

        public override void Loop()
        {
            Renderer.Control.Clear();
        }
    }
}
