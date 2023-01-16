using static SDL2.SDL;

namespace Magician.Renderer
{
    public static class Control
    {
        public static void Clear()
        {
            SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)Ref.bgCol.R, (byte)Ref.bgCol.G, (byte)Ref.bgCol.B, (byte)Ref.bgCol.A);
            SDL_RenderClear(SDLGlobals.renderer);
        }
    }
}