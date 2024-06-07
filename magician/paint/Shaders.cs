namespace Magician.Paint;

using System.IO;
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
            Scribe.Error($"Could not create shader {name}. Must provide at least one of {vertexPath}, {fragmentPath}");
        }
        if (!vExists)
        {
            vertexPath = "../magician/paint/shaders/default.v.glsl";
        }
        if (!fExists)
        {
            fragmentPath = "../magician/paint/shaders/default.f.glsl";
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
    static uint vao = Renderer.GL.GenVertexArray();
    static uint vbo = Renderer.GL.GenBuffer();

    static Shaders()
    {
        Renderer.GL.BindVertexArray(vao);
        Renderer.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe{Renderer.GL.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)(3 + 4) * sizeof(float), (void*)0);}
        unsafe{Renderer.GL.VertexAttribPointer(1, 4, GLEnum.Float, true, (uint)(3 + 4) * sizeof(float), (void*)(3 * sizeof(float)));}
        Renderer.GL.EnableVertexAttribArray(1);
        Renderer.GL.EnableVertexAttribArray(0);

        Default = new Shader("default", "../magician/paint", true);
        Inverse = new Shader("inverse", "../magician/paint", true);
        Cull = new Shader("cull", "../magician/paint");

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
        shaders[s] = (Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader), shaders[s].fragment, shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].vertex, vertexShaderSrc);
        Renderer.GL.CompileShader(shaders[s].vertex);
        Renderer.GL.GetShader(shaders[s].vertex, GLEnum.CompileStatus, out status);
        if (status != 1)
        {
            throw Scribe.Error($"Compilation error in shader {s.Name}: {Renderer.GL.GetShaderInfoLog(shaders[s].vertex)}\n{s.VertexSrc}");
        }

        // Generate fragment shader
        shaders[s] = (shaders[s].vertex, Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader), shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].fragment, fragmentShaderSrc);
        Renderer.GL.CompileShader(shaders[s].fragment);
        Renderer.GL.GetShader(shaders[s].fragment, GLEnum.CompileStatus, out status);
        if (status != 1)
        {
            throw Scribe.Error($"Compilation error in shader {s.Name}: {Renderer.GL.GetShaderInfoLog(shaders[s].fragment)}\n{s.FragmentSrc}");
        }

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
    internal static unsafe (uint, uint) Prepare(float[] data, int[] dataLens)
    {
        Scribe.WarnIf(dataLens.Length < 2, "incomplete data in PrepareDraw");
        Scribe.WarnIf(dataLens.Length > 2, "unsupported data in PrepareDraw");  // TODO: support more shader data!
        int posLength = dataLens[0];
        int colLength = dataLens[1];
        uint stride = (uint)dataLens.Sum();

        // Create vertex array object
        //uint vao = Renderer.GL.GenVertexArray();

        //uint vbo = Renderer.GL.GenBuffer();

        // Upload to the VAO
        fixed (float* buf = data)
        {
            Renderer.GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }

        // Specify how to read vertex data
        //
        //gl.BindFragDataLocation()
        return (vao, vbo);
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