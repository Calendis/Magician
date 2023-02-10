using System;
using Magician.Library;

namespace Magician.Geo
{
    public static class Ref
    {
        // Reference to the Origin of the current Spell (see Spellcaster.Load)
        public static Multi Origin
        {
            get; set;
        }
        static Ref()
        {
            Origin = new Multi().Tagged("new Origin");
        }
    }
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
            return Point(Ref.Origin, x, y, Data.Col.UIDefault.FG);
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
            line[0].Parented(line);
            line[1].Parented(line);
            return line;
        }
        public static Multi Line(Multi p1, Multi p2)
        {
            return Line(p1, p2, Data.Col.UIDefault.FG);
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
                ps.Add(Point(ps, x, y, Data.Col.UIDefault.FG));
            }

            //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
            return ps.Positioned(xOffset, yOffset).Colored(col).DrawFlags(DrawMode.INNER);

        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Data.Col.UIDefault.FG, sides, magnitude);
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
            return Star(xOffset, yOffset, Data.Col.UIDefault.FG, sides, innerRadius, outerRadius);
        }
        public static Multi Star(int sides, double innerRadius, double outerRadius)
        {
            return Star(0, 0, sides, innerRadius, outerRadius);
        }
    }

    public static class Check
    {
        // TODO: implement this by making the triangulated triangles global and linking each Multi to its set of vertices
        public static bool PointInPolygon(double x, double y, Multi polygon)
        {
            // Special case for rectangles
            if (IsRectangle(polygon))
            {
                double minX = Math.Min(polygon[0].X, polygon[2].X);
                double xRange = Math.Max(
                   Math.Abs(polygon[0].X - polygon[1].X),
                   Math.Abs(polygon[0].X - polygon[3].X)
                );
                double minY = Math.Min(polygon[0].Y, polygon[2].Y);
                double yRange = Math.Max(
                   Math.Abs(polygon[0].Y - polygon[1].Y),
                   Math.Abs(polygon[0].Y - polygon[3].Y)
                );
                return (x >= minX) && (x < minX + xRange) && (y >= minY) && (y < minY + yRange);
            }
            
            // For other shapes, grab the triangles from Siedel's algo and check each
            List<int[]> vertices = Seidel.Triangulator.Triangulate(polygon);
            foreach (int[] idxTriangle in vertices)
            {
                if (idxTriangle.Length != 3) {throw new InvalidDataException("bad triangle :(");}
            }
            
            throw new NotImplementedException("doesn't work yet, file an issue at https://github.com/Calendis/Magician");
        }

        public static bool IsRectangle(Multi m, double tolerance = Data.Globals.defaultTol)
        {
            if (m.Count != 4)
            {
                return false;
            }
            Multi v0 = m[0];

            // Either the x or the y of the first must match the x or y of the neighbour, within a tolerance
            return (Math.Abs(m[0].X - m[1].X) <= tolerance) ||
                    (Math.Abs(m[0].Y - m[1].Y) <= tolerance);
        }
    }

    public static class Find
    {
        /* Find the Euclidian distance */
        public static double Distance(double x0, double x1, double y0, double y1)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static double Distance(Multi m0, Multi m1)
        {
            return Distance(m0.X, m1.X, m0.Y, m1.Y);
        }
        public static double Distance(Multi m)
        {
            if (m.Count != 2)
            {
                throw new NotImplementedException("Given Multi was not a Line!");
            }
            return Distance(m[0], m[1]);
        }
    }
}
