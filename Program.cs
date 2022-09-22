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
            Demo demo = new Demo();
            demo.InitSDL();
            demo.CreateWindow();
            demo.CreateRenderer();

            demo.GameLoop();

            // Cleanup
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(win);
            SDL.SDL_Quit();
        }

        void GameLoop()
        {
            while (!done)
            {
                SDL.SDL_WaitEvent(out SDL.SDL_Event e);
                Render();
            }
        }
        
        void Render()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 255, 255);
            SDL.SDL_RenderClear(renderer);

            SDL.SDL_RenderPresent(renderer);
        }

        void InitSDL()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"Error initializing SDL: {SDL.SDL_GetError()}");
            }
        }

        void CreateWindow()
        {
            win = SDL.SDL_CreateWindow("Test Window", 0, 0, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the window: {SDL.SDL_GetError()}");
            }
        }

        void CreateRenderer()
        {
            renderer = SDL.SDL_CreateRenderer(win, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"Error creating the renderer: {SDL.SDL_GetError()}");
            }
        }
    }
}