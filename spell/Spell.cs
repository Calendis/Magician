// This class is where you create your stuff
// It's static for now

using Magician.Renderer;

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Random rng = new Random();
        static Texture? tx0;
        static Texture? tx1;

        static Multi vertex;


        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            Geo.Origin.Add(UI.Presets.Graph.Cartesian());

            Geo.Origin.Add(
                Geo.RegularPolygon(0, 0, new RGBA(0x00f010a0), 5, 120)
                .Sub(m => m.Driven(
                    x => 0.01, "phase+"
                    )
                    .Textured(new Text("Maybe??", new RGBA(0x0000ffff)).Render())
                )
            );

            IMap i = new Seq(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,39,39);
            Geo.Origin.Add(i.Plot(0, -200, -10, 50, 0.1, new RGBA(0xffff00ff)));

            Geo.Origin.csts[1].csts[0].Textured(new Renderer.Text("Yes!", new RGBA(0xff00ffff)).Render());

        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
        }
    }
}