using static SDL2.SDL;

namespace Magician.Renderer
{
    public static class Control
    {
        public static bool doRender = true;
        public static bool display = true;
        public static bool saveFrame = false;
        public static int saveCount = 0;
        public static void Clear()
        {
            Clear(Data.Col.UIDefault.BG);
        }
        public static void Clear(Color c)
        {
            SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
            SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
            SDL_RenderClear(SDLGlobals.renderer);
            //SDL_SetRenderTarget(SDLGlobals.renderer, IntPtr.Zero);
        }
    }
}