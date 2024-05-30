namespace Magician.Paint;

using System.IO;

public class Shader
{
    string vertexPath;
    string fragmentPath;
    public string VertexSrc;
    public string FragmentSrc;
    public Shader(string name, string root, bool auto=false)
    {
        vertexPath = $"{root}/shaders/{name}.v.glsl";
        fragmentPath = $"{root}/shaders/{name}.f.glsl";
        bool vExists = File.Exists(vertexPath);
        bool fExists = File.Exists(fragmentPath);
        if (!vExists || !fExists)
        {
            Scribe.Error($"Shader {name} is missing source file{(!vExists && !fExists ? "s" : "")} {(!vExists ? vertexPath : "")}{(!fExists ? $"{(vExists || fExists ? "" : ", ")}{fragmentPath}" : "")}");
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
    static Dictionary<Shader, (uint vertex, uint fragment, uint prog)> shaders = new();
    public static Shader Default;
    public static Shader Inverse;
    //static uint prog = Renderer.GL.CreateProgram();
    static Shaders()
    {
        Default = new Shader("default", "../magician/paint", true);
        Inverse = new Shader("inverse", "../magician/paint", true);

        // Set the default shader
        Swap(Default);
    }
    public static void Swap(Shader s)
    {
        Renderer.GL.UseProgram(shaders[s].prog);
    }
    public static unsafe void Generate(Shader s)
    {
        string vertexShaderSrc = s.VertexSrc;
        string fragmentShaderSrc = s.FragmentSrc;

        if (shaders.ContainsKey(s))
        {
            throw Scribe.Error($"Shader {s} already exists");
        }

        shaders.Add(s, (0, 0, 0));
        shaders[s] = (Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader), shaders[s].fragment, shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].vertex, vertexShaderSrc);
        Renderer.GL.CompileShader(shaders[s].vertex);

        // TODO: make sure vertex shader compiles correctly
        //

        shaders[s] = (shaders[s].vertex, Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader), shaders[s].prog);
        Renderer.GL.ShaderSource(shaders[s].fragment, fragmentShaderSrc);
        Renderer.GL.CompileShader(shaders[s].fragment);

        // TODO: make sure fragment shader compiles correctly
        //

        shaders[s] = (shaders[s].vertex, shaders[s].fragment, Renderer.GL.CreateProgram());
        Renderer.GL.AttachShader(shaders[s].prog, shaders[s].vertex);
        Renderer.GL.AttachShader(shaders[s].prog, shaders[s].fragment);
        Renderer.GL.LinkProgram(shaders[s].prog);
        // TODO: make sure progam compiles correctly
        //
    }

    // Common code called before gl.DrawArrays
    internal static unsafe (uint, uint) Prepare(float[] vs, int[] dataLens)
    {
        Scribe.WarnIf(dataLens.Length < 2, "incomplete data in PrepareDraw");
        Scribe.WarnIf(dataLens.Length > 2, "unsupported data in PrepareDraw");  // TODO: support more shader data!
        uint stride = (uint)dataLens.Sum();
        int posLength = dataLens[0];
        int colLength = dataLens[1];

        // Create vertex array object
        uint vao = Renderer.GL.GenVertexArray();
        Renderer.GL.BindVertexArray(vao);

        uint vbo = Renderer.GL.GenBuffer();
        Renderer.GL.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, vbo);

        // Upload to the VAO
        fixed (float* buf = vs)
        {
            Renderer.GL.BufferData(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, (nuint)(vs.Length * sizeof(float)), buf, Silk.NET.OpenGL.BufferUsageARB.StaticDraw);
        }

        // Specify how to read vertex data
        Renderer.GL.VertexAttribPointer(0, posLength, Silk.NET.OpenGL.GLEnum.Float, false, (uint)(posLength + colLength) * sizeof(float), (void*)0);
        Renderer.GL.VertexAttribPointer(1, colLength, Silk.NET.OpenGL.GLEnum.Float, true, (uint)(posLength + colLength) * sizeof(float), (void*)(posLength * sizeof(float)));
        Renderer.GL.EnableVertexAttribArray(1);
        Renderer.GL.EnableVertexAttribArray(0);
        //gl.BindFragDataLocation()
        return (vao, vbo);
    }

    internal static void Post(uint vao, uint vbo)
    {
        // End stuff
        Renderer.GL.DeleteVertexArray(vao);
        Renderer.GL.BindVertexArray(0);
        Renderer.GL.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        Renderer.GL.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ElementArrayBuffer, 0);
        Renderer.GL.DeleteBuffer(vbo);
    }
}