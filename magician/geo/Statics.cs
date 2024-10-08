namespace Magician.Geo;
using Core;
using Core.Maps;

public static class Ref
{
    // Reference to the Origin of the current Spell (see Spellcaster.Load)
    public static Node Origin { get; internal set; }
    public static Node Perspective { get; }
    public static Node Undefined { get; internal set; }
    //public static List<Multi> AllowedOrphans;
    public static Vec3 DefaultHeading = new(0, 0, -1);
    public static Vec3 DefaultUp = new(0, 1, 0);
    public static Vec3 DefaultRight = new(1, 0, 0);
    internal static Node placeholderOrigin = new Node().Tagged("Placeholder Origin").Parented(null);
    public static double FOV
    {
        get => Perspective.Val;
        set => Perspective.Written(value);
    }
    static Ref()
    {
        //Origin = new Node().Tagged("Placeholder Origin");
        Origin = placeholderOrigin;
        // TODO: find out why -399 works here... something to do with FOV
        Perspective = new Node(0, 0, 399).Parented(null);
        FOV = 90;
        Undefined = new Node(double.MaxValue, double.MaxValue, double.MinValue).Tagged("UNDEFINED");

        //AllowedOrphans = new List<Multi>()
        //    {
        //        Origin,
        //        Perspective,
        //        Undefined
        //    };
    }
}
public static class Create
{
    // Create a point
    /* TODO: remove parent arguement from Create methods */
    public static Node Point(Node? parent, double x, double y, double z, Color? col)
    {
        return new Node(parent, x, y, z, col).Flagged(DrawMode.INVISIBLE);
    }
    public static Node Point(Node? parent, double x, double y, Color? col)
    {
        return Point(parent, x, y, 0, col);
    }
    public static Node Point(double x, double y, double z = 0, Color? col = null)
    {
        return Point(Ref.Origin, x, y, z, col).Flagged(DrawMode.INVISIBLE);
    }
    public static Node Point(double x, double y, Color col)
    {
        return Point(x, y, 0, col);
    }
    /*         public static Multi Point(double x, double y, double z=0)
            {
                return Point(Ref.Origin, x, y, z, Data.Col.UIDefault.FG);
            } */

    // Create a line
    public static Node Line(Node p1, Node p2, Color col)
    {
        double x1 = p1.X;
        double y1 = p1.Y;
        double x2 = p2.X;
        double y2 = p2.Y;

        Node line = new Node(0, 0, col, DrawMode.PLOT,
        Point(x1, y1, col),
        Point(x2, y2, col));
        // Make sure the parents are set correctly
        line[0].Parented(line);
        line[1].Parented(line);
        return line;
    }
    public static Node Line(Node p1, Node p2)
    {
        return Line(p1, p2, Runes.Col.UIDefault.FG);
    }

    // TODO: make Rect easier to implement with a Flatten method
    // Rect from 2 points
    public static Node Rect(Node p0, Node p1)
    {
        return Rect(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
    }
    public static Node Rect(double x, double y, double width, double height)
    {
        return new Node(
            Point(width, -height),
            //.Textured(
            //    new Renderer.Text("1", new RGBA(0xff0000e0), 20).Render()
            //),
            Point(width, 0),
            //.Textured(
            //    new Renderer.Text("2", new RGBA(0xff0000e0), 20).Render()
            //),
            Point(0, 0),
            //.Textured(
            //    new Renderer.Text("3", new RGBA(0xff0000e0), 20).Render()
            //),
            Point(0, -height)
        //.Textured(
        //   new Renderer.Text("4", new RGBA(0xff0000e0), 20).Render()
        //)
        ).To(x, y);
    }

    // Create a regular polygon with a position, number of sides, color, and magnitude
    public static Node RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
    {
        //List<Multi> ps = new List<Multi>();
        Node ps = new Node().Parented(Ref.Origin);
        double angle = 360d / (double)sides;
        for (int i = 0; i < sides; i++)
        {
            double x = magnitude * Math.Cos(angle * i / 180 * Math.PI);
            double y = magnitude * Math.Sin(angle * i / 180 * Math.PI);
            ps.Add(Point(ps, x, y, Runes.Col.UIDefault.FG));
        }

        //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        return ps.To(xOffset, yOffset).Colored(col).Flagged(DrawMode.INNER);

    }
    public static Node RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
    {
        return RegularPolygon(xOffset, yOffset, Runes.Col.UIDefault.FG, sides, magnitude);
    }
    public static Node RegularPolygon(int sides, double magnitude)
    {
        return RegularPolygon(0, 0, sides, magnitude);
    }

    // Create a star with an inner and outer radius
    public static Node Star(double xOffset, double yOffset, Color col, int sides, double innerRadius, double outerRadius)
    {
        Node ps = new Node().Parented(Ref.Origin);
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

        return ps.To(xOffset, yOffset).Colored(col).Flagged(DrawMode.INNER);
        //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
    }
    public static Node Star(double xOffset, double yOffset, int sides, double innerRadius, double outerRadius)
    {
        return Star(xOffset, yOffset, Runes.Col.UIDefault.FG, sides, innerRadius, outerRadius);
    }
    public static Node Star(int sides, double innerRadius, double outerRadius)
    {
        return Star(0, 0, sides, innerRadius, outerRadius);
    }

    public static Node Along(Parametric pm, double start, double end, double spacing, Node template)
    {
        Node container = new();
        for (double x = start; x < end; x += spacing)
        {
            List<double> pos = pm.Evaluate(x).Values.Select(d => d).ToList();
            while (pos.Count < 3)
            {
                pos.Add(0);
            }
            container.Add(template.Copy().To(pos[0], pos[1], pos[2]));
        }
        return container;
    }

    /* Create 3D shapes! */
    public static Node TriPyramid(double x, double y, double z, double radius)
    {
        Node tetra = new(x, y, z, Mesh.SimplePyramid,
            Create.RegularPolygon(3, radius)
            .Add(
            new Node[] { Create.Point(0, 0, Math.Sqrt(3) * radius) }
            ).ToArray()
        );
        return tetra;
    }

    public static Node Cube(double x, double y, double z, double radius)
    {
        Node cube = new(x, y, z, Mesh.Cubic,
        Create.RegularPolygon(4, radius).Add(
            Create.RegularPolygon(4, radius)
                .Sub(m => m.Translated(0, 0, radius * Math.Sqrt(2)))  // radius is half the diagonal
                .Constituents.ToArray())  // array of point Multis
            .ToArray()  // array of all 8 points
        );

        return cube;
    }
}

public static class Check
{
    public static bool PointInPolygon(double x, double y, Node polygon)
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
        // TODO: account for perspective
        List<int[]> triangles = Tri.Seidel.Triangulator.Triangulate(polygon);
        foreach (int[] vertexIdx in triangles)
        {
            if (vertexIdx.Length != 3) { Scribe.Issue("Renderer gave bad triangle :("); }
            double x0, y0, x1, y1, x2, y2;
            int idx0 = vertexIdx[0];
            int idx1 = vertexIdx[1];
            int idx2 = vertexIdx[2];

            // If all vertex indices are zero, we're done
            if (idx0 + idx1 + idx2 == 0)
            {
                break;
            }

            // Calculate absolute coordinates of triangle
            x0 = polygon[idx0 - 1].X;
            y0 = polygon[idx0 - 1].Y;
            x1 = polygon[idx1 - 1].X;
            y1 = polygon[idx1 - 1].Y;
            x2 = polygon[idx2 - 1].X;
            y2 = polygon[idx2 - 1].Y;

            // These two vectors add up to the position of the mouse
            double v0 = (x0 * (y2 - y0) + (y - y0) * (x2 - x0) - x * (y2 - y0)) / ((y1 - y0) * (x2 - x0) - (x1 - x0) * (y2 - y0));
            double v1 = (y - y0 - v0 * (y1 - y0)) / (y2 - y0);

            // Point is NOT in triangle
            if (v0 < 0 || v1 < 0 || Math.Abs(v0) > 1 || Math.Abs(v1) > 1 || v0 + v1 > 1 || double.IsNaN(v0) || double.IsNaN(v1))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    public static bool PointInRectVolume(double x, double y, double z, (double, double) xRange, (double, double) yRange, (double, double) zRange)
    {
        return x > xRange.Item1 && x < xRange.Item2
        && y > yRange.Item1 && y < yRange.Item2
        && z > zRange.Item1 && z < zRange.Item2;
    }
    public static bool PointInRectVolume(Node p, (double, double) xRange, (double, double) yRange, (double, double) zRange)
    {
        return PointInRectVolume(p.X, p.Y, p.Z, xRange, yRange, zRange);
    }
    public static bool PointInRect(double pX, double pY, double rX, double rY, double rW, double rH)
    {
        double maxX = rX + rW;
        double maxY = rY + rH;
        return pX >= rX && pX <= maxX && pY >= rY && pY <= maxY;
    }

    public static bool IsRectangle(Node m, double tolerance = Runes.Globals.defaultTol)
    {
        if (m.Count != 4)
        {
            return false;
        }
        Node v0 = m[0];

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
    public static double Distance(Node m0, Node m1)
    {
        return Distance(m0.X, m1.X, m0.Y, m1.Y);
    }
    public static double Length(Node m)
    {
        if (m.Count != 2)
        {
            throw new NotImplementedException("Given Multi was not a Line!");
        }
        return Distance(m[0], m[1]);
    }

    public static Vec OOBVector(double x, double y, double z, (double, double) xRange, (double, double) yRange, (double, double) zRange)
    {
        return new(x > 0 ? -(xRange.Item1 - x) : xRange.Item2 - x,
        y > 0 ? -(yRange.Item1) - y : yRange.Item2 - y,
        z > 0 ? -(zRange.Item1) - z : zRange.Item2 - z);
    }
    public static Vec OOBVector(Vec v, (double, double) xRange, (double, double) yRange, (double, double) zRange)
    {
        return OOBVector(v.x.Get(), v.y.Get(), v.z.Get(), xRange, yRange, zRange);
    }

}

public static class Unit
{
    public static Vec Circle(double phase, double radius = 1)
    {
        double x = radius * Math.Cos(phase);
        double y = radius * Math.Sin(phase);
        return new Vec(x, y);
    }

}


