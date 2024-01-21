namespace Magician.Paint;
using Geo;
using Silk.NET.OpenGL;
using Silk.NET.Maths;

public static class Renderer
{
    static SdlContext? sdlContext;
    static GL? gl;
    static bool saveFrame = false;
    static int saveCount = 0;
    static IntPtr target;
    readonly static List<RDrawable> drawables = new();

    public static GL GL { get => gl ?? throw Scribe.Error("Null GL renderer"); set => gl = value; }
    public static SdlContext SDL { get => sdlContext ?? throw Scribe.Error("Null SDL context"); set => sdlContext = value; }
    public static List<RDrawable> Drawables => drawables;
    public static bool Render { get; set; }
    public static bool Display { get; set; }

    public static void Clear()
    {
        Clear(Runes.Col.UIDefault.BG);
    }
    public static void Clear(Color c)
    {
        if (gl is null)
            throw Scribe.Error("Cannot clear uninitialized gl context");

        gl.ClearColor((float)c.R / 255f, (float)c.G / 255f, (float)c.B / 255f, (float)c.A / 255f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
    }
    public static void DrawAll()
    {
        foreach (RDrawable rd in drawables)
        {
            rd.Draw();
        }
    }
}

public static class Render
{
    public static void Polygon(double[][] vertices, DrawMode drawMode = DrawMode.OUTERP, List<Color>? cols=null, Node? cache=null)
    {
        cols ??= Enumerable.Range(0,vertices.Length).Select(n => Runes.Col.UIDefault.FG).ToList();
        // Draw points
        if ((drawMode & DrawMode.POINTS) > 0)
        {
            int numPoints = vertices.Length;
            RPoint[] rPointArray = new RPoint[numPoints];
            RPoints rPoints;

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = new RPoint(vertices[i][0], vertices[i][1], vertices[i][2],
                    cols[i].R, cols[i].G, cols[i].B, cols[i].A);
            }
            rPoints = new(rPointArray);
            cache?.drawables.Add(rPoints);
            Renderer.Drawables.Add(rPoints);
        }

        // Draw lines
        if ((drawMode & DrawMode.PLOT) > 0)
        {
            bool connected = (drawMode & DrawMode.CONNECTINGLINE) > 0 && vertices.Length >= 3;
            int numLines = vertices.Length - (connected ? 0 : 1);
            if (numLines < 1)
                return;

            RLine[] rLineArray = new RLine[numLines];
            RLines rLines;

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                double x0 = vertices[i][0]; double x1 = vertices[i + 1][0];
                double y0 = vertices[i][1]; double y1 = vertices[i + 1][1];
                double z0 = vertices[i][2]; double z1 = vertices[i + 1][2];
                rLineArray[i] = new RLine(x0, y0, z0, x1, y1, z1, cols[i].R, cols[i].G, cols[i].B, cols[i].A);
            }
            // If the Multi is a closed shape, connect the first and last constituent with a line
            if (connected)
            {
                double[] pLast = vertices[^1];
                double[] pFirst = vertices[0];

                double subr = cols[^1].R;
                double subg = cols[^1].G;
                double subb = cols[^1].B;
                double suba = cols[^1].A;

                rLineArray[^1] = new RLine(pLast[0], pLast[1], pLast[2], pFirst[0], pFirst[1], pFirst[2], subr, subb, subg, suba);
            }
            rLines = new(rLineArray);
            cache?.drawables.Add(rLines);
            Renderer.Drawables.Add(rLines);
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        if (((drawMode & DrawMode.INNER) > 0) && vertices.Length >= 3)
        {
            List<int> ect = EarCut.Triangulate(vertices);
            if (ect.Count % 3 != 0)
            {
                throw Scribe.Issue("Triangulator returned non-triplet vertices");
            }
            List<int[]> triVertexIndices = new();
            for (int i = 0; i < ect.Count; i += 3)
            {
                triVertexIndices.Add(new int[] { ect[i], ect[i + 1], ect[i + 2] });
            }

            int numTriangles = triVertexIndices.Count;
            RTriangle[] rTriArray = new RTriangle[numTriangles];
            RTriangles rTris;

            for (int i = 0; i < numTriangles; i++)
            {
                int[] vertexIndices = triVertexIndices[i];
                int tri0 = vertexIndices[0];
                int tri1 = vertexIndices[1];
                int tri2 = vertexIndices[2];

                RTriangle rTri = new(
                    vertices[tri0][0], vertices[tri0][1], vertices[tri0][2],
                    vertices[tri1][0], vertices[tri1][1], vertices[tri1][2],
                    vertices[tri2][0], vertices[tri2][1], vertices[tri2][2],
                    cols[i].R, cols[i].G, cols[i].B, cols[i].A
                );
                rTriArray[i] = rTri;
            }

            rTris = new RTriangles(rTriArray);
            cache?.drawables.Add(rTris);
            Renderer.Drawables.Add(rTris);
        }
    }

    public static void Body(Geo.Mesh mesh, params IEnumerable<double>[] vertices)
    {
        //
    }

    public static List<double[]> Project(IEnumerable<Core.Vec> n, double xOffset, double yOffset, double zOffset, Node? camera=null)
    {
        List<double[]> projectedVerts = new();// double[n.Count][];
        // These two vectors define the camera
        camera ??= Ref.Perspective;
        Vec3 targV = new Core.Vec((Core.IVec)camera + camera.Heading).ToVec3();
        Vec3 upV = targV.YawPitchRotated(0, Math.PI / 2);

        // TODO: move these out of the loop
        // Matrix magic
        Matrix4X4<double> view = Matrix4X4.CreateLookAt<double>(
            new(Ref.Perspective.X, Ref.Perspective.Y, Ref.Perspective.Z),
            new(targV.x.Get(), targV.y.Get(), targV.z.Get()),
            new(0, 1, 0)
        );
        Matrix4X4<double> projection = Matrix4X4.CreatePerspectiveFieldOfView<double>(
            Ref.FOV / 180d * Math.PI,
            Runes.Globals.winWidth / Runes.Globals.winHeight,
            0.1, 2000
        );

        foreach (Core.Vec v in n)
        {
            Vector3D<double> worldCoords = new(
                v.x.Get()+xOffset,
                v.y.Get()+yOffset,
                v.z.Get()+zOffset
            );
            Vector4D<double> intermediate = Vector4D.Transform<double>(worldCoords, view);
            Vector4D<double> final = Vector4D.Transform<double>(intermediate, projection);

            // Format the projected vertices for GLSL
            projectedVerts.Add(new double[]
            {
                final.X/-final.Z,
                final.Y/-final.Z,
                -final.Z,
                1+0*final.W
            });
        }
        return projectedVerts;
    }    
    public static List<double[]> Cull(Node n, double xOffset, double yOffset, double zOffset, List<double[]> vertices, int[]? face=null)
    {
        List<double[]> clippedVerts = new();
        // Camera-axis culling
        int counter = 0;
        foreach (double[] v in vertices)
        {
            // Check to see if the constituent's z-coordinate is out-of-bounds
            // It is considered OOB when it is not in front of the camera along the axis parallel to the camera
            bool zInBounds;

            Vec3 absPos;
            if (face is not null)
                absPos = new(n[face[counter]].x.Get()+xOffset+n.x.Get(), n[face[counter]].y.Get()+yOffset+n.y.Get(), n[face[counter]].z.Get()+zOffset+n.z.Get());
            else
                absPos = new(n[counter].x.Get()+xOffset+n.x.Get(), n[counter].y.Get()+yOffset+n.y.Get(), n[counter].z.Get()+zOffset+n.z.Get());


            Vec3 camPos = Ref.Perspective;
            // Rotate so that we can compare straight along the axis using a >=
            absPos = absPos.YawPitchRotated(Ref.Perspective.yaw, -Ref.Perspective.pitch);
            camPos = camPos.YawPitchRotated(Ref.Perspective.yaw, -Ref.Perspective.pitch);
            zInBounds = absPos.z.Get() - camPos.z.Get() >= 0;

            if (zInBounds)
            {
                clippedVerts.Add(v);
            }
            else
            {
                // Seems to work fine without calculating clipping intersections, so do nothing
            }
            counter++;
        }
        return clippedVerts;
    }
}