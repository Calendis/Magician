using static SDL2.SDL;

namespace Magician
{   
    class Demo {
        
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        List<Multi> multis = new List<Multi>();
        Random r = new Random();
        int frames = 0;
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
                Game setup
            */
            Multi colShifter = Multi.RegularPolygon(0, 0, new Color(180f, 1, 1, 255), 5, 120)
            .Driven(new Driver(x => 180*Math.Sin(x[0]/10)+180), "col0")
            ;
            //Multi m0 = Multi.RegularPolygon(-250, 0, new Color(0x00ffffff), 4, 120)
            //.Driven(new Driver(x => 0.02), "phase+")
            //;

            //Multi m1 = Multi.RegularPolygon(250, 0, new Color(0x00ffffff, true), 6, 60)
            //;

            //Plot p = new Plot(0, 0, new Driver(x => x[0]*x[0]), -50, 50, 0.1, Color.RED);

            //multis.Add(m0);
            //multis.Add(m1);
            multis.Add(colShifter);
            
            //multis.Add(p.Interpolation().Wielding(Multi.RegularPolygon(0, 0, 5, 50)));
            /*
                Main gameloop
            */
            while (!done)
            {
                //SDL_WaitEvent(out SDL_Event events);
                SDL_PollEvent(out SDL_Event sdlEvent);
                Render();
                Drive();

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
            // Clear with colour
            SDL_SetRenderDrawColor(renderer,
            Globals.bgCol.R, Globals.bgCol.G, Globals.bgCol.B, 255);
            SDL_RenderClear(renderer);

            // Draw objects
            foreach(Multi m in multis)
            {
                m.Draw(ref renderer);
            }

            frames++;

            // Display
            SDL_RenderPresent(renderer);
        }

        void Drive()
        {
            foreach (Multi m in multis)
            {
                m.Drive(frames * timeResolution);
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