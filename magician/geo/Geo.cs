using System;
using Magician.Library;
using Magician.UI;

namespace Magician.Geo
{
    public static class Ref
    {
        // Reference to the Origin of the current Spell (see Spellcaster.Load)
        public static Multi Origin { get; internal set; }
        public static Multi Perspective { get; }
        public static List<Multi> AllowedOrphans;
        public static double FOV
        {
            get => Perspective.Read();
            set => Perspective.Written(value);
        }
        static Ref()
        {
            Origin = new Multi().Tagged("Placeholder Origin");
            Perspective = new Multi(0, 0, -1).Parented(null);
            FOV = 90;

            AllowedOrphans = new List<Multi>()
            {
                Origin,
                Perspective
            };
        }
    }
    public static class Create
    {
        // Create a point
        /* TODO: remove parent arguement from Create methods */
        public static Multi Point(Multi? parent, double x, double y, double z, Color? col)
        {
            return new Multi(parent, x, y, z, col).SetDraw(DrawMode.INVISIBLE);
        }
        public static Multi Point(Multi? parent, double x, double y, Color? col)
        {
            return Point(parent, x, y, 0, col);
        }
        public static Multi Point(double x, double y, double z = 0, Color? col = null)
        {
            return Point(Ref.Origin, x, y, z, col).SetDraw(DrawMode.INVISIBLE);
        }
        public static Multi Point(double x, double y, Color col)
        {
            return Point(x, y, 0, col);
        }
        /*         public static Multi Point(double x, double y, double z=0)
                {
                    return Point(Ref.Origin, x, y, z, Data.Col.UIDefault.FG);
                } */

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

        // TODO: make Rect easier to implement with a Flatten method
        // Rect from 2 points
        public static Multi Rect(Multi p0, Multi p1)
        {
            return Rect(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
        }
        public static Multi Rect(double x, double y, double width, double height)
        {
            return new Multi(
                Point(width, -height).Textured(new Renderer.Text("1", new RGBA(0xff0000e0), 20).Render()),
                Point(width, 0).Textured(new Renderer.Text("2", new RGBA(0xff0000e0), 20).Render()),
                Point(0, 0).Textured(new Renderer.Text("3", new RGBA(0xff0000e0), 20).Render()),
                Point(0, -height).Textured(new Renderer.Text("4", new RGBA(0xff0000e0), 20).Render())
            ).Positioned(x, y);
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
            return ps.Positioned(xOffset, yOffset).Colored(col).SetDraw(DrawMode.INNER);

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

            return ps.Positioned(xOffset, yOffset).Colored(col).SetDraw(DrawMode.INNER);
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

        // This approach to 3d objects is impractical
        // For more info, see Multi3D
        public static Multi ImpractiCube(double x, double y, double z, double radius)
        {
            Multi cube = new Multi(x, y, z);

            cube.Add(new Multi(
                Point(radius / 2, radius / 2, 0),
                Point(-radius / 2, radius / 2, 0),
                Point(-radius / 2, -radius / 2, 0),
                Point(radius / 2, -radius / 2, 0)
            ).Positioned(0, 0, 0).SetDraw(DrawMode.FULL).Colored(new RGBA(0xff0000d0)));
            cube.Add(new Multi(
                Point(radius / 2, radius / 2, 0),
                Point(-radius / 2, radius / 2, 0),
                Point(-radius / 2, -radius / 2, 0),
                Point(radius / 2, -radius / 2, 0)
            ).Positioned(0, 0, -radius / 2).SetDraw(DrawMode.FULL).Colored(new RGBA(0x00ffffd0)));

            cube.Add(new Multi(
                Point(0, radius / 2, radius / 2),
                Point(0, radius / 2, -radius / 2),
                Point(0, -radius / 2, -radius / 2),
                Point(0, -radius / 2, radius / 2)
            ).Positioned(radius / 2, 0, 0).SetDraw(DrawMode.FULL).Colored(new RGBA(0xffff00d0)));
            cube.Add(new Multi(
                Point(0, radius / 2, radius / 2),
                Point(0, radius / 2, -radius / 2),
                Point(0, -radius / 2, -radius / 2),
                Point(0, -radius / 2, radius / 2)
            ).Positioned(-radius / 2, 0, 0).SetDraw(DrawMode.FULL).Colored(new RGBA(0x0000ffd0)));

            cube.Add(new Multi(
                Point(radius / 2, 0, radius / 2),
                Point(-radius / 2, 0, radius / 2),
                Point(-radius / 2, 0, -radius / 2),
                Point(radius / 2, 0, -radius / 2)
            ).Positioned(0, radius / 2, 0).SetDraw(DrawMode.FULL).Colored(new RGBA(0x00ff00d0)));
            cube.Add(new Multi(
                Point(radius / 2, 0, radius / 2),
                Point(-radius / 2, 0, radius / 2),
                Point(-radius / 2, 0, -radius / 2),
                Point(radius / 2, 0, -radius / 2)
            ).Positioned(0, -radius / 2, 0).SetDraw(DrawMode.FULL).Colored(new RGBA(0xff00ffd0)));

            return cube.SetDraw(DrawMode.INVISIBLE);
        }

        /* Create 3D shapes! */
        public static Multi3D Tetrahedron(double x, double y, double z, double radius)
        {
            Multi3D tetra = new Multi3D(x, y, z,
                Create.RegularPolygon(3, radius)
                .Add(
                new Multi[] { Create.Point(0, 0, radius) }
                ).ToArray())
            .FacesSimplex();
            return tetra;
        }

        public static Multi3D Cube(double x, double y, double z, double radius)
        {
            Multi3D cube = new Multi3D(x, y, z,
                Create.RegularPolygon(4, radius)
                .Add(
         Create.RegularPolygon(4, radius).Sub(m => m.Translated(0, 0, -radius * 2)).Constituents.ToArray()
                ).ToArray())
            .FacesCube();
            return cube;
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
            List<int[]> triangles = Seidel.Triangulator.Triangulate(polygon);
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

        //public static double
    }

    public class Matrix
    {
        public int width;
        public int height;
        double[,] mx;

        public double Get(int row, int col)
        {
            return mx[row, col];
        }

        public double X => Get(0, 0);
        public double Y => Get(0, 1);
        public double Z => Get(0, 2);

        public Matrix(double[,] mx)
        {
            height = mx.GetLength(0); // rows
            width = mx.GetLength(1);  // columns
            this.mx = mx;
        }
        public Matrix(Multi m) : this(new double[,] { { m.X, 0, 0 }, { 0, m.Y, 0 }, { 0, 0, m.Z } }) { }
        public static Matrix Row(Multi m) { return new Matrix(new double[,] { { m.X, m.Y, m.Z } }); }

        public Matrix Mult(Matrix mox)
        {
            if (mox.width != height)
            {
                throw Scribe.Error($"Columns of {mox} must match rows of {this}");
            }

            double[,] result = new double[mox.height, width];
            for (int row = 0; row < mox.height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    double sum = 0;
                    for (int k = 0; k < width; k++)
                    {
                        sum += mox.Get(row, k) * Get(k, col);
                    }
                    result[row, col] = sum;
                }
            }
            return new Matrix(result);
        }

        public static Matrix RotationX(double theta)
        {
            return new Matrix(new double[,]
            {
                { 1, 0, 0 },
                { 0, Math.Cos(theta), -Math.Sin(theta) },
                { 0, Math.Sin(theta), Math.Cos(theta) }
            });
        }
        public static Matrix RotationY(double theta)
        {
            return new Matrix(new double[,]
            {
                { Math.Cos(theta), 0, Math.Sin(theta) },
                { 0, 1, 0 },
                { -Math.Sin(theta), 0, Math.Cos(theta) }
            });
        }
        public static Matrix RotationZ(double theta)
        {
            return new Matrix(new double[,]
            {
                { Math.Cos(theta), -Math.Sin(theta), 0 },
                { Math.Sin(theta), Math.Cos(theta), 0 },
                { 0, 0, 1 }
            });
        }
        public static Matrix Orthographic = new Matrix(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } });
        // TODO: fix this
        public static Matrix Isometric = new Matrix(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });
        /* public static Matrix Perspective(double x, double y, double z)
        {

            double s = 1 / Math.Tan(Ref.FOV * Math.PI / 360);
            double xScale = 1 / Data.Globals.winWidth;
            double yScale = 1 / Data.Globals.winHeight;
            double aspect = (double)Data.Globals.winHeight / Data.Globals.winWidth;

            double zNear = 1; // Offset so that the camera isn't "inside your head", if you get what I mean
            double zFar = zNear+0;// - Ref.Perspective.Z;

            double xp, yp, zp;
            xp = x + Ref.Perspective.X;     
            yp = y + Ref.Perspective.Y;     
            zp = z - Ref.Perspective.Z;     
            
            return new Matrix(new double[,]
            {
                {s*xp/zp, 0, 0},
                {0, s*yp/zp, 0},
                {0, 0, zFar/(zFar-zNear)}
            })
            .Mult(RotationX(Ref.Perspective.HeadingX))
            .Mult(RotationY(Ref.Perspective.HeadingY))
            .Mult(RotationZ(Ref.Perspective.HeadingZ))
            ;

        } */

        public static Matrix Perspective
        {
            get
            {
                double s = 1 / Math.Tan(Ref.FOV * Math.PI / 360);
                double aspect = (double)Data.Globals.winHeight / Data.Globals.winWidth;

                double zNear = 1; // Offset so that the camera isn't "inside your head", if you get what I mean
                double zFar = 101;//zNear - Ref.Perspective.Z;
                if (zFar == zNear)
                {
                    zFar = Double.NegativeInfinity;
                }

                return new Matrix(new double[,]
                {
                {s*aspect, 0, 0},
                {0, s, 0},
                {0, 0, zFar/(zFar-zNear)}
                })
                //.Mult(RotationX(Ref.Perspective.HeadingX))
                //.Mult(RotationY(Ref.Perspective.HeadingY))
                //.Mult(RotationZ(Ref.Perspective.HeadingZ))
                ;
            }

        }

        public override string ToString()
        {
            string s = "";
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    s += $"{Get(row, col)} ";
                }
                s += "\n";
            }
            return s;
        }

        internal Matrix ToCartesian(double xOffset, double yOffset)
        {
            for (int row = 0; row < height; row++)
            {
                mx[row, 0] = mx[row, 0] + Data.Globals.winWidth / 2 + xOffset;
                mx[row, 1] = -mx[row, 1] + Data.Globals.winHeight / 2 + yOffset;
            }
            return this;
        }
    }

}
