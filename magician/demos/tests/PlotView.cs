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

        //Var("parameter").Set(0.5*Math.Sin(Time/2));
        //Origin["myPlot"].Update();

        //Scribe.Flush();
    }

    public override void PreLoop()
    {
        //Oper plot = Var("x").Mult(Val(2)).Pow(Val(2)).Plus(Var("z").Mult(Val(3)).Pow(Val(2))).Mult(Var("parameter"));
        //Oper sphereRadiusFour = Val(16).Minus(Var("x").Pow(Val(2))).Minus(Var("z").Pow(Val(2))).Root(Val(2));
        //Var("parameter").Set(0);        
        //Origin["myPlot"] = new Implicit(
        //    plot, 0, 0, 1300, 10, 10, 2,
        //    (-2, 2, 0.2), (-2, 2, 0.2)
        //).Flagged(DrawMode.INNER);

        double innerRadius = 6;
        double outerRadius = 4;
        double radius = outerRadius + innerRadius;
        Oper torus = Val(outerRadius).Pow(Val(2)).Minus(Val(innerRadius).Minus(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Root(Val(2))).Pow(Val(2))).Root(Val(2));
        Origin["mySphere"] = new Implicit(
            torus, 500, 100, 500, 40, 40, 1,
            Sampling.Spiral,
            (-radius, radius, radius/20), (-radius, radius, radius/20)
        ).Flagged(DrawMode.OUTER);
        
        Origin["mySphere2"] = new Implicit(
            torus, 1500, 100, 500, 40, 40, 1,
            (-radius, radius, radius/20), (-radius, radius, radius/20)
        ).Flagged(DrawMode.OUTER);
    }
}