// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using Magician.UI;
using Magician.Geo;
using Magician.Interactive;
using Magician.Data;
using static Magician.Geo.Create;
using static Magician.Geo.Ref;

using static SDL2.SDL;

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

            Origin["hex"] = RegularPolygon(6, 100).Colored(new RGBA(0x00e05080))
            .Wielding(
                RegularPolygon(3, 45).Colored(HSLA.Random(alpha: 0x80, lightness: 0.9, saturation: 0.9))
                .Wielding(
                    RegularPolygon(4, 20).Colored(HSLA.Random(alpha: 0x80, lightness: 0.9, saturation: 1))
                )
            )
            .Surrounding(
                RegularPolygon(3, 300).DrawFlags(DrawMode.OUTERP)
                .Sub(
                    m => m
                    .DrivenPM(
                        p => p + 0.001,
                        m => m
                    )
                )
            )
            ;
        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        { 
            Renderer.Control.Clear();
        }
    }
}