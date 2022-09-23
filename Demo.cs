using static SDL2.SDL;

namespace Magician
{   
    class Demo {
        
        static IntPtr win;
        static IntPtr renderer;

        bool done = false;
        List<Multi> multis = new List<Multi>();
        Random r = new Random();
        
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
            // Game setup
            List<Point> points = new List<Point>();
            int pointCount = 12;
            double angleIncr = 360 / pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                double deg = angleIncr * i;
                points.Add(new Point(deg/180*Math.PI, 150, true));
            }
            multis.Add(new Polygon(points.ToArray()));
            multis.Add(new NonIntersectPolygon(new Color(0xff0000ff), points.ToArray()));

            
            while (!done)
            {
                SDL_WaitEvent(out SDL_Event e);
                Render();

                // Modify polygon
                
                int c = multis[0].Constituents.Count;
                if (c > 5 && r.Next(10) < 5)
                {
                    int ri = r.Next(c);
                    multis[0].Constituents.RemoveAt(ri);
                    multis[1].Constituents.RemoveAt(ri);

                }
                else if (r.Next(10) > 5)
                {
                    int rx = r.Next(-200, 200);
                    int ry = r.Next(-200, 200);
                    Point p0  = new Point(rx, ry);
                    Point p1  = new Point(rx, ry, new Color(0xff0000ff));                  
                    multis[0].Constituents.Add(p0);
                    multis[1].Constituents.Add(p1);
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

            // Display
            SDL_RenderPresent(renderer);
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