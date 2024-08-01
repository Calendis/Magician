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
    //// TODO: support second colour for lines
    readonly static List<(byte[], float[], float[])> lines = new();
    readonly static List<(byte[], byte[], byte[], float[], float[], float[])> tris = new();
    public static GL GL { get => gl ?? throw Scribe.Error("Null GL renderer"); set => gl = value; }
    public static SdlContext SDL { get => sdlContext ?? throw Scribe.Error("Null SDL context"); set => sdlContext = value; }
    public static bool Render { get; set; }
    public static bool Display { get; set; }
    //internal static bool pointsRenderedAtLeastOnce = false;
    internal static int pointBufferSize = 0;
    //internal static bool linesRenderedAtLeastOnce = false;
    internal static int lineBufferSize = 0;
    //internal static bool trisRenderedAtLeastOnce = false;
    internal static int triBufferSize = 0;

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
        //Scribe.Info($"DrawAll: ({points.Count}, {lines.Count}, {tris.Count})");
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
    internal class BoxedInt
    {
        int n;
        internal BoxedInt(int i)
        {
            n = i;
        }
        internal void Incr(int i)
        {
            n += i;
        }
        internal int Get()
        {
            return n;
        }
        public override string ToString()
        {
            return $"{n}";
        }
    }

    public static void PostRender()
    {
        pointsGenerated = 0;
        linesGenerated = 0;
        trianglesGenerated = 0;
        lastSeen = null;
        Shaders.pointsTodo.Clear();
        Shaders.linesTodo.Clear();
        Shaders.trisTodo.Clear();
    }

    static int pointsGenerated = 0;
    static int linesGenerated = 0;
    static int trianglesGenerated = 0;
    static Node? lastSeen = null;
    // In Polygon, we can now see the latest todo node in Shaders
    public static void Polygon(List<double[]> vertices, List<Color> cols, Node n, bool add = false)
    {
        n.stale = false;
        //Scribe.Info($"In Polygon, we see Node {n}");
        //cols = Enumerable.Range(0, vertices.Count).Select(n => Runes.Col.UIDefault.FG).ToList();
        //Scribe.Info($"rendering {drawMode} with {vertices.Count} vertices:");
        // Render points
        if ((n.DrawFlags & DrawMode.POINTS) > 0)
        {
            int numPoints = vertices.Count;
            (byte[], float[])[] rPointArray = new (byte[], float[])[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = ([(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A], [(float)vertices[i][0], (float)vertices[i][1], (float)vertices[i][2]]);
            }
            if (lastSeen != n)
            {
                n.range.ps = (pointsGenerated, numPoints + pointsGenerated);
            }
            else
            {
                n.range.ps = (n.range.ps.start, numPoints + pointsGenerated);
            }
            Renderer.Drawables.Add(rPointArray);
            pointsGenerated += numPoints;
            if (add)
                Shaders.pointsTodo.Add(n);
        }

        // Render lines and add geometry to array
        if ((n.DrawFlags & DrawMode.PLOT) > 0)
        {
            bool connected = (n.DrawFlags & DrawMode.CONNECTINGLINE) > 0 && vertices.Count >= 3;
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
                rLineArray[i] = ([(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A], [(float)x0, (float)y0, (float)z0], [(float)x1, (float)y1, (float)z1]);
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

                rLineArray[^1] = ([(byte)subr, (byte)subg, (byte)subb, (byte)suba], [(float)pLast[0], (float)pLast[1], (float)pLast[2]], [(float)pFirst[0], (float)pFirst[1], (float)pFirst[2]]);
            }
            // TODO: can that cause a render bug for non-connected? (the last slot is empty)
            if (lastSeen != n)
            {
                n.range.ls = (linesGenerated, numLines + linesGenerated);
            }
            else
            {
                n.range.ls = (n.range.ls.start, numLines + linesGenerated);
            }
            Renderer.Drawables.Add(rLineArray);
            linesGenerated += numLines;
            if (add)
                Shaders.linesTodo.Add(n);
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        if (((n.DrawFlags & DrawMode.INNER) > 0) && vertices.Count >= 3)
        {
            List<int> ect = EarCut.Triangulate(vertices);
            if (ect.Count % 3 != 0) { throw Scribe.Issue("Triangulator returned non-triplet vertices"); }
            List<int[]> triVertexIndices = new();
            for (int i = 0; i < ect.Count; i += 3)
            {
                triVertexIndices.Add([ect[i], ect[i + 1], ect[i + 2]]);
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
                    [(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A],
                    [(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A],
                    [(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A],
                    [(float)vertices[tri0][0], (float)vertices[tri0][1], (float)vertices[tri0][2]],
                    [(float)vertices[tri1][0], (float)vertices[tri1][1], (float)vertices[tri1][2]],
                    [(float)vertices[tri2][0], (float)vertices[tri2][1], (float)vertices[tri2][2]]
                );
            }
            if (lastSeen != n)
            {
                n.range.ts = (trianglesGenerated, numTriangles + trianglesGenerated);
            }
            else
            {
                n.range.ts = (n.range.ts.start, numTriangles + trianglesGenerated);
            }
            Renderer.Drawables.Add(rTriArray);
            trianglesGenerated += numTriangles;
            if (add)
                Shaders.trisTodo.Add(n);
        }
        lastSeen = n;
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

        // grouped in 7s
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

        //var buffers = Shaders.Prepare(vertices!, [RDrawData.posLength, RDrawData.colLength]);
        var pointBuf = Shaders.PreparePoints(vertices!);
        Renderer.GL.BindVertexArray(pointBuf.vao);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, pointBuf.vbo);
        Renderer.GL.DrawArrays(GLEnum.Points, 0, (uint)vertices!.Length);
        Shaders.Post(pointBuf.vao, pointBuf.vbo);
    }
    public static void Lines(List<(byte[] rgba, float[] p0, float[] p1)> lines)
    {
        int dataLength = 2 * (RDrawData.posLength + RDrawData.colLength);
        float[] vertices;

        int numLines = lines.Count;
        vertices = new float[numLines * dataLength];
        // group in 14s
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

        //var buffers = Shaders.Prepare(vertices!, [RDrawData.posLength * 2, RDrawData.colLength * 2]);
        var lineBuf = Shaders.PrepareLines(vertices!);
        Renderer.GL.BindVertexArray(lineBuf.vao);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, lineBuf.vbo);
        Renderer.GL.DrawArrays(GLEnum.Lines, 0, (uint)vertices!.Length);
        Shaders.Post(lineBuf.vao, lineBuf.vbo);
    }
    public static void Triangles(List<(byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2)> tris)
    {
        int dataLength = 3 * (RDrawData.posLength + RDrawData.colLength);
        float[] vertices;

        int numTriangles = tris.Count;
        vertices = new float[numTriangles * dataLength];
        // grouped in 21s
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

        //var buffers = Shaders.Prepare(vertices!, [RDrawData.posLength * 3, RDrawData.colLength * 3]);
        var triBuf = Shaders.PrepareTris(vertices!);
        Renderer.GL.BindVertexArray(triBuf.vao);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, triBuf.vbo);
        Renderer.GL.DrawArrays(GLEnum.Triangles, 0, (uint)vertices!.Length);
        Shaders.Post(triBuf.vao, triBuf.vbo);
    }
}

internal static class RDrawData
{
    internal const int posLength = 3;
    internal const int colLength = 4;

}