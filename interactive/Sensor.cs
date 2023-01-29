using static SDL2.SDL;

namespace Magician.Interactive
{
    public abstract class Sensor : IMap
    {
        public abstract double[] Evaluate(params double[] x);

        public abstract double Evaluate(double x);
    }

    /*
    public class ASCII : Sensor, IMap
    {
        //public double
    }
    */

    public class MouseOver : Sensor, IMap
    {
        Multi m;
        public MouseOver(Multi m)
        {
            this.m = m;
        }

        public override double Evaluate(double x)
        {
            return Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m)?1:0;
        }

        public override double[] Evaluate(params double[] x)
        {
            return new double[] {Geo.Check.PointInPolygon(Events.MouseX, Events.MouseY, m)?1:0};
        }
    }

    public static class Events
    {
        public static Dictionary<SDL_Keycode, bool> keys = new Dictionary<SDL_Keycode, bool>();
        
        static int[] mouse = new int[2];
        static bool getMouse = false;

        public static double MouseX
        {
            get => (double)mouse[0] - Globals.winWidth/2;
        }

        public static double MouseY
        {
            get => (double)-mouse[1] + Globals.winHeight/2;
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
