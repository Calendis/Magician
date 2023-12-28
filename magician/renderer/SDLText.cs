using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Magician.Renderer;

public class Text : IDisposable
{
    public static string FallbackFontPath = "";
    string fontPath;

    string s;
    Color c;
    int size;
    IntPtr font;
    bool disposed = false;

    public Text(string s, Color c, int size, string fp = "")
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
        //font = TTF_OpenFont(fontPath, size);
    }

    public _SDLTexture Render()
    {
        if (font == IntPtr.Zero)
        {
            Scribe.Error($"Null font pointer. {SDL_GetError()}");
        }

        // Create an SDL color from a Color
        //SDL_Color sdlC;
        //sdlC.r = (byte)c.R;
        //sdlC.g = (byte)c.G;
        //sdlC.b = (byte)c.B;
        //sdlC.a = (byte)c.A;
        //IntPtr textSurface = TTF_RenderText_Solid(font, s, sdlC);
        //IntPtr textSurface = TTF_RenderText_Blended(font, s, sdlC);
        //IntPtr textSurface = TTF_RenderText_Shaded(font, s, sdlC, Data.Col.UIDefault.BG);
        /* if (textSurface == IntPtr.Zero)
        {
            Scribe.Error($"{SDL_GetError()}");
        } */

        //IntPtr textTexture = SDL_CreateTextureFromSurface(SDLGlobals.renderer, textSurface);
        //SDL_FreeSurface(textSurface);
        //return new Texture(textTexture);
        return new _SDLTexture("todo", 80, 80);
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