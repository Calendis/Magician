using static SDL2.SDL;

// Wrapper around an SDL texture
namespace Magician.Renderer
{
    public class Texture : IDisposable
    {
        IntPtr texture;
        int w, h;
        public int Width
        {
            get => w;
        }
        public int Height
        {
            get => h;
        }
        public IntPtr TexIntPtr
        {
            get => texture;
        }
        bool disposed = false;

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
                w = ((SDL_Surface*)texture)->w;
                h = ((SDL_Surface*)texture)->h;
            }
        }
        // Create a texture from a texture
        public Texture(Texture texture)
        {
            this.texture = texture.texture;  // texture
            w = texture.w;
            h = texture.h;
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
            srcRect.w = w;
            srcRect.h = h;
            SDL_Rect dstRect;
            dstRect.x = (int)xOffset;
            dstRect.y = (int)yOffset;
            dstRect.w = w;
            dstRect.h = h;
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
