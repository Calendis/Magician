namespace Magician.Renderer;

internal abstract class RDrawable
{
    public int Layer { get; set; }
    public byte[] rgba = new byte[4];
    public abstract void Draw();
    public static List<RDrawable> drawables = new List<RDrawable>();

    public static void DrawAll()
    {
        foreach (RDrawable rd in drawables)
        {
            rd.Draw();
        }
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
        Control.SaveTarget();
        //SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);

        //SDL_SetRenderDrawColor(SDLGlobals.renderer, rgba[0], rgba[1], rgba[2], rgba[3]);
        // TODO: This corrupts the state
        //SDL_RenderDrawPointF(SDLGlobals.renderer, pos[0], pos[1]);

        Control.RecallTarget();
    }
}

internal class RLine : RDrawable
{
    public float[] p0 = new float[3];
    public float[] p1 = new float[3];
    public RLine(double x0, double y0, double z0, double x1, double y1, double z1, double r, double g, double b, double a)
    {
        p0[0] = (float)x0; p0[1] = (float)y0; p0[2] = (float)z0;
        p1[0] = (float)x1; p1[1] = (float)y1; p1[2] = (float)z1;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }

    public override void Draw()
    {
        Control.SaveTarget();
        //SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);

        /* SDL_SetRenderDrawColor(SDLGlobals.renderer, rgba[0], rgba[1], rgba[2], rgba[3]);
        SDL_RenderDrawLineF(SDLGlobals.renderer,
            p0[0], p0[1],
            p1[0], p1[1]); */

        //SDLGlobals.gl.

        Control.RecallTarget();
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
        Control.SaveTarget();
        //SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);

        Control.RecallTarget();
        throw Scribe.Error("For now, drawing RTriangles is disabled");
    }
}

internal class RGeometry : RDrawable
{
    //SDL_Vertex[] vs;
    float[] vs;
    public RGeometry(params RTriangle[] rts)
    {
        int numTriangles = rts.Length;
        //List<RTriangle> rtsl = rts.ToList().Sort((rt0, rt1) => rt0.)
        //vs = new SDL_Vertex[numTriangles * 3];
        vs = new float[numTriangles * 9];  // x y z x y z x y z ...
        for (int i = 0; i < numTriangles; i++)
        {
            RTriangle currentTriangle = rts[i];
            /* vs[3 * i] = new SDL_Vertex();
            vs[3 * i].position.x = currentTriangle.p0[0];
            vs[3 * i].position.y = currentTriangle.p0[1];

            vs[3 * i + 1] = new SDL_Vertex();
            vs[3 * i + 1].position.x = currentTriangle.p1[0];
            vs[3 * i + 1].position.y = currentTriangle.p1[1];

            vs[3 * i + 2] = new SDL_Vertex();
            vs[3 * i + 2].position.x = currentTriangle.p2[0];
            vs[3 * i + 2].position.y = currentTriangle.p2[1]; */
            vs[9*i]   = currentTriangle.p0[0] / Data.Globals.winWidth;
            vs[9*i+1] = currentTriangle.p0[1] / -Data.Globals.winHeight;
            vs[9*i+2] = currentTriangle.p0[2]/ -Data.Globals.winWidth;
            
            vs[9*i+3] = currentTriangle.p1[0] / Data.Globals.winWidth;
            vs[9*i+4] = currentTriangle.p1[1] / -Data.Globals.winHeight;
            vs[9*i+5] = currentTriangle.p1[2]/ -Data.Globals.winWidth;

            vs[9*i+6] = currentTriangle.p2[0] / Data.Globals.winWidth;
            vs[9*i+7] = currentTriangle.p2[1] / -Data.Globals.winHeight;
            vs[9*i+8] = currentTriangle.p2[2]/ -Data.Globals.winWidth;
            // Color
            //SDL_Color c0;
            //c0.r = rts[i].rgba[0]; c0.g = rts[i].rgba[1]; c0.b = rts[i].rgba[2]; c0.a = rts[i].rgba[3];

            // Randomly-coloured triangles for debugging
            /* Random rnd = new Random(i);
            byte rndRed = (byte)rnd.Next(256);
            byte rndGrn = (byte)rnd.Next(256);
            byte rndBlu = (byte)rnd.Next(256);
            c0.r = rndRed;
            c0.g = rndGrn;
            c0.b = rndBlu;
            c0.a = rts[i].rgba[3]; */

            //vs[3 * i].color = c0;
            //vs[3 * i + 1].color = c0;
            //vs[3 * i + 2].color = c0;
        }
    }

    public override unsafe void Draw()
    {
        Control.SaveTarget();
        //SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);

        //IntPtr ip = new();
        //SDL_RenderGeometry(SDLGlobals.renderer, ip, vs, vs.Length, null, 0);

        // Create vertex array object
        Silk.NET.OpenGL.GL gl = Renderer.SDLGlobals.gl;
        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // Example vertices
        /* float[] vertices =
        {
             0.5f,  -0.5f, 0.0f,
             0.5f, 0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f
        }; */

        uint vbo = gl.GenBuffer();
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, vbo);

        // Upload to the VAO
        fixed (float* buf = vs)
        {
            gl.BufferData(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, (nuint)(vs.Length * sizeof(float)), buf, Silk.NET.OpenGL.BufferUsageARB.StaticDraw);
        }

        // GLSL
        string vertexShaderSrc = @"
            #version 330 core 
            layout (location = 0) in vec3 pos;

            void main()
            {
                gl_Position = vec4(pos, 1.0);
            }
        ";

        string fragmentShaderSrc = $@"
            #version 330 core

            out vec4 out_col;

            void main()
            {{
                out_col = vec4({(float)Data.Rand.RNG.NextDouble()}, 0.0, 1.0, 0.5);
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

        // Specify how to read vertex data
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, Silk.NET.OpenGL.GLEnum.Float, false, 3 * sizeof(float), (void*)0);

        gl.DrawArrays(Silk.NET.OpenGL.GLEnum.Triangles, 0, (uint)vs.Length);

        // Clean shaders
        gl.DetachShader(prog, vertexShader);
        gl.DetachShader(prog, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        // End stuff
        gl.DeleteVertexArray(vao);
        gl.BindVertexArray(0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ElementArrayBuffer, 0);

        //SDLGlobals.gl.ClearColor(1, 0, 0, 0.5f);


        Control.RecallTarget();
    }
}