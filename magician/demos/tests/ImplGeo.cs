namespace Magician.Demos.Tests;
using Core.Caster;
using static Alg.Notate;
using Alg.Symbols;
using Magician.Geo;
using Magician.Alg;
using Magician.Interactive;

public class ImplicitGeom : Spell
{
    double walkSpeed = 3.3;
    public override void PreLoop()
    {
        Scribe.Info("Generalized expression rendering...");

        // Expressions we want to render
        //Oper o1 = Var("x").Plus(Var("z").Pow(Val(2))).Root(Val(2));
        Oper o2 = Val(25).Minus(Var("x").Pow(Val(2))).Minus(Var("z").Pow(Val(2))).Root(Val(2));
        // Create geometry from expressions
        //Geo.Implicit geo1 = new(o1, -100, -200, 0, 30, 30, (-7, 7, 0.5), (-7, 7, 0.5));
        Geo.Explicit geo2 = new(o2, -900, 0, 600, 160, 160, 1, Sampling.Spiral, (-5, 5, 0.2), (-5, 5, 0.2));
        Geo.Explicit geo3 = new(o2, 900, 0, 600, 160, 160, 1, (-4.8, 5, 0.2), (-4.8, 5, 0.2));
        // Add our geometry to the scene
        //Origin["myGeo1"] = geo1.Flagged(DrawMode.OUTER);
        Origin["myGeo2"] = geo2.Flagged(DrawMode.OUTER);
        Origin["myGeo3"] = geo3.Flagged(DrawMode.OUTER);

    }
    public override void Loop()
    {
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w]){Ref.Perspective.Forward(-walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a]){Ref.Perspective.Strafe(-walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s]){Ref.Perspective.Forward(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d]){Ref.Perspective.Strafe(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_j]){Ref.Perspective.RotatedY(0.05);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l]){Ref.Perspective.RotatedY(-0.05);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE]){Ref.Perspective.y.Incr(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT]){Ref.Perspective.y.Incr(-walkSpeed);}
        //Scribe.Flush();
    }
}