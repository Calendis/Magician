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

        // Initializations
        static Spell()
        {
            uiGrid = UI.Presets.Graph.Cartesian();
        }

        public static void PreLoop()
        {
            Renderer.Control.Clear();
            
            // Add a grid
            Origin["grid"] = uiGrid.Render();

            Origin["spinner"] = RegularPolygon(6, 80).Colored(new RGBA(0x00e05080))
            .Sub(m => m
                .DrivenPM(
                    ph => ph + 0.01,
                    mg => mg
                )
            )
            .Wielding(
                RegularPolygon(3, 30)

            );
            Origin["spinner"][1]
            .Sub(
                    m => m
                    .DrivenPM(
                        ph => ph + 0.06,
                        mg => mg
                    )
                )
                .DrivenAbs(
                    x => Events.MouseX,
                    y => Events.MouseY
                )
            ;

            // Demo of the EPIC parenting system
            /*
            Origin["ofOrigin"] = Star(400, -100, 5, 30, 60).Colored(new RGBA(0xffbb0080))
            .DrivenXY(
                x => 100*Math.Sin(Environment.Time/60),
                y => 100*Math.Cos(Environment.Time/60)
            )
            ;
            
            Origin["ofOrigin"][1].Become(RegularPolygon(4, 40)
            .Parented(Origin["ofOrigin"])
            .DrivenXY(
                x => Events.MouseX,
                y => Events.MouseY
            ));
            */
        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        { 
            Renderer.Control.Clear();
        }
    }
}