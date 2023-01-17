using static SDL2.SDL;

namespace Magician.Renderer
{
    public static class Control
    {
        public static void Clear()
        {
            SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
            SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)Ref.UIDefault.BG.R, (byte)Ref.UIDefault.BG.G, (byte)Ref.UIDefault.BG.B, (byte)Ref.UIDefault.BG.A);
            SDL_RenderClear(SDLGlobals.renderer);
        }
    }
}