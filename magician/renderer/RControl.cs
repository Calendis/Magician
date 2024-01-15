using Silk.NET.OpenGL;

namespace Magician.Renderer;
public static class RControl
{
    public static bool doRender = true;
    public static bool display = true;
    public static bool saveFrame = false;
    public static int saveCount = 0;
    static IntPtr target;
    public static void Clear()
    {
        Clear(Runes.Col.UIDefault.BG);
    }
    public static void Clear(Color c)
    {
        if (RGlobals.gl is null)
            throw Scribe.Error("Cannot clear uninitialized gl context");
            
        RGlobals.gl.ClearColor((float)c.R/255f, (float)c.G/255f, (float)c.B/255f, (float)c.A/255f);
        RGlobals.gl.Clear(ClearBufferMask.ColorBufferBit);
    }

}