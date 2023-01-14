namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            int f = frames;
            double tr = timeResolution;

            Geo.Origin.Add(Geo.Line(
                Geo.Point(80, -200).DrawFlags(DrawMode.INVISIBLE),
                Geo.Point(80, -100).DrawFlags(DrawMode.INVISIBLE),
                new RGBA(0xff00ffff)
            ));

            Geo.Origin.Add(
                Geo.RegularPolygon(30, 100, new RGBA(0x55d00090), 5, 100)
            .Sub(m => m.Driven(x => 0.01, "phase+")).DrawFlags(DrawMode.OUTER)
            .Driven(x => 100 * Math.Sin(f * tr * 0.33), "y")
            .Driven(x => 10 * Math.Sin(f * tr), "magnitude+")
            );

            Multi g = new UI.Grid(100, 10, 100, 10).Render();
            Geo.Origin.Add(g);
        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            //
        }
    }
}