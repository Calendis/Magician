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

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w]) { Ref.Perspective.Forward(-walkSpeed); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a]) { Ref.Perspective.Strafe(-walkSpeed); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s]) { Ref.Perspective.Forward(walkSpeed); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d]) { Ref.Perspective.Strafe(walkSpeed); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_j]) { Ref.Perspective.RotatedY(0.05); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l]) { Ref.Perspective.RotatedY(-0.05); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE]) { Ref.Perspective.y.Incr(walkSpeed); }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT]) { Ref.Perspective.y.Incr(-walkSpeed); }

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

        double innerRadius = 5;
        double outerRadius = 4;
        double radius = outerRadius + innerRadius;
        Oper torus = Val(outerRadius).Pow(Val(2)).Minus(Val(innerRadius).Minus(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Root(Val(2))).Pow(Val(2))).Root(Val(2));

        Origin["mySphere"] = new Implicit(
            torus, 500, 100, 500, 40, 40, 2,
            Sampling.Spiral,
            (-radius, radius, radius / 20), (-radius, radius, radius / 20)
        ).Flagged(DrawMode.OUTER);

        //Origin["mySphere2"] = new Implicit(
        //    torus, 1500, 100, 500, 40, 40, 1,
        //    (-radius, radius, radius / 20), (-radius, radius, radius / 20)
        //).Flagged(DrawMode.OUTER);

        // Approximate a tanglecube
        Equation tanglecube = new(
            Var("x").Pow(Val(4)).Plus(Var("y").Pow(Val(4))).Plus(Var("z").Pow(Val(4))).Divide(Val(2)).Plus(Val(60)),
            Fulcrum.EQUALS,
            Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Mult(Val(8))
        );
        Core.Maps.IRelation tcs = tanglecube.Solved();
        Origin["tanglecube"] = new Implicit(
            tcs, -500, 100, 500, 40, 40, 1,
            (-4, 4, 0.2), (-4, 4, 0.2)
        ).Flagged(DrawMode.POINTS);
        
        // Approximate a horn
        //Equation horn = new(
        //    Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("x").Pow(Val(2))).Minus(Val(2)).Pow(Val(3)).Minus(Var("x").Pow(Val(2)).Mult(Var("z").Pow(Val(3)))).Minus(Var("y").Pow(Val(2)).Mult(Var("z").Pow(Val(3)))),
        //    Fulcrum.EQUALS,
        //    Val(0)
        //);
        //Core.Maps.IRelation hornRel = horn.Solved();
        //Origin["horn"] = new Implicit(
        //    hornRel, -500, 100, 500, 40, 40, 1,
        //    (-4.5, 4.5, 0.125), (-4.5, 4.5, 0.125)
        //).Flagged(DrawMode.POINTS);
        
        // Approximate a simple implicit relation
        //Equation imp = new(
        //        Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Minus(Val(4).Mult(Var("x")).Mult(Var("y")).Mult(Var("z"))).Plus(Val(25)),
        //        Fulcrum.EQUALS,
        //        Val(0)
        //);
        //Core.Maps.IRelation impmap = imp.Solved();
        //Origin["imp"] = new Implicit(
        //    impmap, -500, 100, 500, 140, 40, 1,
        //    (-5, 5, 0.75/10), (-5, 5, 0.75/10)
        //).Flagged(DrawMode.POINTS);
    }
}