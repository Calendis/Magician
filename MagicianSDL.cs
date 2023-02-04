using static SDL2.SDL;
using Magician.Library;

namespace Magician
{
    class MagicianSDL
    {
        static IntPtr win;
        bool done = false;
        int frames = 0;
        int stopFrame = -1;
        int driveDelay = 0;
        double timeResolution = 0.1;
        static Spellcaster caster = new Spellcaster();

        static void Main(string[] args)
        {
            /* Startup */
            Console.WriteLine(Data.App.Title);
            MagicianSDL magicianSDL = new MagicianSDL();

            magicianSDL.InitSDL();
            magicianSDL.CreateWindow();
            magicianSDL.CreateRenderer();
            SDL2.SDL_ttf.TTF_Init();

            // Load a spell
            caster.Spellbook.Add(new Demos.SpinningStuff());

            // Run
            magicianSDL.MainLoop();

            // Cleanup
            SDL_DestroyRenderer(SDLGlobals.renderer);
            SDL_DestroyWindow(win);
            SDL_Quit();

        }

        void MainLoop()
        {
            /* TODO: this stuff should be set within a spell */
            caster.PrepareSpell();
            caster.CurrentSpell.PreLoop();

            // Create a surface
            //IntPtr s = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, 400, 300, 0, SDL_PIXELFORMAT_ARGB8888);

            // Create a texture from the surface
            // Textures are hardware-acclerated, while surfaces use CPU rendering
            SDLGlobals.renderedTexture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Data.Globals.winWidth, Data.Globals.winHeight);
            //SDL_FreeSurface(s);

            while (!done)
            {
                caster.CurrentSpell.Loop();
                caster.CurrentSpell.Time = frames * timeResolution;

                // Event handling
                while (SDL_PollEvent(out SDL_Event sdlEvent) != 0 ? true : false)
                {
                    Interactive.Events.Process(sdlEvent);
                    switch (sdlEvent.type)
                    {
                        case SDL_EventType.SDL_QUIT:
                            done = true;
                            break;
                    }
                }

                // Drive things
                if (frames >= driveDelay)
                {
                    Drive();
                }

                // Draw things
                if (frames != stopFrame)
                {
                    Render();
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
                Geo.Ref.Origin.Draw(UI.Perspective.x.Evaluate(), UI.Perspective.y.Evaluate());

                // SAVE FRAME TO IMAGE
                if (Renderer.Control.saveFrame && frames < stopFrame)
                {
                    IntPtr texture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_ARGB8888, 0, Data.Globals.winWidth, Data.Globals.winHeight);
                    IntPtr target = SDL_GetRenderTarget(SDLGlobals.renderer);

                    int width, height;
                    SDL_SetRenderTarget(SDLGlobals.renderer, texture);
                    SDL_QueryTexture(texture, out _, out _, out width, out height);
                    IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, width, height, 0, SDL_PIXELFORMAT_ARGB8888);
                    SDL_Rect r = new SDL_Rect();
                    r.x = 0;
                    r.y = 0;
                    r.w = Data.Globals.winWidth;
                    r.h = Data.Globals.winHeight;
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
                    SDL_DestroyTexture(texture);
                }

                // Display
                SDL_Rect srcRect;
                srcRect.x = 0;
                srcRect.y = 0;
                srcRect.w = Data.Globals.winWidth;
                srcRect.h = Data.Globals.winHeight;
                SDL_Rect dstRect;
                dstRect.x = 0;
                dstRect.y = 0;
                dstRect.w = Data.Globals.winWidth;
                dstRect.h = Data.Globals.winHeight;

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
            Geo.Ref.Origin.Drive();
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
            win = SDL_CreateWindow(Data.App.Title, 0, 0, Data.Globals.winWidth, Data.Globals.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL_GetError()}");
            }
        }
        void CreateRenderer()
        {
            // With VSync
            SDLGlobals.renderer = SDL_CreateRenderer(win, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            // No VSync
            //SDLGlobals.renderer = SDL_CreateRenderer(win, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (SDLGlobals.renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL_GetError()}");
            }
        }
    }
}