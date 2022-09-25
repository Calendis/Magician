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
            RegularPolygon rp = new RegularPolygon(5, 100);
            Multi rpm = rp.Multi();

            for (int i = 0; i < rpm.Count; i++)
            {
                Multi rpm2 = new RegularPolygon(5, 70).Multi();
                for (int j = 0; j < rpm2.Count; j++)
                {
                    rpm2.SetConstituent(j, new RegularPolygon(5, 20));
                    rpm2.Constituents[j].AddDriver(new Driver(x=>0.2, rpm2.Constituents[j].IncrPhase));
                    rpm2.Constituents[j].AddDriver(new Driver(x=>-Math.Sin(x[0]/10), rpm2.Constituents[j].IncrMagnitude));
                    rpm2.AddDriver(new Driver((x)=>2, rpm2.IncrPhase));
                }
                rpm.SetConstituent(i, rpm2);
            }
            
            Driver[] subDrivers = new Driver[rpm.Count];
            Driver[] subDrivers2 = new Driver[rpm.Count];
            for (int i = 0; i < subDrivers.Length; i++)
            {
                subDrivers[i] = new Driver((x)=>0.1, rpm.Constituents[i].IncrPhase);
                subDrivers2[i] = new Driver((x)=>3*Math.Sin(x[0]), rpm.Constituents[i].IncrMagnitude);
            }
            rpm.AddSubDrivers(subDrivers, subDrivers2);
            //rpm.AddDriver(new Driver((x)=>10*Math.Sin(x[0]), rpm.IncrX));
            multis.Add(rpm);
                        
            /*
                Main gameloop
            */
            while (!done)
            {
                SDL_WaitEvent(out _);
                Render();
                Drive();
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

            // Display
            SDL_RenderPresent(renderer);

            frames++;
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