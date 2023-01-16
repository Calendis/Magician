using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Magician.Renderer
{
    public class Text
    {
        string s;
        Color c;

        public Text(string s, Color c)
        {
            this.s = s;
            this.c = c;
        }

        public Texture Render()
        {
            // Load the default font
            IntPtr font = TTF_OpenFont("ui/assets/fonts/Space_Mono/SpaceMono-Regular.ttf", Ref.fontSize);
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
    }
}