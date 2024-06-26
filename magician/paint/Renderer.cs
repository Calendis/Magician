namespace Magician.Paint;
using Geo;
using Silk.NET.OpenGL;
using Silk.NET.Maths;

public static class Renderer
{
    static SdlContext? sdlContext;
    static GL? gl;
    static bool saveFrame = false;
    readonly static List<(byte[], float[])> points = new();
    // TODO: support second colour for lines
    readonly static List<(byte[], float[], float[])> lines = new();
    readonly static List<(byte[], byte[], byte[], float[], float[], float[])> tris = new();
    public static GL GL { get => gl ?? throw Scribe.Error("Null GL renderer"); set => gl = value; }
    public static SdlContext SDL { get => sdlContext ?? throw Scribe.Error("Null SDL context"); set => sdlContext = value; }
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
        PrepareMatrices();
        Draw.Points(points);
        Draw.Lines(lines);
        Draw.Triangles(tris);
    }

    public static class Drawables
    {
        internal static void Add(params (byte[], float[])[] ps) { points.AddRange(ps); }
        internal static void Add(params (byte[], float[], float[])[] ls) { lines.AddRange(ls); }
        internal static void Add(params (byte[], byte[], byte[], float[], float[], float[])[] ts) { tris.AddRange(ts); }
        public static void Clear()
        {
            points.Clear();
            lines.Clear();
            tris.Clear();
        }
    }

    public static unsafe void PrepareMatrices()
    {
        Node camera = Ref.Perspective;
        double targX = camera.X + camera.Heading.X;
        double targY = camera.Y + camera.Heading.Y;
        double targZ = camera.Z + camera.Heading.Z;

        Vector3D<double> defaultUp = new(0, 1, 0);
        Vector3D<double> upV = Vector3D.Transform(defaultUp, camera.Rotation);

        Matrix4X4<float> mview = Matrix4X4.CreateLookAt<float>(
            new((float)camera.X, (float)camera.Y, (float)camera.Z),
            new((float)targX, (float)targY, (float)targZ),
            new((float)upV.X, (float)upV.Y, (float)upV.Z)
        );
        Matrix4X4<float> mproj = Matrix4X4.CreatePerspectiveFieldOfView<float>(
            (float)(Ref.FOV / 180f * Math.PI),
            (float)(Runes.Globals.winWidth / Runes.Globals.winHeight),
            0.1f, 2000f
        );

        int viewLoc = GL.GetUniformLocation(Shaders.shaders[Shaders.Current].prog, "view");
        int projLoc = GL.GetUniformLocation(Shaders.shaders[Shaders.Current].prog, "proj");
        if (viewLoc == -1 || projLoc == -1) { throw Scribe.Issue("Could not find uniforms within shader!"); }

        GL.UniformMatrix4(viewLoc, 1, false, &mview.Row1.X);
        GL.UniformMatrix4(projLoc, 1, false, &mproj.Row1.X);
    }
}

public static class Render
{
    public static Stack<(int a, int b)> BufAllocIdxs = new();
    static Dictionary<Node, List<(int, int)>> previousAllocIdxs = new();
    static int bufIdx = 0;
    static int cacheCount = 0;

    public static void Cache(Node n)
    {
        Scribe.Info($"Distributing indices to {n}");
        (int start, int tentativeSize) = BufAllocIdxs.Pop();
        Scribe.Info($"\tstart, tenative size: ({start}, {tentativeSize})");
        if (start == 0)
        {
            n.size = (start, cacheCount);
            n.stale = false;
            Scribe.Info($"\tstart, end: ({start}, {cacheCount})");
        }
        else if (tentativeSize == 0)
        {
            if (!previousAllocIdxs.ContainsKey(n.Parent))
            {
                previousAllocIdxs.Add(n.Parent, new List<(int, int)> { (start, 0) });
            }
            else
            {
                previousAllocIdxs[n.Parent].Add((start, 0));
            }
            //previousAllocIdxs.((start, 0));
            n.size = (start, start);
            n.stale = false;
            Scribe.Info($"\tstart, end: ({start}, {start})");
        }
        else if (tentativeSize == previousAllocIdxs[n].Count)
        {
            foreach ((int st, int en) in previousAllocIdxs[n])
            {
                tentativeSize += en;
            }
            previousAllocIdxs[n].Clear();
            int end = tentativeSize + start;
            if (!previousAllocIdxs.ContainsKey(n.Parent))
            {
                previousAllocIdxs.Add(n.Parent.Parent, new List<(int, int)> { (start, tentativeSize) });
            }
            else
            {
                previousAllocIdxs[n].Add((start, tentativeSize));
            }
            //previousAllocIdxs.Add((start, tentativeEnd));
            n.size = (start, end);
            n.stale = false;
            Scribe.Info($"\tstart, end: ({start}, {end})");

        }
        else
        {
            throw Scribe.Issue($"No match in render cache! start: {start}, tentative size: {tentativeSize}, prevAllocs: {previousAllocIdxs[n.Parent].Count}");
        }
        cacheCount++;
    }

    // TODO: move code from Polygon
    public static void PreCache()
    {
        //
    }
    public static void PostCache()
    {
        if (BufAllocIdxs.Count != 0) { Scribe.Issue($"Misalignment while caching render! ({BufAllocIdxs.Count} remaining)"); }
        //BufAllocIdxs.Clear();
        bufIdx = 0;
        cacheCount = 0;
    }

    public static void Polygon(List<double[]> vertices, Node toCache, DrawMode drawMode = DrawMode.OUTERP, List<Color>? cols = null, List<int[]>? faces = null)
    {
        cols ??= Enumerable.Range(0, vertices.Count).Select(n => Runes.Col.UIDefault.FG).ToList();
        int numConstituents = toCache.Count;
        // numConstituents is a TENTATIVE value, the lower possible bound of the Node
        // the proper value will be calculated during the Pop phase, in the Cache function
        BufAllocIdxs.Push((bufIdx, numConstituents));
        bufIdx++;

        // If there are faces, rearrange the provided vertices
        if (faces is not null)
            throw Scribe.Issue($"TODO: support caching of Nodes with faces");


        // Render points
        if ((drawMode & DrawMode.POINTS) > 0)
        {
            int numPoints = vertices.Count;
            (byte[], float[])[] rPointArray = new (byte[], float[])[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = (new byte[] { (byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A }, new float[] { (float)vertices[i][0], (float)vertices[i][1], (float)vertices[i][2] });
            }
            Renderer.Drawables.Add(rPointArray);
        }

        // Render lines and add geometry to array
        if ((drawMode & DrawMode.PLOT) > 0)
        {
            bool connected = (drawMode & DrawMode.CONNECTINGLINE) > 0 && vertices.Count >= 3;
            int numLines = vertices.Count - (connected ? 0 : 1);
            if (numLines < 1)
                return;

            (byte[], float[], float[])[] rLineArray = new (byte[], float[], float[])[numLines];
            //RLines rLines;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                double x0 = vertices[i][0]; double x1 = vertices[i + 1][0];
                double y0 = vertices[i][1]; double y1 = vertices[i + 1][1];
                double z0 = vertices[i][2]; double z1 = vertices[i + 1][2];
                rLineArray[i] = (new byte[] { (byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A }, new float[] { (float)x0, (float)y0, (float)z0 }, new float[] { (float)x1, (float)y1, (float)z1 });
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

                rLineArray[^1] = (new byte[] { (byte)subr, (byte)subg, (byte)subb, (byte)suba }, new float[] { (float)pLast[0], (float)pLast[1], (float)pLast[2] }, new float[] { (float)pFirst[0], (float)pFirst[1], (float)pFirst[2] });
            }
            Renderer.Drawables.Add(rLineArray);
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        if (((drawMode & DrawMode.INNER) > 0) && vertices.Count >= 3)
        {
            List<int> ect = EarCut.Triangulate(vertices);
            if (ect.Count % 3 != 0) { throw Scribe.Issue("Triangulator returned non-triplet vertices"); }
            List<int[]> triVertexIndices = new();
            for (int i = 0; i < ect.Count; i += 3)
            {
                triVertexIndices.Add(new int[] { ect[i], ect[i + 1], ect[i + 2] });
            }

            int numTriangles = triVertexIndices.Count;
            (byte[], byte[], byte[], float[], float[], float[])[] rTriArray = new (byte[], byte[], byte[], float[], float[], float[])[numTriangles];

            for (int i = 0; i < numTriangles; i++)
            {
                int[] vertexIndices = triVertexIndices[i];
                int tri0 = vertexIndices[0];
                int tri1 = vertexIndices[1];
                int tri2 = vertexIndices[2];

                rTriArray[i] = (
                    new byte[] { (byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A },
                    new byte[] { (byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A },
                    new byte[] { (byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A },
                    new float[] { (float)vertices[tri0][0], (float)vertices[tri0][1], (float)vertices[tri0][2] },
                    new float[] { (float)vertices[tri1][0], (float)vertices[tri1][1], (float)vertices[tri1][2] },
                    new float[] { (float)vertices[tri2][0], (float)vertices[tri2][1], (float)vertices[tri2][2] }
                );
            }

            Renderer.Drawables.Add(rTriArray);
        }
    }
}

public static class Draw
{
    public static void Points(List<(byte[] rgba, float[] pos)> points)
    {
        int dataLength = RDrawData.posLength + RDrawData.colLength;
        float[] vertices;

        int numPts = points.Count;
        vertices = new float[numPts * dataLength];

        for (int i = 0; i < numPts; i++)
        {
            vertices[dataLength * i + 0] = points[i].pos[0];
            vertices[dataLength * i + 1] = points[i].pos[1];
            vertices[dataLength * i + 2] = points[i].pos[2];
            // Color
            vertices[dataLength * i + 3] = points[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = points[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = points[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = points[i].rgba[3] / 255f;
        }

        (uint vao, uint vbo) = Shaders.Prepare(vertices!, new int[] { RDrawData.posLength, RDrawData.colLength });
        Renderer.GL.DrawArrays(GLEnum.Points, 0, (uint)vertices!.Length);
        Shaders.Post(vao, vbo);
    }
    public static void Lines(List<(byte[] rgba, float[] p0, float[] p1)> lines)
    {
        int dataLength = 2 * (RDrawData.posLength + RDrawData.colLength);
        float[] vertices;

        int numLines = lines.Count;
        vertices = new float[numLines * dataLength];
        for (int i = 0; i < numLines; i++)
        {
            vertices[dataLength * i + 0] = lines[i].p0[0];
            vertices[dataLength * i + 1] = lines[i].p0[1];
            vertices[dataLength * i + 2] = lines[i].p0[2];
            vertices[dataLength * i + 7] = lines[i].p1[0];
            vertices[dataLength * i + 8] = lines[i].p1[1];
            vertices[dataLength * i + 9] = lines[i].p1[2];

            vertices[dataLength * i + 3] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = lines[i].rgba[3] / 255f;
            vertices[dataLength * i + 10] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i + 11] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i + 12] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i + 13] = lines[i].rgba[3] / 255f;
        }

        (uint vao, uint vbo) = Shaders.Prepare(vertices!, new int[] { RDrawData.posLength, RDrawData.colLength });
        Renderer.GL.DrawArrays(GLEnum.Lines, 0, (uint)vertices!.Length);
        Shaders.Post(vao, vbo);
    }
    public static void Triangles(List<(byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2)> tris)
    {
        int dataLength = 3 * (RDrawData.posLength + RDrawData.colLength);
        float[] vertices;

        int numTriangles = tris.Count;
        vertices = new float[numTriangles * dataLength];
        for (int i = 0; i < numTriangles; i++)
        {
            vertices[dataLength * i + 0] = tris[i].p0[0];
            vertices[dataLength * i + 1] = tris[i].p0[1];
            vertices[dataLength * i + 2] = tris[i].p0[2];

            vertices[dataLength * i + 7] = tris[i].p1[0];
            vertices[dataLength * i + 8] = tris[i].p1[1];
            vertices[dataLength * i + 9] = tris[i].p1[2];

            vertices[dataLength * i + 14] = tris[i].p2[0];
            vertices[dataLength * i + 15] = tris[i].p2[1];
            vertices[dataLength * i + 16] = tris[i].p2[2];

            vertices[dataLength * i + 3] = tris[i].rgba0[0] / 255f;
            vertices[dataLength * i + 4] = tris[i].rgba0[1] / 255f;
            vertices[dataLength * i + 5] = tris[i].rgba0[2] / 255f;
            vertices[dataLength * i + 6] = tris[i].rgba0[3] / 255f;

            vertices[dataLength * i + 10] = tris[i].rgba1[0] / 255f;
            vertices[dataLength * i + 11] = tris[i].rgba1[1] / 255f;
            vertices[dataLength * i + 12] = tris[i].rgba1[2] / 255f;
            vertices[dataLength * i + 13] = tris[i].rgba1[3] / 255f;

            vertices[dataLength * i + 17] = tris[i].rgba2[0] / 255f;
            vertices[dataLength * i + 18] = tris[i].rgba2[1] / 255f;
            vertices[dataLength * i + 19] = tris[i].rgba2[2] / 255f;
            vertices[dataLength * i + 20] = tris[i].rgba2[3] / 255f;
        }

        (uint vao, uint vbo) = Shaders.Prepare(vertices!, new int[] { RDrawData.posLength, RDrawData.colLength });
        Renderer.GL.DrawArrays(GLEnum.Triangles, 0, (uint)vertices!.Length);
        Shaders.Post(vao, vbo);
    }
}

public static class RDrawData
{
    internal const int posLength = 3;
    internal const int colLength = 4;

}