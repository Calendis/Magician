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

            Multi pentagon = Geo.RegularPolygon(0, 0, new RGBA(0x00f01070), 5, 120)
                .Sub(m => m
                    .Textured(new Text($"{m.Index}", new RGBA(0xff00ffff)).Render())
                );

            Geo.Origin.Add(
                pentagon
            );
            pentagon.Driven(new Seq(1,2,3,4,5,6,7,8,9,10), "x+");

        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
        }
    }
}