﻿using static SDL2.SDL;

namespace Magician
{   
    class Demo {
        
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        List<Drawable> mathObjs = new List<Drawa2ble>();
        Random r = new Random();
        int frames = 0;
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
                What do we want to make?
            */
            Quantity q = new Quantity(40);
            mathObjs = new List<Drawable>()
            {
                
                new Plot(-100, 100, new Driver(x => q.Driven(x =>
                {
                    return 100*Math.Abs(Math.Sin(x[0]/10));
                }).Evaluate()*Math.Sin(x[0]/10)), -200, 200, 1, Color.Blue.ToHSL())
                .Driven(x => 100-x[0], "dx")
                .Driven(x => -0.17, "y+")
                .Driven(x => 0.01 + Math.Sin(x[0]/10), "x+")
                .Driven(x => x[0], "col0"),
            };


            //multis.Add(m);
            
            /*
                Main gameloop
            */
            while (!done)
            {
                //SDL_WaitEvent(out SDL_Event events);
                SDL_PollEvent(out SDL_Event sdlEvent);
                Render();
                if (frames > driveDelay)
                {
                    Drive();
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
            Globals.bgCol.R, Globals.bgCol.G, Globals.bgCol.B, 255);
            SDL_RenderClear(renderer);

            // Draw objects
            foreach(Drawable d in mathObjs)
            {
                d.Draw(ref renderer);
            }

            // SAVE FRAME TO IMAGE
            /*
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
                SDL_SaveBMP(surface, $"saved/{frames}.png");
                SDL_FreeSurface(surface);
            }
            */

            frames++;

            // Display
            SDL_RenderPresent(renderer);
            //SDL_Delay(1/6);
        }

        void Drive()
        {
            foreach (Driveable d in mathObjs)
            {
                d.Drive((frames-driveDelay) * timeResolution);
            }

            for (int i = 0; i < Quantity.ExtantQuantites.Count; i++)
            {
                Quantity.ExtantQuantites[i].Drive((frames-driveDelay) * timeResolution);
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