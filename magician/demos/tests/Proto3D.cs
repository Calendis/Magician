using Magician.Geo;
using Magician.Interactive;
using Magician.Library;

namespace Magician.Demos.Tests;

public class Proto3D : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["2"].RotatedZ(-0.009);
        Origin["tetra"].RotatedX(-0.0025);
        Origin["tetra"].RotatedY(-0.003);
        Origin["tetra"].RotatedZ(-0.004);

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w])
        {
            //Ref.Perspective.z.Delta(walkSpeed);
            Ref.Perspective.Forward(-walkSpeed);
            //Origin["tetra"].z.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            //Ref.Perspective.x.Delta(-walkSpeed);
            Ref.Perspective.Strafe(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s])
        {
            //Ref.Perspective.z.Delta(-walkSpeed);
            Ref.Perspective.Forward(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d])
        {
            //Ref.Perspective.x.Delta(walkSpeed);
            Ref.Perspective.Strafe(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_j])
        {
            Ref.Perspective.RotatedY(0.05);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.Perspective.RotatedY(-0.05);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
        {
            Ref.Perspective.y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Delta(-walkSpeed);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_x])
        {
            Origin["cube"].RotatedX(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_y])
        {
            Origin["cube"].RotatedY(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_z])
        {
            Origin["cube"].RotatedZ(0.01);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_UP])
        {
            Origin["cube"].y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_DOWN])
        {
            Origin["cube"].y.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LEFT])
        {
            Origin["cube"].x.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_RIGHT])
        {
            Origin["cube"].x.Delta(walkSpeed);
        }
        //Ref.Perspective.x.Set(10 * Math.Sin(Time / 10));
        //Ref.Perspective.y.Set(10 * Math.Cos(Time / 10));
        //Scribe.Info(Ref.Perspective.z);

        Origin["spring"] = new ParamMap(
            t => 120 * Math.Sin(t * 2+Math.Abs(Math.Sin(Time/10))),  // x
            t => t*10 + 120 * Math.Cos(t * (2+Math.Abs(Math.Sin(Time/120)*4))),  // y
            t => t * 35)                 // z
            .Plot(0, 0, 0, 0, 15 * Math.PI, 0.05, new RGBA(0x00ffff));
        Origin["spring"].Sub((m, i) => m.Colored(new HSLA(m.Normal * 2 * Math.PI + Time/4, 1, 1, 255)));
    }

    public override void PreLoop()
    {
        b = new Brush(new CustomMap(x => Events.MouseX), new CustomMap(y => Events.MouseY));
        Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        Origin["cube"] = Create.Cube(0, 0, 90, 10);

        Origin["2"] = Create.Cube(0, 0, 200, 50);
        Origin["tetra"] = Create.TriPyramid(-100, 0, 200, 16);

        Origin["spring"] = new ParamMap(
            t => 120 * Math.Sin(t * 2),  // x
            t => 120 * Math.Cos(t * 6),  // y
            t => t * 35)                 // z
            .Plot(0, 0, 0, 0, 30 * Math.PI, 0.05, new RGBA(0x00ffff));
        Origin["spring"].Sub((m, i) => m.Colored(new HSLA(m.Normal * 2 * Math.PI, 1, 1, 255)));

        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140).WithFlags(DrawMode.FILLED);

        Origin["cube"].Colored(new RGBA(0x6020ffff));
    }
}