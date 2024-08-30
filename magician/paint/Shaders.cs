namespace Magician.Paint;

using System.IO;
using Magician.Geo;
using Silk.NET.OpenGL;

public class Shader
{
    string vertexPath;
    string fragmentPath;
    public string VertexSrc;
    public string FragmentSrc;
    public string Name;
    public Shader(string name, string root, bool auto = false)
    {
        Name = name;
        vertexPath = $"{root}/shaders/{name}.v.glsl";
        fragmentPath = $"{root}/shaders/{name}.f.glsl";
        bool vExists = File.Exists(vertexPath);
        bool fExists = File.Exists(fragmentPath);
        if (!vExists && !fExists)
        {
            throw Scribe.Error($"Could not create shader {name}. Must provide at least one of {vertexPath}, {fragmentPath}");
        }
        if (!vExists)
        {
            vertexPath = "magician/paint/shaders/default.v.glsl";
        }
        if (!fExists)
        {
            fragmentPath = "magician/paint/shaders/default.f.glsl";
        }

        VertexSrc = File.ReadAllText(vertexPath);
        FragmentSrc = File.ReadAllText(fragmentPath);
        if (!auto)
            return;
        Shaders.Generate(this);
    }
}

public static class Shaders
{
    // Key: shader object
    // Val: vertex shader, fragment shader, program that uses each
    internal static Dictionary<Shader, (uint vertex, uint fragment, uint prog)> shaders = new();

    // Default shaders
    public static Shader Current;
    public static Shader Default;
    public static Shader Inverse;

    // Cull shader is always applied
    static Shader Cull;
    static uint cullV;
    internal static (uint points, uint lines, uint tris) vao;
    internal static (uint points, uint lines, uint tris) vbo;

    static Shaders()
    {
        vao.points = Renderer.GL.GenVertexArray();
        vbo.points = Renderer.GL.GenBuffer();
        Renderer.GL.BindVertexArray(vao.points);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.points);
        unsafe { Renderer.GL.VertexAttribPointer(0, RDrawData.posLength, GLEnum.Float, false, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)0); }
        unsafe { Renderer.GL.VertexAttribPointer(1, RDrawData.colLength, GLEnum.Float, true, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)(3 * sizeof(float))); }
        Renderer.GL.EnableVertexAttribArray(0);
        Renderer.GL.EnableVertexAttribArray(1);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        vao.lines = Renderer.GL.GenVertexArray();
        vbo.lines = Renderer.GL.GenBuffer();
        Renderer.GL.BindVertexArray(vao.lines);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.lines);
        unsafe { Renderer.GL.VertexAttribPointer(0, RDrawData.posLength, GLEnum.Float, false, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)0); }
        unsafe { Renderer.GL.VertexAttribPointer(1, RDrawData.colLength, GLEnum.Float, true, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)(3 * sizeof(float))); }
        Renderer.GL.EnableVertexAttribArray(0);
        Renderer.GL.EnableVertexAttribArray(1);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        vao.tris = Renderer.GL.GenVertexArray();
        vbo.tris = Renderer.GL.GenBuffer();
        Renderer.GL.BindVertexArray(vao.tris);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.tris);
        unsafe { Renderer.GL.VertexAttribPointer(0, RDrawData.posLength, GLEnum.Float, false, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)0); }
        unsafe { Renderer.GL.VertexAttribPointer(1, RDrawData.colLength, GLEnum.Float, true, (uint)(RDrawData.posLength + RDrawData.colLength) * sizeof(float), (void*)(3 * sizeof(float))); }
        Renderer.GL.EnableVertexAttribArray(0);
        Renderer.GL.EnableVertexAttribArray(1);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        //Renderer.GL.BindFragDataLocation()

        Default = new Shader("default", "magician/paint", true);
        Inverse = new Shader("inverse", "magician/paint", true);
        Cull = new Shader("cull", "magician/paint");

        // Compile the cull shader immediately, as it is common
        //cullV = Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
        //Renderer.GL.ShaderSource(cullV, Cull.VertexSrc);
        //Renderer.GL.CompileShader(cullV);

        // Set the default shader
        Swap(Default);
        Current = Default;  // Stupid compiler likes this line
    }
    public static void Swap(Shader s)
    {
        Renderer.GL.UseProgram(shaders[s].prog);
        Current = s;
    }
    public static unsafe void Generate(Shader s)
    {
        if (shaders.ContainsKey(s))
            throw Scribe.Error($"Shader {s} already exists");

        string vertexShaderSrc = s.VertexSrc;
        string fragmentShaderSrc = s.FragmentSrc;
        // The tuple is placeholder data
        shaders.Add(s, (0, 0, 0));
        int status;

        // Generate vertex shader
        shaders[s] = (Renderer.GL.CreateShader(ShaderType.VertexShader), shaders[s].fragment, shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].vertex, vertexShaderSrc);
        Renderer.GL.CompileShader(shaders[s].vertex);
        Renderer.GL.GetShader(shaders[s].vertex, GLEnum.CompileStatus, out status);
        if (status != 1) { throw Scribe.Error($"Compilation error in shader {s.Name}: {Renderer.GL.GetShaderInfoLog(shaders[s].vertex)}\n{s.VertexSrc}"); }

        // Generate fragment shader
        shaders[s] = (shaders[s].vertex, Renderer.GL.CreateShader(ShaderType.FragmentShader), shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].fragment, fragmentShaderSrc);
        Renderer.GL.CompileShader(shaders[s].fragment);
        Renderer.GL.GetShader(shaders[s].fragment, GLEnum.CompileStatus, out status);
        if (status != 1) { throw Scribe.Error($"Compilation error in shader {s.Name}: {Renderer.GL.GetShaderInfoLog(shaders[s].fragment)}\n{s.FragmentSrc}"); }

        // Assemble shader program for use
        shaders[s] = (shaders[s].vertex, shaders[s].fragment, Renderer.GL.CreateProgram());
        Renderer.GL.AttachShader(shaders[s].prog, shaders[s].vertex);
        //Renderer.GL.AttachShader(shaders[s].prog, cullV);  // We always use the cull shader
        Renderer.GL.AttachShader(shaders[s].prog, shaders[s].fragment);
        Renderer.GL.LinkProgram(shaders[s].prog);
        Renderer.GL.GetProgram(shaders[s].prog, GLEnum.LinkStatus, out status);
        if (status != 1)
        {
            throw Scribe.Error($"Link error in shader program: {Renderer.GL.GetProgramInfoLog(shaders[s].prog)}");
        }
    }

    internal static unsafe void InitPBuf(float[] vertices)
    {
        fixed (float* data = vertices)
        {
            //int c = 0;
            //foreach (Node k in Renderer.Drawables.points.Keys)
            //{
            //    int numPoints = k.range.ps.end - k.range.ps.start;
            //    c += numPoints;
            //    Scribe.Info($"points {k.Title()} from {k.range.ps.start} to {k.range.ps.end}, total size {numPoints}");
            //}
            nuint allocSize = (nuint)vertices.Length * sizeof(float);
            Renderer.GL.BindVertexArray(vao.points);
            Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.points);
            Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
            Renderer.pointBufferSize = (int)allocSize;
        }
    }
    internal static unsafe void InitLBuf(float[] vertices)
    {
        //int c = 0;
        fixed (float* data = vertices)
        {
            //foreach (Node n in Renderer.Drawables.lines.Keys)
            //{
            //    int numLines = n.range.ls.end - n.range.ls.start;
            //    c += numLines;
            //    //Scribe.Info($"lines {n.Title()} from {n.range.ls.start} to {n.range.ls.end}, total size {numLines}");
            //}
            nuint allocSize = (nuint)vertices.Length * sizeof(float);
            Renderer.GL.BindVertexArray(vao.lines);
            Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.lines);
            Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
            //Renderer.lineBufferSize = (int)allocSize;
        }
    }

    internal static unsafe void InitTBuf(float[] vertices)
    {
        fixed (float* data = vertices)
        {
            //int c = 0;
            //foreach (Node n in Renderer.Drawables.tris.Keys)
            //{
            //    int numTris = n.range.ts.end - n.range.ts.start;
            //    c += numTris;
            //    Scribe.Info($"tris {n.Title()} from {n.range.ts.start} to {n.range.ts.end}, total size {numTris}");
            //}
            nuint allocSize = (nuint)vertices.Length * sizeof(float);
            Renderer.GL.BindVertexArray(vao.tris);
            Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.tris);
            Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
            //Renderer.triBufferSize = (int)allocSize;
        }
    }

    // Common code called before gl.DrawArrays
    internal static unsafe (uint vao, uint vbo) PreparePoints(float[] vertices, Node nd)
    {
        fixed (float* data = vertices)
        {
            int numPoints = nd.range.ps.end - nd.range.ps.start;
            if (vertices.Length != numPoints)
            {
                //throw Scribe.Issue($"Point data mismatch: expected {numPoints*7} got {vertices.Length} from {n.Title()}");
            }
            //Scribe.Info($"buffer points: {numPoints} from {n.Title()} at {RuntimeHelpers.GetHashCode(n)}");
            if (numPoints > 0)
            {
                Renderer.GL.BindVertexArray(vao.points);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.points);
                Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, nd.range.ps.start * 7 * sizeof(float), (nuint)(numPoints * 7 * sizeof(float)), data);
            }
        }
        return (vao.points, vbo.points);
    }
    internal static unsafe (uint vao, uint vbo) PrepareLines(float[] vertices, Node nd)
    {
        fixed (float* data = vertices)
        {
            int numLines = nd.range.ls.end - nd.range.ls.start;
            if (vertices.Length != numLines)
            {
                //throw Scribe.Issue($"Line data mismatch: expected {numLines*14} got {vertices.Length} from {n.Title()}");
            }
            if (numLines > 0)
            {
                //Scribe.Info($"buffer lines: {numLines} from {nd.Title()} at {RuntimeHelpers.GetHashCode(nd)}\n\t{nd.range.ls.start * 14 * sizeof(float)} to {nd.range.ls.end * 14 * sizeof(float)} / {Renderer.lineBufferSize}");
                Renderer.GL.BindVertexArray(vao.lines);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.lines);
                Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, nd.range.ls.start * 14 * sizeof(float), (nuint)(numLines * 14 * sizeof(float)), data);
            }
        }
        return (vao.lines, vbo.lines);
    }
    internal static unsafe (uint vao, uint vbo) PrepareTris(float[] vertices, Node nd)
    {
        fixed (float* data = vertices)
        {
            //Scribe.Info($"Got buffer size {Renderer.triBufferSize}");
            int numTris = nd.range.ts.end - nd.range.ts.start;
            //if (vertices.Length != numTris)
            //{
            //    //throw Scribe.Issue($"Triangle data mismatch: expected {numTris*21} got {vertices.Length} from {nd.Title()}");
            //}
            if (numTris > 0)
            {
                Renderer.GL.BindVertexArray(vao.tris);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.tris);
                Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, nd.range.ts.start * 21 * sizeof(float), (nuint)(numTris * 21 * sizeof(float)), data);
            }
        }
        return (vao.tris, vbo.tris);
    }

    internal static void Post(uint vao, uint vbo)
    {
        // End stuff
        //Renderer.GL.DeleteVertexArray(vao);
        //Renderer.GL.BindVertexArray(0);
        //Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        //Renderer.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        //Renderer.GL.DeleteBuffer(vbo);
    }
}
