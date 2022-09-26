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
            
            /*Multi m = new Multi(
                new Line(new Point(0, 0), new Point(-100, -100)),
                new Line(new Point(0, 0), new Point(100, 100)),
                new Line(new Point(0, 0), new Point(100, -100)),
                new Line(new Point(0, 0), new Point(-100, 100)));*/

            Multi m = new Multi(
                new Point(0, 0), new Point(-100, -100),
                new Point(0, 0), new Point(100, 100),
                new Point(0, 0), new Point(100, -100),
                new Point(0, 0), new Point(-100, 100));
            
            m.Recurse();
            //Line m = new Line(new Point(0, 0), new Point(100, 100));
            //Plot m = new Plot(0, 0, new Driver(x => 100*Math.Sin(x[0] / 40)), -50, 50, 0.1, new Color(0xff0000ff));
            //Multi m = p.Interpolation();
            //m.AddDriver(new Driver(x => Math.Sin(x[0]), m.IncrX));
            
            multis.Add(m);
                        
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