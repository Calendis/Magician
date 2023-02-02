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

            Origin["parametric"] = new Multimap(1,
                x => 180 * Math.Cos(x / 3),
                y => 180 * Math.Sin(y / 7)
            ).TextAlong(-49, 49, 0.3, "Here's an example of a Multimap with 1 input and two outputs being used to draw text parametrically"
            , new RGBA(0x00ff9080));
        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        {
            Renderer.Control.Clear();
        }
    }
}