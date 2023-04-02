using static SDL2.SDL;
using Silk.NET.OpenGL;

namespace Magician.Renderer;
public static class Control
{
    public static bool doRender = true;
    public static bool display = true;
    public static bool saveFrame = false;
    public static int saveCount = 0;
    static IntPtr target;
    public static void Clear()
    {
        Clear(Data.Col.UIDefault.BG);
    }
    public static void Clear(Color c)
    {
        SaveTarget();
        //SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
        //SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
        //SDL_RenderClear(SDLGlobals.renderer);
        SDLGlobals.gl.ClearColor((float)c.R/255f, (float)c.G/255f, (float)c.B/255f, (float)c.A/255f);
        SDLGlobals.gl.Clear(ClearBufferMask.ColorBufferBit);
        RecallTarget();
    }

    public static void SaveTarget()
    {
        //target = SDL_GetRenderTarget(SDLGlobals.renderer);
    }

    public static void RecallTarget()
    {
        //SDL_SetRenderTarget(SDLGlobals.renderer, target);
    }

}