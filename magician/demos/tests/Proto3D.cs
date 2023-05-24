using Magician.Geo;
using Magician.Interactive;
using Magician.Library;

namespace Magician.Demos.Tests;

public class Proto3D : Spell
{
    Brush? b;
    double walkSpeed = 0.3;
    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["2"].RotatedZ(-0.009);
        Origin["tetra"].RotatedX(-0.0025);
        Origin["tetra"].RotatedY(-0.003);
        Origin["tetra"].RotatedZ(-0.004);


        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w])
        {
            Ref.Perspective.z.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            Ref.Perspective.x.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s])
        {
            Ref.Perspective.z.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d])
        {
            Ref.Perspective.x.Delta(walkSpeed);
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

        // TODO: camera rotation
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_o])
        {
            Ref.Origin.RotatedX(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.Origin.RotatedY(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_k])
        {
            Ref.Origin.RotatedZ(0.01);
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
    }

    public override void PreLoop()
    {
        b = new Brush(new CustomMap(x => Events.MouseX), new CustomMap(y => Events.MouseY));
        Origin["cube"] = Create.Cube(0, 0, 90, 10);

        Origin["2"] = Create.Cube(0, 0, 200, 16);
        Origin["tetra"] = Create.TriPyramid(-100, 0, 200, 16);
    }
}