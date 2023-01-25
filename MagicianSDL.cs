using static SDL2.SDL;

namespace Magician
{
    class MagicianSDL
    {
        static IntPtr win;

        bool done = false;
        Random r = new Random();
        int frames = 0;
        int stopFrame = -1;
        
        int driveDelay = 0;
        double timeResolution = 0.1;

        static void Main(string[] args)
        {
            // Startup
            Console.WriteLine("Abracadabra!");

            SDL2.SDL.SDL_version d;
            SDL2.SDL_ttf.SDL_TTF_VERSION(out d);
            Console.WriteLine($"SDL_ttf version: {d.major}.{d.minor}.{d.patch}");
            SDL2.SDL_ttf.TTF_Init();

            MagicianSDL demo = new MagicianSDL();
            demo.InitSDL();
            demo.CreateWindow();
            demo.CreateRenderer();

            demo.GameLoop();

            // Cleanup
            SDL_DestroyRenderer(SDLGlobals.renderer);
            SDL_DestroyWindow(win);
            SDL_Quit();
        }

        void GameLoop()
        {
            // Cast a spell
            Spell.PreLoop(ref frames, ref timeResolution);

            // Create a surface
            IntPtr s = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, 400, 300, 0, SDL_PIXELFORMAT_ARGB8888);

            // Create a texture from the surface
            // Textures are hardware-acclerated, while surfaces use CPU rendering
            SDLGlobals.renderedTexture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Ref.winWidth, Ref.winHeight);
            SDL_FreeSurface(s);

            while (!done)
            {
                // Cast a spell
                Spell.Loop(ref frames, ref timeResolution);

                // Control flow and SDL
                SDL_PollEvent(out SDL_Event sdlEvent);
                if (frames >= driveDelay)
                {
                    Drive();
                }
                if (frames != stopFrame)
                {
                    Render();
                }

                // Event handling
                switch (sdlEvent.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        done = true;
                        break;
                    
                    default:
                        Interactive.Events.Process(sdlEvent);
                        break;
                    
                }
            }
        }

        // Renders each frame to a texture and displays the texture
        void Render()
        {
            if (Renderer.Control.doRender)
            {
                // Options
                SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL_SetTextureBlendMode(SDLGlobals.renderedTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

                // Draw objects
                SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
                Geo.Origin.Draw(0, 0);

                // SAVE FRAME TO IMAGE
                if (Renderer.Control.saveFrame)
                {
                    IntPtr texture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_ARGB8888, 0, Ref.winWidth, Ref.winHeight);
                    IntPtr target = SDL_GetRenderTarget(SDLGlobals.renderer);

                    int width, height;
                    SDL_SetRenderTarget(SDLGlobals.renderer, texture);
                    SDL_QueryTexture(texture, out _, out _, out width, out height);
                    IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, width, height, 0, SDL_PIXELFORMAT_ARGB8888);
                    SDL_Rect r = new SDL_Rect();
                    r.x = 0;
                    r.y = 0;
                    r.w = Ref.winWidth;
                    r.h = Ref.winHeight;
                    unsafe
                    {
                        SDL_SetRenderTarget(SDLGlobals.renderer, IntPtr.Zero);
                        
                        SDL_Surface* surf = (SDL_Surface*)surface;
                        SDL_RenderReadPixels(SDLGlobals.renderer, ref r, SDL_PIXELFORMAT_ARGB8888, surf->pixels, surf->pitch);
                        SDL_SaveBMP(surface, $"saved/frame_{Renderer.Control.saveCount.ToString("D4")}.bmp");
                        Renderer.Control.saveCount++;
                        SDL_FreeSurface(surface);

                        SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
                    }
                }

                // Display
                SDL_Rect srcRect;
                srcRect.x = 0;
                srcRect.y = 0;
                srcRect.w = Ref.winWidth;
                srcRect.h = Ref.winHeight;
                SDL_Rect dstRect;
                dstRect.x = 0;
                dstRect.y = 0;
                dstRect.w = Ref.winWidth;
                dstRect.h = Ref.winHeight;

                SDL_SetRenderTarget(SDLGlobals.renderer, IntPtr.Zero);
                SDL_RenderCopy(SDLGlobals.renderer, SDLGlobals.renderedTexture, ref srcRect, ref dstRect);
                if (Renderer.Control.display)
                {
                    SDL_RenderPresent(SDLGlobals.renderer);
                }
                //SDL_Delay(1/6);
            }
            frames++;
        }

        // Drive the dynamics of Multis and Quantities
        void Drive()
        {
            Geo.Origin.Go((frames - driveDelay) * timeResolution);
            for (int i = 0; i < Quantity.ExtantQuantites.Count; i++)
            {
                Quantity.ExtantQuantites[i].Go((frames - driveDelay) * timeResolution);
            }
        }
        void InitSDL()
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error initializing SDL: {SDL_GetError()}");
            }
        }
        void CreateWindow()
        {
            win = SDL_CreateWindow("Test Window", 0, 0, Ref.winWidth, Ref.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL_GetError()}");
            }
        }
        void CreateRenderer()
        {
            SDLGlobals.renderer = SDL_CreateRenderer(win, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (SDLGlobals.renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL_GetError()}");
            }
        }
    }
}