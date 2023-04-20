using System.Runtime.InteropServices;

namespace Magician.Renderer;

internal abstract class RDrawable
{
    public int Layer { get; set; }
    public byte[] rgba = new byte[4];
    public abstract void Draw();
    public static List<RDrawable> drawables = new List<RDrawable>();
    protected static Silk.NET.OpenGL.GL gl;

    protected float[]? vertices;
    protected static int posLength = 3;
    protected static int colLength = 4;

    static RDrawable()
    {
        if (Renderer.RGlobals.gl is null)
            throw Scribe.Error("Must create a gl context before creating an RDrawable");
        gl = Renderer.RGlobals.gl;
    }

    public static void DrawAll()
    {
        foreach (RDrawable rd in drawables)
        {
            rd.Draw();
        }
    }

    public static unsafe void GenShaders()
    {
        // GLSL
        string vertexShaderSrc = @"
            #version 330 core

            layout (location = 0) in vec3 pos;
            layout (location = 1) in vec4 rgba;
            out vec3 pos2;
            out vec4 rgba2;

            void main()
            {
                gl_Position = vec4(pos, 1.0);
                pos2 = pos;
                rgba2 = rgba;
            }
        ";

        string fragmentShaderSrc = $@"
            #version 330 core

            in vec3 pos2;
            in vec4 rgba2;

            out vec4 out_col;

            void main()
            {{
                out_col = vec4(rgba2.x, rgba2.y, rgba2.z, rgba2.w);
            }}
        ";

        uint vertexShader = gl.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSrc);
        gl.CompileShader(vertexShader);

        // TODO: make sure vertex shader compiles correctly
        //

        uint fragmentShader = gl.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSrc);
        gl.CompileShader(fragmentShader);

        // TODO: make sure fragment shader compiles correctly
        //

        uint prog = gl.CreateProgram();
        gl.AttachShader(prog, vertexShader);
        gl.AttachShader(prog, fragmentShader);
        gl.LinkProgram(prog);
        gl.UseProgram(prog);
        // TODO: make sure progam compiles correctly
        //

        // Clean shaders
        gl.DetachShader(prog, vertexShader);
        gl.DetachShader(prog, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    // Common code called before gl.DrawArrays
    protected internal static unsafe uint PrepareDraw(float[] vs, int[] dataLens)
    {
        Scribe.WarnIf(dataLens.Length < 2, "incomplete data in PrepareDraw");
        Scribe.WarnIf(dataLens.Length > 2, "unsupported data in PrepareDraw");  // TODO: support more shader data!
        uint stride = (uint)dataLens.Sum();
        int posLength = dataLens[0];
        int colLength = dataLens[1];
        
        // Create vertex array object
        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        uint vbo = gl.GenBuffer();
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, vbo);

        // Upload to the VAO
        fixed (float* buf = vs)
        {
            gl.BufferData(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, (nuint)(vs.Length * sizeof(float)), buf, Silk.NET.OpenGL.BufferUsageARB.StaticDraw);
        }

        // Specify how to read vertex data
        gl.VertexAttribPointer(0, posLength, Silk.NET.OpenGL.GLEnum.Float, false, (uint)(posLength +colLength) * sizeof(float), (void*)0);
        gl.VertexAttribPointer(1, colLength, Silk.NET.OpenGL.GLEnum.Float, true, (uint)(posLength + colLength) * sizeof(float), (void*)(posLength * sizeof(float)));
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(0);
        //gl.BindFragDataLocation()
        return vao;
    }

    protected internal static void PostDraw(uint vao)
    {
        // End stuff
        gl.DeleteVertexArray(vao);
        gl.BindVertexArray(0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ElementArrayBuffer, 0);
    }
}


internal class RPoint : RDrawable
{
    public float[] pos = new float[3];
    public RPoint(double x, double y, double z, double r, double g, double b, double a)
    {
        pos[0] = (float)x; pos[1] = (float)y; pos[2] = (float)z;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }
    public override void Draw()
    {
        Scribe.Warn("single-point drawing not supported");
    }
}

// Handles drawing of points
internal class RPoints : RDrawable
{
    protected static int dataLength = posLength + colLength;
    public RPoints(RPoint[] pts)
    {
        int numPts = pts.Length;
        vertices = new float[numPts * dataLength];

        for (int i = 0; i < numPts; i++)
        {
            RPoint currentPoint = pts[i];
            Scribe.Info(currentPoint);
            // TODO: remove the magic 800s
            vertices[dataLength * i + 0] = currentPoint.pos[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 1] = currentPoint.pos[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 2] = currentPoint.pos[2] / 800;
            // Color
            vertices[dataLength * i + 3] = pts[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = pts[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = pts[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = pts[i].rgba[3] / 255f;
        }
    }

    public override void Draw()
    {
        uint vao = PrepareDraw(vertices!, new int[]{posLength, colLength});
        gl.DrawArrays(Silk.NET.OpenGL.GLEnum.Points, 0, (uint)vertices!.Length);
        PostDraw(vao);
    }
}

internal class RLine : RDrawable
{
    public float[] p0 = new float[3];
    public float[] p1 = new float[3];
    public RLine(
    double x0, double y0, double z0,
    double x1, double y1, double z1,
    double r, double g, double b, double a)
    {
        p0[0] = (float)x0; p0[1] = (float)y0; p0[2] = (float)z0;
        p1[0] = (float)x1; p1[1] = (float)y1; p1[2] = (float)z1;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }
    public override void Draw()
    {
        Scribe.Warn("single-line drawing not supported");
    }
}

internal class RLines : RDrawable
{
    protected static int dataLength = 2 * (posLength + colLength);
    public RLines(RLine[] lines)
    {
        int numLines = lines.Length;
        vertices = new float[numLines * dataLength];
        for (int i = 0; i < numLines; i++)
        {
            RLine currentLine = lines[i];

            // TODO: remove the magic 800s
            vertices[dataLength * i + 0] = currentLine.p0[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 1] = currentLine.p0[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 2] = currentLine.p0[2] / 800;
            vertices[dataLength * i + 7] = currentLine.p1[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 8] = currentLine.p1[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 9] = currentLine.p1[2] / 800;

            // Color
            vertices[dataLength * i +  3] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i +  4] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i +  5] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i +  6] = lines[i].rgba[3] / 255f;
            vertices[dataLength * i + 10] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i + 11] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i + 12] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i + 13] = lines[i].rgba[3] / 255f;

        }
    }

    public override unsafe void Draw()
    {
        uint vao = PrepareDraw(vertices!, new int[]{posLength, colLength});
        gl.DrawArrays(Silk.NET.OpenGL.GLEnum.Lines, 0, (uint)vertices!.Length);
        PostDraw(vao);
    }
}

internal class RTriangle : RDrawable
{
    public float[] p0 = new float[3];
    public float[] p1 = new float[3];
    public float[] p2 = new float[3];
    public RTriangle(double x0, double y0, double z0, double x1, double y1, double z1, double x2, double y2, double z2, double r, double g, double b, double a)
    {
        p0[0] = (float)x0; p0[1] = (float)y0; p0[2] = (float)z0;
        p1[0] = (float)x1; p1[1] = (float)y1; p1[2] = (float)z1;
        p2[0] = (float)x2; p2[1] = (float)y2; p2[2] = (float)z2;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }

    public override void Draw()
    {
        throw Scribe.Error("For now, drawing RTriangle is disabled");
    }
}

// Handles drawing of filled polygons
internal class RTriangles : RDrawable
{
    protected static int dataLength = 3 * (posLength + colLength);
    public RTriangles(params RTriangle[] tris)
    {
        int numTriangles = tris.Length;
        vertices = new float[numTriangles * dataLength];
        for (int i = 0; i < numTriangles; i++)
        {
            RTriangle currentTriangle = tris[i];

            // TODO: remove the magic 800s
            vertices[dataLength * i + 0] = currentTriangle.p0[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 1] = currentTriangle.p0[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 2] = currentTriangle.p0[2] / 800;

            vertices[dataLength * i + 7] = currentTriangle.p1[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 8] = currentTriangle.p1[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 9] = currentTriangle.p1[2] / 800;

            vertices[dataLength * i + 14] = currentTriangle.p2[0] / Data.Globals.winWidth;
            vertices[dataLength * i + 15] = currentTriangle.p2[1] / Data.Globals.winHeight;
            vertices[dataLength * i + 16] = currentTriangle.p2[2] / 800;

            // Color
            vertices[dataLength * i + 3] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = tris[i].rgba[3] / 255f;

            vertices[dataLength * i + 10] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 11] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 12] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 13] = tris[i].rgba[3] / 255f;

            vertices[dataLength * i + 17] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 18] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 19] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 20] = tris[i].rgba[3] / 255f;
        }
    }

    public override unsafe void Draw()
    {
        // Vertices isn't null here because it was initialized in the constructor
        uint vao = PrepareDraw(vertices!, new int[]{posLength, colLength});
        gl.DrawArrays(Silk.NET.OpenGL.GLEnum.Triangles, 0, (uint)vertices!.Length);
        PostDraw(vao);
    }
}