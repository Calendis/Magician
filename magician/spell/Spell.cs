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

namespace Magician.Library
{
    public abstract class Spell
    {
        protected UI.Grid uiGrid;
        public double Time { get; set; }
        public Random RNG = new Random();
        public double RandX => RNG.NextDouble() * Globals.winWidth - Globals.winWidth / 2;
        public double RandY => RNG.NextDouble() * Globals.winHeight - Globals.winHeight / 2;

        // The Origin is the eventual parent Multi for all Multis
        public Multi Origin = Create.Point(null, 0, 0, Data.Color.UIDefault.FG)
        .DrawFlags(DrawMode.INVISIBLE)
        .Tagged("Origin")
        ;

        public static class Create
        {
            // Create a point
            public static Multi Point(Multi? parent, double x, double y, Color col)
            {
                return new Multi(parent, x, y, col).DrawFlags(DrawMode.POINT);
            }
            public static Multi Point(double x, double y, Color col)
            {
                return Point(Ref.Origin, x, y, col).DrawFlags(DrawMode.POINT);
            }
            public static Multi Point(double x, double y)
            {
                return Point(Ref.Origin, x, y, Data.Color.UIDefault.FG);
            }

            // Create a line
            public static Multi Line(Multi p1, Multi p2, Color col)
            {
                double x1 = p1.X;
                double y1 = p1.Y;
                double x2 = p2.X;
                double y2 = p2.Y;

                Multi line = new Multi(0, 0, col, DrawMode.PLOT,
                Point(x1, y1, col),
                Point(x2, y2, col));
                // Make sure the parents are set correctly
                line[0].parent = line;
                line[1].parent = line;
                return line;
            }
            public static Multi Line(Multi p1, Multi p2)
            {
                return Line(p1, p2, Data.Color.UIDefault.FG);
            }

            // Create a regular polygon with a position, number of sides, color, and magnitude
            public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
            {
                //List<Multi> ps = new List<Multi>();
                Multi ps = new Multi().Parented(Ref.Origin);
                double angle = 360d / (double)sides;
                for (int i = 0; i < sides; i++)
                {
                    double x = magnitude * Math.Cos(angle * i / 180 * Math.PI);
                    double y = magnitude * Math.Sin(angle * i / 180 * Math.PI);
                    ps.Add(Point(ps, x, y, Data.Color.UIDefault.FG));
                }

                //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
                return ps.Positioned(xOffset, yOffset).Colored(col).DrawFlags(DrawMode.INNER);

            }
            public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
            {
                return RegularPolygon(xOffset, yOffset, Data.Color.UIDefault.FG, sides, magnitude);
            }
            public static Multi RegularPolygon(int sides, double magnitude)
            {
                return RegularPolygon(0, 0, sides, magnitude);
            }

            // Create a star with an inner and outer radius
            public static Multi Star(double xOffset, double yOffset, Color col, int sides, double innerRadius, double outerRadius)
            {
                Multi ps = new Multi().Parented(Ref.Origin);
                double angle = 360d / (double)sides;
                for (int i = 0; i < sides; i++)
                {
                    double innerX = innerRadius * Math.Cos(angle * i / 180 * Math.PI);
                    double innerY = innerRadius * Math.Sin(angle * i / 180 * Math.PI);
                    double outerX = outerRadius * Math.Cos((angle * i + angle / 2) / 180 * Math.PI);
                    double outerY = outerRadius * Math.Sin((angle * i + angle / 2) / 180 * Math.PI);
                    ps.Add(Point(innerX, innerY, col));
                    ps.Add(Point(outerX, outerY, col));
                }

                return ps.Positioned(xOffset, yOffset).Colored(col).DrawFlags(DrawMode.INNER);
                //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
            }
            public static Multi Star(double xOffset, double yOffset, int sides, double innerRadius, double outerRadius)
            {
                return Star(xOffset, yOffset, Data.Color.UIDefault.FG, sides, innerRadius, outerRadius);
            }
            public static Multi Star(int sides, double innerRadius, double outerRadius)
            {
                return Star(0, 0, sides, innerRadius, outerRadius);
            }
        }

        // Initializations
        public Spell()
        {
            uiGrid = UI.Presets.Graph.Cartesian();
        }

        public abstract void PreLoop();
        public abstract void Loop();

    }
}