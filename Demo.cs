using static SDL2.SDL;

namespace Magician
{   
    class Demo {
        
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        List<Drawable> mathObjs = new List<Drawable>();
        Random r = new Random();
        int frames = 0;
        int stopFrame = -1;
        bool saveFrames = false;
        int driveDelay = 0;
        double timeResolution = 0.25;
        
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
            Multi m = new Plot(-200, 0, new Driver(x => 100*Math.Cos(x[0]/10)), 0, 200, 1, Color.Green.ToHSL()).Interpolation();
            //m.Lined = false;
            Quantity q = new Quantity(1).Driven(x => x[0]);
            Quantity q2 = new Quantity(0).Driven(x => Math.Abs(Math.Sin(x[0])));

            Quantity.ExtantQuantites.Add(q);
            Quantity.ExtantQuantites.Add(q2);
            Random r = new Random();

            mathObjs = new List<Drawable>() {};
            
            /*
                Main gameloop
            */
            while (!done)
            {
                mathObjs.Clear();
                /*
                mathObjs.Add(m
                .Where(dr => ((Multi)dr)
                    .Eject()
                    .Driven(x => 
                    {
                        return dr.XAbsolute(0) / 200;
                    }, "y+")

                    .Driven(x => 
                    {
                        return 10*Math.Sin(x[0]);
                    }, "x+")
                    //.Driven(x => q.Evaluate(), "col0")
                    )
                );*/
                mathObjs.Add(
                    new Plot(0, 0, new Driver(x => x[0]*x[0]*0.0625), -200, 200, 1, Color.Blue).Interpolation()
                    //.SubDriven(x => 0.02, "phase+")
                    
                    .Where(c => ((Multi)c)
                        //.Eject()
                        .Driven(x => -q.Evaluate() + c.YAbsolute(0) + 20*Math.Sin(c.XAbsolute(0)/10+q.Evaluate()), "y")
                    )
                    .Wielding(Multi.RegularPolygon(0, 0, Color.Blue, 4, 20)
                        .Where(c => ((Multi)c)
                            .Driven(x => q.Evaluate()*0.1 + c.Phase, "phase")
                            )
                        )
                    .Filter(x => Math.Cos(Math.PI*x), q2.Evaluate())
                );
                
                //SDL_WaitEvent(out SDL_Event events);
                SDL_PollEvent(out SDL_Event sdlEvent);
                if (frames > driveDelay)
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
            Globals.bgCol.R, Globals.bgCol.G, Globals.bgCol.B, 255);
            SDL_RenderClear(renderer);

            // Draw objects
            foreach(Drawable d in mathObjs)
            {
                d.Draw(ref renderer);
            }

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
                    SDL_SaveBMP(surface, $"saved/frame_{frames.ToString("D3")}.bmp");
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