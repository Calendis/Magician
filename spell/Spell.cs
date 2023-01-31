// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using Magician.UI;
using Magician.Geo;
using static Magician.Geo.Create;
using static Magician.Geo.Ref;

using static SDL2.SDL;
using Magician.Interactive;

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Random rng = new Random();
        static UI.Grid uiGrid;
        static Multi myMulti;
        static Multi banner;
        static Multi multi2;

        // Initializations
        static Spell()
        {
            uiGrid = UI.Presets.Graph.Cartesian();
            myMulti = RegularPolygon(3, 100).Colored(new RGBA(0x00f08080));
            banner = new Multi();
            multi2 = Star(6, 10, 48).Colored(new RGBA(0xf8070080));
        }

        public static void PreLoop()
        {
            Renderer.Control.Clear();
            // Add a grid
            Origin.Add(uiGrid.Render());
            Origin.Add(myMulti, multi2);

            myMulti
            .DrivenXY(
                x => Events.MouseX,
                y => Events.MouseY
            )
            .Sub(
                m =>
                m.DrivenPM(
                    ph => ph + 0.01,
                    m => m
                )
            )
            .DrivenXY(
                x => -x,
                y => myMulti.LastX/2
            )
            .DrivenPM(
                ph => ph,
                mg => mg*2
            )
            ;
            myMulti[0].Become(
                RegularPolygon(4, 20).Sub(
                    m => m.DrivenPM(
                        ph => ph - 0.04,
                        mg => mg
                    )
                )
            );
            myMulti[2].Become(RegularPolygon(5, 20));

            multi2
            .DrivenXY(
                x => 100*Math.Cos(Environment.Time /5),
                y => 100*Math.Sin(Environment.Time/9)
            )
            .DrivenXY(
                x => x+Events.MouseX,
                y => y+Events.MouseY
            )
            ;
        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        { 
            //Origin[2].Rotated(-0.04);
            Renderer.Control.Clear();
            //Console.WriteLine()
        }
    }
}