using SDL2;

namespace Magician
{   
    class Demo {
        
        static IntPtr win;
        static IntPtr renderer;

        static bool done = false;
        
        static void Main(string[] args)
        {
            // Startup
            Console.WriteLine("Abracadabra!");
            InitSDL();
            CreateWindow();
            CreateRenderer();

            GameLoop();

            // Cleanup
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(win);
            SDL.SDL_Quit();
        }

        static void GameLoop()
        {
            while (!done)
            {
                Console.WriteLine("game-loopin!");
            }
        }

        static void InitSDL()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error initializing SDL: {SDL.SDL_GetError()}");
            }
        }

        static void CreateWindow()
        {
            win = SDL.SDL_CreateWindow("Test Window", 0, 0, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL.SDL_GetError()}");
            }
        }

        static void CreateRenderer()
        {
            renderer = SDL.SDL_CreateRenderer(win, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL.SDL_GetError()}");
            }
        }
    }
}