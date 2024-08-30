namespace Magician.Paint;
using Geo;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Collections;
using Magician.Core.Caster;

public static class Renderer
{
    static SdlContext? sdlContext;
    static GL? gl;
    static bool saveFrame = false;
    public static GL GL { get => gl ?? throw Scribe.Error("Null GL renderer"); set => gl = value; }
    public static SdlContext SDL { get => sdlContext ?? throw Scribe.Error("Null SDL context"); set => sdlContext = value; }
    public static bool Render { get; set; }
    public static bool Display { get; set; }
    internal static int pointBufferSize = 0;
    internal static int lineBufferSize = 0;
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

    public static class Drawables
    {
        readonly internal static Dictionary<Node, List<(byte[], float[])>> points = new();
        readonly internal static Dictionary<Node, List<(byte[], float[], float[])>> lines = new();
        readonly internal static Dictionary<Node, List<(byte[], byte[], byte[], float[], float[], float[])>> tris = new();
        public static (List<Node> ps, List<Node> ls, List<Node> ts) Todo = (new(), new(), new());
        internal static void Add(Node n, params (byte[], float[])[] ps) { if (points.ContainsKey(n)) { points[n].AddRange(ps); } else { points.Add(n, ps.ToList()); } }
        internal static void Add(Node n, params (byte[], float[], float[])[] ls) { if (lines.ContainsKey(n)) { lines[n].AddRange(ls); } else { lines.Add(n, ls.ToList()); } }
        internal static void Add(Node n, params (byte[], byte[], byte[], float[], float[], float[])[] ts) { if (tris.ContainsKey(n)) { tris[n].AddRange(ts); } else { tris.Add(n, ts.ToList()); } }

        public static void Clear()
        {
            points.Clear();
            lines.Clear();
            tris.Clear();
        }
        public static void DrawAll()
        {
            PrepareMatrices();
            int total = 0;

            foreach (Node n in Todo.ts) { Draw.Triangles(n); }
            foreach (Node n in tris.Keys) { total += tris[n].Count * 21; }
            GL.BindVertexArray(Shaders.vao.tris);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, Shaders.vbo.tris);
            GL.DrawArrays(GLEnum.Triangles, 0, (uint)total);
            total = 0;

            foreach (Node n in Todo.ls) { Draw.Lines(n); }
            foreach (Node n in lines.Keys) { total += lines[n].Count * 14; }
            GL.BindVertexArray(Shaders.vao.lines);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, Shaders.vbo.lines);
            GL.DrawArrays(GLEnum.Lines, 0, (uint)total);
            total = 0;

            foreach (Node n in Todo.ps) { Draw.Points(n); }
            foreach (Node n in points.Keys) { total += points[n].Count * 7; }
            GL.BindVertexArray(Shaders.vao.points);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, Shaders.vbo.points);
            GL.DrawArrays(GLEnum.Points, 0, (uint)total);
            total = 0;
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
    internal static List<Node> traverseKeys = new();
    internal static List<List<Node>> traverseValues = new();
    //internal static Dictionary<Node, List<Node>> traverse = new();
    internal static Dictionary<Node, (int ps, int ls, int ts)> nodeToSize = new();
    //internal static Dictionary<Node, (int ps, int ls, int ts)> parentToWriteHead = new();
    //internal static Dictionary<Node, (int ps, int ls, int ts)> nodeToStart = new();


    public static void StaleAll(Node? n = null)
    {
        if (n == null)
            n = Ref.Origin;
        foreach (Node c in n)
        {
            StaleAll(c);
        }
        n.stale = true;
    }

    public static void CacheRender()
    {
        (int ps, int ls, int ts) writeHead = nodeToSize[Ref.Origin];
        int i = 0;
        foreach (Node k in traverseKeys)
        {
            List<Node> breadth = traverseValues[i++];

            foreach (Node n in breadth)
            {
                //Scribe.Info($"Traverse {n.Title()}");
                if (n.Parent != k)
                {
                    throw Scribe.Issue($"Key error in CacheRender: {n} != {k}. The render tree is not being traversed correctly");
                }
                int ps, ls, ts;
                if (nodeToSize.ContainsKey(n))
                {
                    ps = nodeToSize[n].ps;
                    ls = nodeToSize[n].ls;
                    ts = nodeToSize[n].ts;
                }
                // TODO: why does this happen
                else
                {
                    //Scribe.Warn($"{n} not found");
                    ps = 0; ls = 0; ts = 0;
                }

                if (n.resized)
                {
                    Scribe.Info($"resizing {n.Title()}");
                    Console.WriteLine($"\tbefore: {n.range}");
                    n.range = (
                        (writeHead.ps, ps + writeHead.ps),
                        (writeHead.ls, ls + writeHead.ls),
                        (writeHead.ts, ts + writeHead.ts)
                    );
                    Console.WriteLine($"\tafter: {n.range}");
                    n.resized = false;
                }
                writeHead = (writeHead.ps + ps, writeHead.ls + ls, writeHead.ts + ts);
                //writeHead = (writeHead.ps + n.range.ps.end, writeHead.ls + n.range.ls.end, writeHead.ts + n.range.ts.end);
            }
        }
        //nodeToSize.Clear();
        traverseKeys.Clear();
        traverseValues.Clear();
    }
    // TODO: write a description for this very important function
    public static void Polygon(List<double[]> vertices, List<Color> cols, Node n, bool markTodo, bool isFace = false)
    {
        //Scribe.Info($"In Polygon, we see Node {n}");
        //cols = Enumerable.Range(0, vertices.Count).Select(n => Runes.Col.UIDefault.FG).ToList();
        //Scribe.Info($"rendering {drawMode} with {vertices.Count} vertices:");

        int numPoints = 0;
        if ((n.DrawFlags & DrawMode.POINTS) > 0)
        {
            numPoints = vertices.Count;
            if (numPoints > 0 && markTodo)
            {
                Renderer.Drawables.Todo.ls.Add(n);
            }
            (byte[], float[])[] rPointArray = new (byte[], float[])[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = ([(byte)cols[i].R, (byte)cols[i].G, (byte)cols[i].B, (byte)cols[i].A], [(float)vertices[i][0], (float)vertices[i][1], (float)vertices[i][2]]);
            }
            Renderer.Drawables.Add(n, rPointArray);
            //pointsGenerated += numPoints;
        }

        // Render lines and add geometry to array
        int numLines = 0;
        if ((n.DrawFlags & DrawMode.PLOT) > 0)
        {
            bool connected = (n.DrawFlags & DrawMode.CONNECTINGLINE) > 0 && vertices.Count >= 3;
            numLines = vertices.Count - (connected ? 0 : 1);
            if (numLines < 0)
                return;
            if (numLines > 0 && markTodo)
            {
                Renderer.Drawables.Todo.ls.Add(n);
            }

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
            Renderer.Drawables.Add(n, rLineArray);
            //linesGenerated += numLines;
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        int numTris = 0;
        if (((n.DrawFlags & DrawMode.INNER) > 0) && vertices.Count >= 3)
        {
            List<int> ect = EarCut.Triangulate(vertices);
            if (ect.Count % 3 != 0) { throw Scribe.Issue("Triangulator returned non-triplet vertices"); }
            List<int[]> triVertexIndices = new();
            for (int i = 0; i < ect.Count; i += 3)
            {
                triVertexIndices.Add([ect[i], ect[i + 1], ect[i + 2]]);
            }

            numTris = triVertexIndices.Count;
            if (numTris > 0 && markTodo)
            {
                Renderer.Drawables.Todo.ts.Add(n);
            }
            (byte[], byte[], byte[], float[], float[], float[])[] rTriArray = new (byte[], byte[], byte[], float[], float[], float[])[numTris];

            for (int i = 0; i < numTris; i++)
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

            Renderer.Drawables.Add(n, rTriArray);
            //trianglesGenerated += numTris;
        }
        if (nodeToSize.ContainsKey(n))
        {
            if (isFace)
            {
                nodeToSize[n] = (nodeToSize[n].ps + numPoints, nodeToSize[n].ls + numLines, nodeToSize[n].ts + numTris);
            }
            else
            {
                nodeToSize[n] = (numPoints, numLines, numTris);
            }
        }
        else
        {
            //Scribe.Info($"Adding {n.Title()}...");
            nodeToSize.Add(n, (numPoints, numLines, numTris));
        }
        //lastSeen = n;
    }
}

public static class Draw
{
    public static void Points(Node n)
    {
        List<(byte[] rgba, float[] pos)> points = Renderer.Drawables.points[n];
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
        if (Renderer.pointBufferSize == 0)
        {
            int c = 0;
            List<(byte[] col, float[] pos)> accumulatedVerts = new();
            foreach (Node k in Renderer.Drawables.points.Keys)
            {
                int numPoints = k.range.ps.end - k.range.ps.start;
                int np2 = Renderer.Drawables.points[k].Count;
                if (numPoints != np2)
                    throw Scribe.Issue($"Cache data mismatch");
                c += numPoints;
                foreach ((byte[] col, float[] pos) point in Renderer.Drawables.points[k])
                {
                    accumulatedVerts.Add(point);
                }
                Scribe.Info($"points {k.Title()} from {k.range.ps.start} to {k.range.ps.end}, total size {numPoints}");

            }
            float[] allVertices = new float[c * 7];
            int i = 0;
            foreach ((byte[] col, float[] pos) point in accumulatedVerts)
            {
                allVertices[7 * i + 0] = point.pos[0];
                allVertices[7 * i + 1] = point.pos[1];
                allVertices[7 * i + 2] = point.pos[2];

                allVertices[7 * i + 3] = point.col[0] / 255f;
                allVertices[7 * i + 4] = point.col[1] / 255f;
                allVertices[7 * i + 5] = point.col[2] / 255f;
                allVertices[7 * i + 6] = point.col[3] / 255f;
                i++;
            }
            Shaders.InitPBuf(allVertices);
            Renderer.pointBufferSize = allVertices.Length * sizeof(float);
        }
        Shaders.PreparePoints(vertices!, n);
        //Renderer.GL.BindVertexArray(Shaders.vao.points);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, Shaders.vbo.points);
        //Renderer.GL.DrawArrays(GLEnum.Points, 0, (uint)vertices!.Length);
        Shaders.Post(Shaders.vao.points, Shaders.vbo.points);
    }
    public static void Lines(Node n)
    {
        List<(byte[] rgba, float[] p0, float[] p1)> lines = Renderer.Drawables.lines[n];
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

            // TODO: support the other line colour
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
        if (Renderer.lineBufferSize == 0)
        {
            int c = 0;
            List<(byte[], float[], float[])> accumulatedLines = new();
            foreach (Node k in Renderer.Drawables.lines.Keys)
            {
                int numLines2 = k.range.ls.end - k.range.ls.start;
                if (numLines2 != Renderer.Drawables.lines[k].Count)
                    throw Scribe.Issue("Line cache data mismatch");
                c += Renderer.Drawables.lines[k].Count;
                foreach ((byte[], float[], float[]) line in Renderer.Drawables.lines[k])
                {
                    accumulatedLines.Add(line);
                }
            }
            float[] allVertices = new float[c * 14];
            int i = 0;
            foreach ((byte[] col, float[] p0, float[] p1) line in accumulatedLines)
            {
                allVertices[14 * i + 0] = line.p0[0];
                allVertices[14 * i + 1] = line.p0[1];
                allVertices[14 * i + 2] = line.p0[2];
                allVertices[14 * i + 7] = line.p1[0];
                allVertices[14 * i + 8] = line.p1[1];
                allVertices[14 * i + 9] = line.p1[2];

                allVertices[14 * i + 3] = line.col[0];
                allVertices[14 * i + 4] = line.col[1];
                allVertices[14 * i + 5] = line.col[2];
                allVertices[14 * i + 6] = line.col[3];
                allVertices[14 * i + 10] = line.col[0];
                allVertices[14 * i + 11] = line.col[1];
                allVertices[14 * i + 12] = line.col[2];
                allVertices[14 * i + 13] = line.col[3];
                i++;
            }
            Shaders.InitLBuf(allVertices);
            Renderer.lineBufferSize = allVertices.Length * sizeof(float);
        }
        var lineBuf = Shaders.PrepareLines(vertices!, n);
        //Renderer.GL.BindVertexArray(lineBuf.vao);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, lineBuf.vbo);
        //Renderer.GL.DrawArrays(GLEnum.Lines, 0, (uint)vertices!.Length);
        Shaders.Post(lineBuf.vao, lineBuf.vbo);
    }
    //public static void Triangles(List<(byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2)> tris)
    public static void Triangles(Node n)
    {
        List<(byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2)> tris = Renderer.Drawables.tris[n];
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
        if (Renderer.triBufferSize == 0)
        {
            int c = 0;
            List<(byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2)> accumulatedTris = new();
            foreach (Node k in Renderer.Drawables.tris.Keys)
            {
                c += Renderer.Drawables.tris[k].Count;
                foreach ((byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2) tri in Renderer.Drawables.tris[k])
                {
                    accumulatedTris.Add(tri);
                }
            }
            float[] allTris = new float[c * 21];
            int i = 0;
            foreach ((byte[] rgba0, byte[] rgba1, byte[] rgba2, float[] p0, float[] p1, float[] p2) tri in accumulatedTris)
            {
                allTris[21 * i + 0] = tri.p0[0];
                allTris[21 * i + 1] = tri.p0[1];
                allTris[21 * i + 2] = tri.p0[2];
                allTris[21 * i + 7] = tri.p1[0];
                allTris[21 * i + 8] = tri.p1[1];
                allTris[21 * i + 9] = tri.p1[2];
                allTris[21 * i + 14] = tri.p2[0];
                allTris[21 * i + 15] = tri.p2[1];
                allTris[21 * i + 16] = tri.p2[2];

                allTris[21 * i + 3] = tri.rgba0[0] / 255f;
                allTris[21 * i + 4] = tri.rgba0[1] / 255f;
                allTris[21 * i + 5] = tri.rgba0[2] / 255f;
                allTris[21 * i + 6] = tri.rgba0[3] / 255f;
                allTris[21 * i + 10] = tri.rgba1[0] / 255f;
                allTris[21 * i + 11] = tri.rgba1[1] / 255f;
                allTris[21 * i + 12] = tri.rgba1[2] / 255f;
                allTris[21 * i + 13] = tri.rgba1[3] / 255f;
                allTris[21 * i + 17] = tri.rgba2[0] / 255f;
                allTris[21 * i + 18] = tri.rgba2[1] / 255f;
                allTris[21 * i + 19] = tri.rgba2[2] / 255f;
                allTris[21 * i + 20] = tri.rgba2[3] / 255f;
                i++;
            }
            Shaders.InitTBuf(allTris);
            Renderer.triBufferSize = allTris.Length * sizeof(float);
        }
        var triBuf = Shaders.PrepareTris(vertices!, n);
        //Renderer.GL.BindVertexArray(triBuf.vao);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, triBuf.vbo);
        //Renderer.GL.DrawArrays(GLEnum.Triangles, 0, (uint)vertices!.Length);
        Shaders.Post(triBuf.vao, triBuf.vbo);
    }
}

internal static class RDrawData
{
    internal const int posLength = 3;
    internal const int colLength = 4;

}