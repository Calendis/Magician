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
            List<Point> points = new List<Point>();
            int pointCount = 12;
            double angleIncr = 360 / pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                double deg = angleIncr * i;
                points.Add(new Point(deg/180*Math.PI, 150, true));
            }
            //multis.Add(new Polygon(points.ToArray()));
            //multis.Add(new NonIntersectPolygon(new Color(0xff0000ff), points.ToArray()));
            
            // Define the game objects
            //Plot p = new Plot(0, 0, new Driver(1, (x) => 100*Math.Sin(x[0])), -90, 90, 20, new Color(0x20ff90ff));
            //Polygon p = new Polygon(new Point(-100, 0), new Point(0, 66), new Point(100, 0));
            RegularPolygon p = new RegularPolygon(5, 80);
            multis.Add(p);
            
            Driver[] ds = new Driver[p.Count];
            for (int i = 0; i < ds.Length; i++)
            {
                ds[i] = new Driver(new double[]{i}, (x)=>5*Math.Sin(x[0]), p.Constituents[i].IncrY);
            }
            p.AddSubDrivers(ds);
            p.AddDriver(new Driver((x)=>10*Math.Sin(x[0]), p.IncrX));
                        
            /*
                Main gameloop
            */
            while (!done)
            {
                //Console.WriteLine($"render {frames}");
                Render();
                //Console.WriteLine($"drive {frames}");
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