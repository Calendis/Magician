using static SDL2.SDL;

namespace Magician.Interactive
{
    public class Sensor : IMap
    {
        public double[] Evaluate(params double[] x)
        {
            throw new NotImplementedException();
        }

        public double Evaluate(double x)
        {
            throw new NotImplementedException();
        }
    }

    public class ASCII : Sensor, IMap
    {
        //public double
    }

    public static class Events
    {
        public static Dictionary<SDL_Keycode, bool> keys = new Dictionary<SDL_Keycode, bool>();
        
        static int[] mouse = new int[2];
        static bool getMouse = false;

        public static double MouseX
        {
            get => (double)mouse[0] - Ref.winWidth/2;
        }

        public static double MouseY
        {
            get => (double)-mouse[1] + Ref.winHeight/2;
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
                    //goto case SDL_EventType.SDL_KEYUP;
                    break;
                
                case SDL_EventType.SDL_KEYUP:
                    keys[e.key.keysym.sym] = false;
                    //goto case SDL_EventType.SDL_MOUSEMOTION;
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    getMouse = true;
                    //SDL_GetMouseState(out mouse[0], out mouse[1]);
                    break;

                default:
                    //SDL_GetMouseState(out mouse[0], out mouse[1]);
                    //Console.WriteLine($"Could not process event {e.type}");
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
