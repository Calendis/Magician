// This class is where you create your stuff
// It's static for now

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Random rng = new Random();
        static bool sw = true;

        static double theta = 0.00;  // This controls the broad shape of the figure, from 0 to 2PI
        static double scale;
        static double modulus = rng.NextDouble();
        
        static int iters = 150;
        static int noPoints = 24000;
        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            //Renderer.Control.Clear();
            for (int i = 0; i < noPoints; i++)
            {
                Geo.Origin.Add(
                    Geo.Point(Ref.RandX, Ref.RandY).Colored(new RGBA(0x00ffff10))
                    .Colored(new HSLA(0, 1, 1, 6))
                );
                scale = 4d/modulus;
            }
        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            //Renderer.Control.Clear();
            double t = frames * timeResolution;
            Geo.Origin.Sub(
                m =>
                {
                    m.Written(m.X.Evaluate())
                    .Rotated(Math.PI / 2)
                    .Translated((t + m.Normal) % (modulus) * 100 * scale, 0)
                    .Scaled(Math.Sin(theta + m.Index))
                    .Colored(new HSLA((m.Evaluate() - m.X.Evaluate()) / 80, 1, 1, 8));
                }
            );

            if (frames % iters == 0 && frames != 0)
            {
                Renderer.Control.Clear(new RGBA(0x000000ff));
                Geo.Origin.Sub(m =>
                    m.Translated(-m.X.Evaluate(), -m.Y.Evaluate()).Translated(Ref.RandX, Ref.RandY)
                );
                modulus = rng.NextDouble();
                scale = 2d/modulus;
                //Renderer.Control.saveFrame = true;
            }
            else
            {
                Renderer.Control.saveFrame = false;
            }
            
        }
    }
}