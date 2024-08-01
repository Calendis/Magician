namespace Magician.Paint;

using System.IO;
using System.Runtime.CompilerServices;
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
    internal static List<Node> pointsTodo = new();
    internal static List<Node> linesTodo = new();
    internal static List<Node> trisTodo = new();
    static (uint points, uint lines, uint tris) vao;
    static (uint points, uint lines, uint tris) vbo;

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

    // Common code called before gl.DrawArrays
    internal static unsafe (uint vao, uint vbo) PreparePoints(float[] vertices)
    {
        fixed (float* data = vertices)
        {
            int c = 0;
            if (Renderer.pointBufferSize > 0)
            {
                foreach (Node n in pointsTodo)
                {
                    int numPoints = n.range.ps.end - n.range.ps.start;
                    Scribe.Info($"buffer points: {numPoints} from {n.Title()} at {RuntimeHelpers.GetHashCode(n)}");
                    if (numPoints > 0)
                    {
                        Renderer.GL.BindVertexArray(vao.points);
                        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.points);
                        Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, n.range.ps.start * 7, (nuint)(numPoints * 7), data);
                    }
                }
            }
            else
            {
                foreach (Node n in pointsTodo)
                {
                    int numPoints = n.range.ps.end - n.range.ps.start;
                    c += numPoints;
                    Scribe.Info($"{n.Title()} from {n.range.ps.start} to {n.range.ps.end}, total size {numPoints}");
                }
                nuint allocSize = (nuint)c*sizeof(float)*7;
                Renderer.GL.BindVertexArray(vao.points);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.points);
                Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
                Renderer.pointBufferSize = (int)allocSize;
            }
        }
        return (vao.points, vbo.points);
    }
    internal static unsafe (uint vao, uint vbo) PrepareLines(float[] vertices)
    {
        fixed (float* data = vertices)
        {
            int c = 0;
            if (Renderer.lineBufferSize > 0)
            {
                foreach (Node n in linesTodo)
                {
                    int numLines = n.range.ls.end - n.range.ls.start;
                    Scribe.Info($"buffer lines: {numLines} from {n.Title()} at {RuntimeHelpers.GetHashCode(n)}");
                    if (numLines > 0)
                    {
                        Renderer.GL.BindVertexArray(vao.lines);
                        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.lines);
                        Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, n.range.ls.start * 14, (nuint)(numLines * 14), data);
                    }
                }
            }
            else
            {
                foreach (Node n in linesTodo)
                {
                    int numLines = n.range.ls.end - n.range.ls.start;
                    c += numLines;
                    Scribe.Info($"{n.Title()} from {n.range.ls.start} to {n.range.ls.end}, total size {numLines}");
                }
                nuint allocSize = (nuint)c*sizeof(float)*14;
                Renderer.GL.BindVertexArray(vao.lines);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.lines);
                Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
                Renderer.lineBufferSize = (int)allocSize;
            }
        }
        return (vao.lines, vbo.lines);
    }
    internal static unsafe (uint vao, uint vbo) PrepareTris(float[] vertices)
    {
        fixed (float* data = vertices)
        {
            int c = 0;
            if (Renderer.triBufferSize > 0)
            {
                //Scribe.Info($"Got buffer size {Renderer.triBufferSize}");
                foreach (Node n in trisTodo)
                {
                    int numTris = n.range.ts.end - n.range.ts.start;
                    c += numTris;
                    Scribe.Warn($"{c*21*sizeof(float)}/{Renderer.triBufferSize}");
                    if (c*21*sizeof(float) > Renderer.triBufferSize-21*sizeof(float))
                        throw Scribe.Issue("obo test");
                    Scribe.Info($"buffer tris: {numTris} from {n.Title()} at {RuntimeHelpers.GetHashCode(n)}");
                    if (numTris > 0)
                    {
                        Renderer.GL.BindVertexArray(vao.tris);
                        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.tris);
                        Renderer.GL.BufferSubData(BufferTargetARB.ArrayBuffer, n.range.ts.start * 21, (nuint)(numTris * 21), data);
                    }
                }
            }
            else
            {
                foreach (Node n in trisTodo)
                {
                    int numTris = n.range.ts.end - n.range.ts.start;
                    c += numTris;
                    Scribe.Info($"{n.Title()} from {n.range.ts.start} to {n.range.ts.end}, total size {numTris}");
                }
                nuint allocSize = (nuint)c*sizeof(float)*21;
                Renderer.GL.BindVertexArray(vao.tris);
                Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.tris);
                Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, allocSize, data, BufferUsageARB.DynamicDraw);
                Renderer.triBufferSize = (int)allocSize;
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
