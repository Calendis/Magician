// This class is where you create your stuff
// It's static for now

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Renderer.Texture t;
        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            // Default graph
            UI.Presets.Graph.Cartesian();
            
            // Plot a sine wave
            Multi sineWave = ((IMap)new Driver(
                x => 30*Math.Sin(x[0]/6)
            ))
            .Plot(0, 0, -300, 300, 3, new RGBA(0x00ff00ff));
            Geo.Origin.Add(sineWave);

            // Plot a Lissajous curve
            Multi lissajous = ((IMap)new Driver(
                t => new double[]{300*Math.Cos(t[0]/5), 300*Math.Sin(t[0])
                }))
            .Plot(0, 0, 0, 100, 0.1, new RGBA(0xff0000ff));
            Geo.Origin.Add(lissajous);

            // Load a texture
            //t = new Renderer.Texture("test.png", 400, 300);
            t = new Renderer.Text("hello world :)", Ref.fgCol).Render();
        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            t.Draw(500, 100);
        }
    }
}