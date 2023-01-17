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

            // Make some text
            t = new Renderer.Text("hello world :)", Ref.UIDefault[4]).Render();

            Geo.Origin.Add(new Multi());  // Placeholder
        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            //Renderer.Control.Clear();
            t.Draw(500, 100);
            double time = (double)frames * timeResolution;

            // Plot a Lissajous curve
            Multi lissajous = ((IMap)new Driver(
                t => new double[]{
                    200*Math.Cos(t[0]-Math.Cos(time)) + 50*Math.Sin(t[0]*4 - Math.Cos(time)),
                    200*Math.Cos(t[0]+time/4)
                }))
            .Plot(0, 0, 0, 100, 0.1, new RGBA(0xff0000ff))
            .Driven(x => 50*Math.Sin(x[0]), "x+");
            Geo.Origin[1] = lissajous;
        }
    }
}