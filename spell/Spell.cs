// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using Magician.UI;
using Magician.Geo;
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
        static Multi pentagon;
        static Multi bigText;

        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            uiGrid = UI.Presets.Graph.Cartesian();
            Origin.Add(uiGrid.Render());

            //

            Perspective.x.Driven(x => 50*Math.Sin(x[0]/10));
            Perspective.y.Driven(x => 50*Math.Cos(x[0]/10));
            //Quantity.ExtantQuantites.Add(Perspective.x);
            //Quantity.ExtantQuantites.Add(Perspective.y);
            bigText = ((IMap)new Driver(x => 64*Math.Sin(x[0]/40)))
            .MultisAlong(-100, 100, 10*Math.PI, new Multi().Textured(new Text("a", new RGBA(0x00ffffff)).Render()));
            Origin.Add(bigText);

            Origin.Add(((IMap)new Driver(x => 64*Math.Sin(x[0]/40)))
            .TextAlong(-200, 200, 10*Math.PI, "hellotheremyfriendsletshavesomefunyahooooooooooooooooooooooooooo"));

        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            uiGrid = uiGrid.Update();
            Origin.csts[0] = uiGrid.Render();
            //Console.WriteLine(Perspective.x);
        }
    }
}