using static SDL2.SDL;

namespace Magician.Interactive
{
    public static class Events
    {
        // Keymap generated from SDL enum
        public static Dictionary<SDL_Keycode, bool> keys = new Dictionary<SDL_Keycode, bool>();

        // Flags
        static bool getMouse = false;
        static bool getScroll = false;

        // Event state
        static int[] mouse = new int[2];
        static float[] scroll = new float[2];

        public static double MouseX
        {
            get => (double)mouse[0] - Data.Globals.winWidth / 2;
        }
        public static double MouseY
        {
            get => (double)-mouse[1] + Data.Globals.winHeight / 2;
        }

        public static double ScrollX
        {
            get => (double)scroll[0];
        }
        public static double ScrollY
        {
            get => (double)scroll[1];
        }

        static Events()
        {
            SDL_Keycode[] SDLKeys = Enum.GetValues<SDL_Keycode>();
            foreach (SDL_Keycode sdlKC in SDLKeys)
            {
                keys.Add(sdlKC, false);
            }
        }

        public static void Process(SDL_Event e)
        {
            switch (e.type)
            {
                case SDL_EventType.SDL_KEYDOWN:
                    keys[e.key.keysym.sym] = true;
                    break;

                case SDL_EventType.SDL_KEYUP:
                    keys[e.key.keysym.sym] = false;
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    getMouse = true;
                    break;

                case SDL_EventType.SDL_MOUSEWHEEL:
                    scroll[0] = e.wheel.preciseX;
                    scroll[1] = e.wheel.preciseY;
                    break;

                default:
                    break;
            }

            ResetFlags();
        }

        static void ResetFlags()
        {
            if (getMouse)
            {
                getMouse = false;
                SDL_GetMouseState(out mouse[0], out mouse[1]);
            }

        }
    }
}