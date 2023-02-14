using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Magician.Renderer
{
    public class Text : IDisposable
    {
        public static string FallbackFontPath = "";
        string fontPath;

        string s;
        Color c;
        int size;
        IntPtr font;
        bool disposed = false;

        public Text(string s, Color c, int size, string fp="")
        {
            if (FallbackFontPath == "")
            {
                throw new InvalidDataException("Must set fallback font path before using Text");
            }
            this.s = s;
            this.c = c;
            this.size = size;
            fontPath = fp == "" ? FallbackFontPath : fp;
            // WIP text support
            // Open the default font
            font = TTF_OpenFont(fontPath, size);
        }

        public Texture Render()
        {
            if (font == (IntPtr)0)
            {
                Scribe.Error($"{SDL_GetError()}");
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
                Scribe.Error($"{SDL_GetError()}");
            }

            IntPtr textTexture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, textSurface);
            SDL_FreeSurface(textSurface);
            return new Texture(textTexture);

            //throw new NotImplementedException("Text as Texture not supported. Please file an issue at https://github.com/Calendis/Magician");
        }

        Multi AsMulti()
        {
            Scribe.Error("Text as Multi not supported yet");
            throw new Exception();
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

        public override string ToString()
        {
            return $"Text {s}";
        }
    }
}