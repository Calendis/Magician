namespace Magician.Demos.Tests;
using Magician.Geo;
using Magician.Interactive;
using Core.Caster;
using Magician.Alg;
using Magician.Alg.Symbols;
using static Magician.Alg.Notate;


public class EqPlotting : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    Equation plotTest3d;
    SolvedEquation spt3d;
    public override void Loop()
    {
        Paint.Renderer.Clear();

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w]){Ref.Perspective.Forward(-walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a]){Ref.Perspective.Strafe(-walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s]){Ref.Perspective.Forward(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d]){Ref.Perspective.Strafe(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_j]){Ref.Perspective.RotatedY(0.05);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l]){Ref.Perspective.RotatedY(-0.05);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE]){Ref.Perspective.y.Incr(walkSpeed);}
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT]){Ref.Perspective.y.Incr(-walkSpeed);}

        Var("time").Set(2.8 * Math.Sin(Time / 3)*Math.Sin(Time / 2) + 0.5);
        ((Implicit)Origin["pt3d"]).Refresh();

        //Scribe.Flush();
    }

    public override void PreLoop()
    {
        plotTest3d = new(
            new Fraction(
                Var("y"),
                Var("time")),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(1), Var("x")),
                    new Fraction(Var("z"), Val(1), Var("z"))
                )
            )
        );

        spt3d = plotTest3d.Solved(Var("y"));
        Var("time").Set(1);
        Oper o2 = Val(25).Minus(Var("x").Pow(Val(2))).Minus(Var("z").Pow(Val(2))).Root(Val(2));

        Origin["pt3d"] = new Implicit(spt3d, 0, 0, 0, 1, 200, 2, (-19, 20, 1), (-19, 20, 1)).Flagged(DrawMode.INNER);
        Origin["mySphere"] = new Implicit(o2, 500, 1250, 2000, 160, 160, 1, Sampling.Spiral, (-5, 5, 0.2), (-5, 5, 0.2)).Flagged(DrawMode.OUTER);
    }
}