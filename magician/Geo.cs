using System;

namespace Magician
{
    public static class Geo
    {
        // The Origin is the eventual parent Multi for all Multis
        public static Multi Origin = Point(null, 0, 0, Ref.UIDefault.FG).DrawFlags(DrawMode.INVISIBLE);

        // Create a point
        public static Multi Point(Multi? parent, double x, double y, Color col)
        {
            return new Multi(parent, x, y, col).DrawFlags(DrawMode.POINT);
        }
        public static Multi Point(double x, double y, Color col)
        {
            return new Multi(x, y, col).DrawFlags(DrawMode.POINT);
        }
        public static Multi Point(double x, double y)
        {
            return Point(x, y, Ref.UIDefault.FG);
        }

        // Create a line
        public static Multi Line(Multi p1, Multi p2, Color col)
        {
            double x1 = p1.X.Evaluate();
            double y1 = p1.Y.Evaluate();
            double x2 = p2.X.Evaluate();
            double y2 = p2.Y.Evaluate();

            return new Multi(x1, y1, col, DrawMode.PLOT,
            Point(0, 0, col),
            Point(x2 - x1, y2 - y1, col));
        }
        public static Multi Line(Multi p1, Multi p2)
        {
            return Line(p1, p2, Ref.UIDefault.FG);
        }

        // Create a regular polygon with a position, number of sides, color, and magnitude
        public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
        {
            List<Multi> ps = new List<Multi>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude * Math.Cos(angle * i / 180 * Math.PI);
                double y = magnitude * Math.Sin(angle * i / 180 * Math.PI);
                ps.Add(Point(x, y, col));
            }

            return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Ref.UIDefault.FG, sides, magnitude);
        }
        public static Multi RegularPolygon(int sides, double magnitude)
        {
            return RegularPolygon(0, 0, sides, magnitude);
        }

        // Create a star with an inner and outer radius
        public static Multi Star(double xOffset, double yOffset, Color col, int sides, double innerRadius, double outerRadius)
        {
            List<Multi> ps = new List<Multi>();
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

            return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        }
        public static Multi Star(double xOffset, double yOffset, int sides, double innerRadius, double outerRadius)
        {
            return Star(xOffset, yOffset, Ref.UIDefault.FG, sides, innerRadius, outerRadius);
        }
        public static Multi Star(int sides, double innerRadius, double outerRadius)
        {
            return Star(0, 0, sides, innerRadius, outerRadius);
        }
    }
}
