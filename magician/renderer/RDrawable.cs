using System.Runtime.InteropServices;

namespace Magician.Renderer;

internal abstract class RDrawable
{
    public int Layer { get; set; }
    public byte[] rgba = new byte[4];
    public abstract void Draw();
    public static List<RDrawable> drawables = new List<RDrawable>();
    protected static Silk.NET.OpenGL.GL gl = Renderer.SDLGlobals.gl;

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
    float[] vs;

    static int posLength = 3;
    static int rgbLength = 4;
    static int dataLength = 3*(posLength+rgbLength);
    public RGeometry(params RTriangle[] rts)
    {
        int numTriangles = rts.Length;
        vs = new float[numTriangles * dataLength];  // x y z r g b a x y z r g b a x y z r g b a ...
        for (int i = 0; i < numTriangles; i++)
        {
            RTriangle currentTriangle = rts[i];

            vs[dataLength * i + 0] = currentTriangle.p0[0] / Data.Globals.winWidth;
            vs[dataLength * i + 1] = currentTriangle.p0[1] / Data.Globals.winHeight;
            vs[dataLength * i + 2] = currentTriangle.p0[2] / 800;

            vs[dataLength * i + 7] = currentTriangle.p1[0] / Data.Globals.winWidth;
            vs[dataLength * i + 8] = currentTriangle.p1[1] / Data.Globals.winHeight;
            vs[dataLength * i + 9] = currentTriangle.p1[2] / 800;

            vs[dataLength * i + 14] = currentTriangle.p2[0] / Data.Globals.winWidth;
            vs[dataLength * i + 15] = currentTriangle.p2[1] / Data.Globals.winHeight;
            vs[dataLength * i + 16] = currentTriangle.p2[2] / 800;
            
            // Color
            vs[dataLength * i + 3] = rts[i].rgba[0] / 255f;
            vs[dataLength * i + 4] = rts[i].rgba[1] / 255f;
            vs[dataLength * i + 5] = rts[i].rgba[2] / 255f;
            vs[dataLength * i + 6] = rts[i].rgba[3] / 255f;

            vs[dataLength * i + 10] = rts[i].rgba[0] / 255f;
            vs[dataLength * i + 11] = rts[i].rgba[1] / 255f;
            vs[dataLength * i + 12] = rts[i].rgba[2] / 255f;
            vs[dataLength * i + 13] = rts[i].rgba[3] / 255f;

            vs[dataLength * i + 17] = rts[i].rgba[0] / 255f;
            vs[dataLength * i + 18] = rts[i].rgba[1] / 255f;
            vs[dataLength * i + 19] = rts[i].rgba[2] / 255f;
            vs[dataLength * i + 20] = rts[i].rgba[3] / 255f;
        }
    }

    public override unsafe void Draw()
    {
        Control.SaveTarget();

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

        //gl.VertexAttribPointer(0, 3, Silk.NET.OpenGL.GLEnum.Float, false, 3*sizeof(float), (void*)0);
        // Specify how to read vertex data
        gl.VertexAttribPointer(0, posLength, Silk.NET.OpenGL.GLEnum.Float, false, (uint)(posLength+rgbLength)*sizeof(float), (void*)0);
        gl.VertexAttribPointer(1, rgbLength, Silk.NET.OpenGL.GLEnum.Float, true, (uint)(posLength+rgbLength)*sizeof(float), (void*)(posLength*sizeof(float)));
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(0);
        
        
        //gl.BindFragDataLocation()
        gl.DrawArrays(Silk.NET.OpenGL.GLEnum.Triangles, 0, (uint)vs.Length);

        // End stuff
        gl.DeleteVertexArray(vao);
        gl.BindVertexArray(0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ElementArrayBuffer, 0);

        //SDLGlobals.gl.ClearColor(1, 0, 0, 0.5f);


        Control.RecallTarget();
    }
}