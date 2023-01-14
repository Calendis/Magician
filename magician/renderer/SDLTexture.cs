using System;
using static SDL2.SDL;

// Wrapper around an SDL texture with Color supprt
namespace Magician.Renderer
{
    public class Texture
    {
        IntPtr texture;
        int w, h;

        // Create a texture from an image file
        public Texture(string filepath, int width, int height)
        {
            
            w = width;
            h = height;
            IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, w, h, 0, SDL_PIXELFORMAT_ARGB8888);

            surface = SDL2.SDL_image.IMG_Load(filepath);
            if (surface == (IntPtr)0)
            {
                Console.WriteLine($"Could not load {filepath}");
                surface = SDL2.SDL_image.IMG_Load("ui/assets/default.png");
            }

            texture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, surface);
            SDL_FreeSurface(surface);
        }

        public void Draw(int xOffset=0, int yOffset=0)
        {
            // Options
            SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            // Draw objects
            SDL_SetRenderTarget(SDLGlobals.renderer, texture);
            SDL_Rect srcRect;
            srcRect.x = xOffset;
            srcRect.y = yOffset;
            srcRect.w = w;
            srcRect.h = h;
            SDL_Rect dstRect;
            dstRect.x = xOffset;
            dstRect.y = yOffset;
            dstRect.w = w;
            dstRect.h = h;
            SDL_RenderCopy(SDLGlobals.renderer, texture, ref srcRect, ref dstRect);
        }
    }
}
