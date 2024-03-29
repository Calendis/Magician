namespace Magician.Demos.Tests;
using Core.Maps;
using Core.Caster;
using Geo;
using Interactive;


public class Proto3D : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    public override void Loop()
    {
        Paint.Renderer.Clear();
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
        // CCW is positive
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
            Ref.Perspective.y.Incr(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Incr(-walkSpeed);
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
            Origin["cube"].y.Incr(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_DOWN])
        {
            Origin["cube"].y.Incr(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LEFT])
        {
            Origin["cube"].x.Incr(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_RIGHT])
        {
            Origin["cube"].x.Incr(walkSpeed);
        }
    }

    public override void PreLoop()
    {
        b = new Brush(new Direct(x => Events.MouseX), new Direct(y => Events.MouseY));
        Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        Origin["cube"] = Create.Cube(200, 100, -200, 10);
        Origin["cube"].Colored(new RGBA(0x6020ffff));
        Origin["2"] = Create.Cube(0, -270, 340, 100);
        Origin["tetra"] = Create.TriPyramid(-100, 0, 200, 16);

        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 8, 40, 140).Flagged(DrawMode.FULL);
        ///Origin["my star"].A(120);
        


    }
}