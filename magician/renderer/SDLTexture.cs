using static SDL2.SDL;

// Wrapper around an SDL texture
namespace Magician.Renderer
{
    public class Texture : IDisposable
    {
        IntPtr texture;
        public int Width { get; private set; }
        public int Height { get; private set; }

        bool disposed = false;

        // Create a texture from an image file
        public Texture(string filepath, int width, int height)
        {

            Width = width;
            Height = height;
            IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, Width, Height, 0, SDL_PIXELFORMAT_ARGB8888);

            surface = SDL2.SDL_image.IMG_Load(filepath);
            if (surface == IntPtr.Zero)
            {
                Console.WriteLine($"Could not load {filepath}");
                surface = SDL2.SDL_image.IMG_Load("magician/ui/assets/default.png");
            }

            texture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, surface);
            SDL_FreeSurface(surface);
        }

        // Create a texture from a given SDL texture
        public Texture(IntPtr texture)
        {
            this.texture = texture;

            // Grab width and height of the rendered text
            unsafe
            {
                Width = ((SDL_Surface*)texture)->w;
                Height = ((SDL_Surface*)texture)->h;
            }
        }

        public void Draw(double xOffset = 0, double yOffset = 0)
        {

            // Options
            SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            // Draw objects
            SDL_SetRenderTarget(SDLGlobals.renderer, texture);
            SDL_Rect srcRect;
            srcRect.x = 0;
            srcRect.y = 0;
            srcRect.w = Width;
            srcRect.h = Height;
            SDL_Rect dstRect;
            dstRect.x = (int)xOffset;
            dstRect.y = (int)yOffset;
            dstRect.w = Width;
            dstRect.h = Height;
            SDL_RenderCopy(SDLGlobals.renderer, texture, ref srcRect, ref dstRect);
        }

        /* IDisposable implementation */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //w = 0;
                    //h = 0;
                }
                // Destroy the texture
                SDL_DestroyTexture(texture);
                texture = IntPtr.Zero;
                disposed = true;
            }
        }

        ~Texture()
        {
            Dispose(false);
        }
    }
}
