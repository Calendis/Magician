﻿using static SDL2.SDL;

namespace Magician
{
    class Demo
    {
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        Random r = new Random();
        int frames = 0;
        int stopFrame = -1;
        bool saveFrames = false;
        int driveDelay = 0;
        double timeResolution = 0.1;

        static void Main(string[] args)
        {
            // Startup
            Console.WriteLine("Abracadabra!");

            Demo demo = new Demo();
            demo.InitSDL();
            demo.CreateWindow();
            demo.CreateRenderer();

            demo.GameLoop();

            // Cleanup
            SDL_DestroyRenderer(renderer);
            SDL_DestroyWindow(win);
            SDL_Quit();
        }

        void GameLoop()
        {
            /*
            *  Pre-loop
            *  -----------------------------------------------------------------
            *  Much is possible in the pre-loop, since Drivers will still work
            */
            List<Multi> lines = new List<Multi>();
            
            Geo.Origin.Add(Geo.Line(
                Geo.Point(80, -200).DrawFlags(DrawMode.INVISIBLE),
                Geo.Point(80, -100).DrawFlags(DrawMode.INVISIBLE),
                new RGBA(0xff00ffff)
            ));

            Geo.Origin.Add(Geo.RegularPolygon(300, 100, new RGBA(0x55d000ff), 5, 100)
            .Sub(m => m.Driven(x => 0.01, "phase+")));

            /*
            *  Loop
            *  ----------------------------------------------------------------
            *  The loop automatically drives and renders the math objects.
            *  When you want to modulate arguments in a constructor, you will
            *  need the loop
            */
            while (!done)
            {                
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
                }
            }
        }

        void Render()
        {
            // Clear with colour)
            SDL_SetRenderDrawColor(renderer,
            (byte)Globals.bgCol.R, (byte)Globals.bgCol.G, (byte)Globals.bgCol.B, (byte)Globals.bgCol.A);
            SDL_RenderClear(renderer);

            // Draw objects
            Geo.Origin.Draw(ref renderer, 0, 0);

            // SAVE FRAME TO IMAGE
            if (saveFrames)
            {
                IntPtr texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, 0, Globals.winWidth, Globals.winHeight);
                IntPtr target = SDL_GetRenderTarget(renderer);

                int width, height;
                SDL_SetRenderTarget(renderer, texture);
                SDL_QueryTexture(texture, out _, out _, out width, out height);
                IntPtr surface = SDL_CreateRGBSurfaceWithFormat(SDL_RLEACCEL, width, height, 0, SDL_PIXELFORMAT_ARGB8888);
                SDL_Rect r = new SDL_Rect();
                r.x = 0;
                r.y = 0;
                r.w = Globals.winWidth;
                r.h = Globals.winHeight;
                unsafe
                {
                    SDL_Surface* surf = (SDL_Surface*)surface;
                    SDL_RenderReadPixels(renderer, ref r, SDL_PIXELFORMAT_ARGB8888, surf->pixels, surf->pitch);
                    SDL_SaveBMP(surface, $"saved/frame_{frames.ToString("D4")}.bmp");
                    SDL_FreeSurface(surface);
                }
            }

            frames++;

            // Display
            SDL_RenderPresent(renderer);
            //SDL_Delay(1/6);
        }

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
            win = SDL_CreateWindow("Test Window", 0, 0, Globals.winWidth, Globals.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL_GetError()}");
            }
        }
        void CreateRenderer()
        {
            renderer = SDL_CreateRenderer(win, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL_GetError()}");
            }
        }
    }
}