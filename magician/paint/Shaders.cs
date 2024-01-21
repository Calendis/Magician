namespace Magician.Paint;

public static class Shaders
{
    public static unsafe void Generate()
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

        uint vertexShader = Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
        Renderer.GL.ShaderSource(vertexShader, vertexShaderSrc);
        Renderer.GL.CompileShader(vertexShader);

        // TODO: make sure vertex shader compiles correctly
        //

        uint fragmentShader = Renderer.GL.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader);
        Renderer.GL.ShaderSource(fragmentShader, fragmentShaderSrc);
        Renderer.GL.CompileShader(fragmentShader);

        // TODO: make sure fragment shader compiles correctly
        //

        uint prog = Renderer.GL.CreateProgram();
        Renderer.GL.AttachShader(prog, vertexShader);
        Renderer.GL.AttachShader(prog, fragmentShader);
        Renderer.GL.LinkProgram(prog);
        Renderer.GL.UseProgram(prog);
        // TODO: make sure progam compiles correctly
        //

        // Clean shaders
        Renderer.GL.DetachShader(prog, vertexShader);
        Renderer.GL.DetachShader(prog, fragmentShader);
        Renderer.GL.DeleteShader(vertexShader);
        Renderer.GL.DeleteShader(fragmentShader);
    }

    // Common code called before gl.DrawArrays
    internal static unsafe uint Prepare(float[] vs, int[] dataLens)
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
        return vao;
    }

    internal static void Post(uint vao)
    {
        // End stuff
        Renderer.GL.DeleteVertexArray(vao);
        Renderer.GL.BindVertexArray(0);
        Renderer.GL.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        Renderer.GL.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ElementArrayBuffer, 0);
    }
}