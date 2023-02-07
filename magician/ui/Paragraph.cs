using Magician.Data;
using Magician.Renderer;

using static SDL2.SDL;

namespace Magician.UI
{
    public class Paragraph
    {
        string[] sentences;
        Text[] texts;

        public Paragraph(Color? c = null, string fontPath="", params string[] ss)
        {
            sentences = new string[ss.Length];
            texts = new Text[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                sentences.Append(ss[i]);
                texts[i] = new Text(ss[i], c ?? Col.UIDefault.FG, fontPath);
            }
        }
        public Paragraph(string s, Color? c=null, char delimiter = '\n', string fp="") : this(c, fp, s.Split(delimiter)) { }

        public Texture Render()
        {
            throw new NotImplementedException("Multi-line text still broken, sorry lol");
            Texture[] textures = new Texture[texts.Length];
            for (int i = 0; i < texts.Length; i++)
            foreach (Text tx in texts)
            {
                textures[i] = texts[i].Render();
            }

            // Stitch all the textures into one
            IntPtr finalTexture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_ARGB8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Data.Globals.winWidth, Data.Globals.winHeight);
            SDL_Rect dstRect;
            dstRect.x = 0;
            dstRect.y = 0;
            dstRect.w = Data.Globals.winWidth;
            dstRect.h = Data.Globals.winHeight;

            // For debugging, load an image
            //Texture imgTex = new Texture("")

            int j = 0;
            foreach (Texture tex in textures)
            {
                SDL_Rect srcRect;
                srcRect.w = tex.Width;
                srcRect.h = tex.Height;
                srcRect.x = 0;
                srcRect.y = tex.Height * j++;
                
                SDL_SetRenderTarget(SDLGlobals.renderer, finalTexture);
                int code = SDL_RenderCopy(SDLGlobals.renderer, tex.TexIntPtr, ref srcRect, ref dstRect);
                SDL_SetRenderTarget(SDLGlobals.renderer, IntPtr.Zero);
            }
            return new Texture(finalTexture);
        }
    }
}