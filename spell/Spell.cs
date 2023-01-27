// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using static SDL2.SDL;

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Random rng = new Random();

        public static void PreLoop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
            Geo.Origin.Add(UI.Presets.Graph.Cartesian());

            Multi pentagon = Geo.RegularPolygon(0, 0, new RGBA(0x00f01070), 5, 120)
                .Sub(m => m
                    .Textured(new Text($"{m.Index}", new RGBA(0xff00ffff)).Render())
                );

            Geo.Origin.Add(
                pentagon
            );
            pentagon
            .Driven(x => Interactive.Events.MouseX, "x")
            .Driven(x => Interactive.Events.MouseY, "y")
            ;
            
            pentagon.Sub(m => m
            .Driven(x => Interactive.Events.keys[SDL_Keycode.SDLK_a]?0.035:0, "phase+")
            .Driven(x => Interactive.Events.keys[SDL_Keycode.SDLK_d]?-0.035:0, "phase+")
            );

            pentagon.Sub(
                m => m.Driven(x => 0.01, "phase+")
            );

        }

        // For stuff that needs to redefined every frame
        public static void Loop(ref int frames, ref double timeResolution)
        {
            Renderer.Control.Clear();
        }
    }
}