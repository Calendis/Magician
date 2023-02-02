using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Magician.Renderer
{
    public class Text : IDisposable
    {
        string s;
        Color c;
        IntPtr font;
        bool disposed = false;

        public Text(string s, Color c)
        {
            this.s = s;
            this.c = c;
            // Open the default fony
            font = TTF_OpenFont("magician/ui/assets/fonts/Space_Mono/SpaceMono-Regular.ttf", Data.Globals.fontSize);
        }

        public Texture Render()
        {
            //IntPtr font = (IntPtr)0;
            if (font == (IntPtr)0)
            {
                Console.WriteLine($"Failed to load font: {SDL_GetError()}");
            }
            
            // Create an SDL color from a Color
            SDL_Color sdlC;
            sdlC.r = (byte)c.R;
            sdlC.g = (byte)c.G;
            sdlC.b = (byte)c.B;
            sdlC.a = (byte)c.A;
            IntPtr textSurface = TTF_RenderText_Solid(font, s, sdlC);
            if (textSurface == (IntPtr)0)
            {
                Console.WriteLine($"A horrible error occured: {SDL_GetError()}");
            }

            IntPtr textTexture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, textSurface);
            SDL_FreeSurface(textSurface);

            return new Texture(textTexture);
            
            //throw new NotImplementedException("Text as Texture not supported. Please file an issue at https://github.com/Calendis/Magician");
        }

        Multi AsMulti()
        {
            throw new NotImplementedException("Text as Multi not supported. Please file an issue at https://github.com/Calendis/Magician");
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
                }
                // Close the font
                TTF_CloseFont(font);
                font = IntPtr.Zero;
                disposed = true;
            }
        }

        ~Text()
        {
            Dispose(false);
        }
    }
}