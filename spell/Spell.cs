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
        }

        public static void PreLoop()
        {
            Renderer.Control.Clear();
            
            // Add a grid
            Origin["grid"] = uiGrid.Render();

            // Create a pentagon...
            Origin["mouseFollowPentagon"] = RegularPolygon(5, 100).Colored(new RGBA(0x00ff3070))
            // ... and make it follow the cursor
            .DrivenXY(
                x => Events.MouseX,
                y => Events.MouseY
            );
        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        { 
            Renderer.Control.Clear();
            Origin["mouseFollowPentagon"].Rotated(0.02);
        }
    }
}