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

        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            //Origin.Add(uiGrid.Render());
            Origin.Add(new Multi());


            Perspective.x.Driven(x => 50 * Math.Sin(x[0] / 10));
            Perspective.y.Driven(x => 50 * Math.Cos(x[0] / 10));
            //Quantity.ExtantQuantites.Add(Perspective.x);
            //Quantity.ExtantQuantites.Add(Perspective.y);

        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        { 
            Renderer.Control.Clear();
            //uiGrid.DisposeAllTextures();
            //uiGrid = uiGrid.Update();
            //Origin[0] = uiGrid.Render();


            double t = frames * timeResolution;
            Origin[0].DisposeAllTextures();
            Origin[0] = (((IMap)new Driver(x => 16 * Math.Sin(x[0] / 40 + t)))
                .TextAlong(-200, 600, 5 * Math.PI, "Hello there, my friendsssssssssssssssssss", new HSLA(t, 1, 1, 255)));            
        }
    }
}