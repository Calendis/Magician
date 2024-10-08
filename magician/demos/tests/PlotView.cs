namespace Magician.Demos.Tests;
using Magician.Geo;
using Magician.Interactive;
using Core.Caster;
using Magician.Alg;
using Magician.Alg.Symbols;
using static Magician.Alg.Notate;
using Paint;

public class EqPlotting : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    Equation plotTest3d;
    SolvedEquation spt3d;
    Node? selected;
    public override void Loop()
    {
        Renderer.Clear();


        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_W]) { Ref.Perspective.Forward(walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_A]) { Ref.Perspective.Strafe(-walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_S]) { Ref.Perspective.Forward(-walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_D]) { Ref.Perspective.Strafe(walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_SPACE]) { Ref.Perspective.Lift(walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT]) { Ref.Perspective.Lift(-walkSpeed); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_J]) { Ref.Perspective.RotatedY(0.02); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_L]) { Ref.Perspective.RotatedY(-0.02); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_K]) { Ref.Perspective.RotatedX(0.02); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_I]) { Ref.Perspective.RotatedX(-0.02); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_U]) { Ref.Perspective.RotatedZ(0.02); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_O]) { Ref.Perspective.RotatedZ(-0.02); }
        // shaders :]
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_1]) { Shaders.Swap(Shaders.Inverse); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_0]) { Shaders.Swap(Shaders.Default); }
        // spin the node around :]
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_Z]) { selected!.RotatedX(0.0085); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_X]) { selected!.RotatedY(0.0085); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_C]) { selected!.RotatedZ(0.0085); }
        // info
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_E]) { Scribe.Info($"\n{Origin["myrect"].range}\n{Origin["myrect2"].range}\n{Origin["tanglecube"].range}\n{Origin.range}\n{Origin["bg"][0].range}"); }
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_F]) {Scribe.Info(Origin["bg"][0]);}
        
        Origin["myrect2"].RotatedX(0.0085/6);
        Origin["myrect2"].RotatedY(0.0085/4);
        Origin["myrect2"].RotatedZ(0.0085/2);

        //Var("parameter").Set(0.5*Math.Sin(Time/2));
        //Origin["myPlot"].Update();
    }

    public override void PreLoop()
    {
        Oper sdfTanglecube = Var("x").Pow(Val(4)).Plus(Var("y").Pow(Val(4))).Plus(Var("z").Pow(Val(4))).Divide(Val(2)).Plus(Val(60)).Minus(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Mult(Val(8)));
        Implicit sdf = new(sdfTanglecube, 610, 0, -50, 75, 1, [0, 2, 1], (-5, 5, 0.6), (-5, 5, 0.6), (-5, 5, 0.6));
        sdf.Flagged(DrawMode.INNER);
        Origin["tanglecube"] = sdf;
        selected = Origin["tanglecube"];

        Origin["myrect"] = Create.Rect(-600, 200, 200, 150).Flagged(DrawMode.INNER);
        Origin["myrect2"] = Create.Star(-300, 200, 20, 150, 100).Colored(HSLA.RandomVisible()).Flagged(DrawMode.FULL | DrawMode.POINTS);
        //selected = Origin["myrect"];

        //Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        Oper plot = Var("x").Mult(Val(2)).Pow(Val(2)).Plus(Var("z").Mult(Val(3)).Pow(Val(2))).Mult(Var("parameter"));
        //Oper sphereRadiusFour = Val(16).Minus(Var("x").Pow(Val(2))).Minus(Var("z").Pow(Val(2))).Root(Val(2));
        //Var("parameter").Set(0);        
        //Origin["myPlot"] = new Implicit(
        //    plot, 0, 0, 1300, 10, 10, 2,
        //    (-2, 2, 0.2), (-2, 2, 0.2)
        //).Flagged(DrawMode.INNER);

        // torus
        //double innerRadius = 5;
        //double outerRadius = 4;
        //double radius = outerRadius + innerRadius;
        //Oper torus = Val(outerRadius).Pow(Val(2)).Minus(Val(innerRadius).Minus(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Root(Val(2))).Pow(Val(2))).Root(Val(2));
        // torus
        //Origin["Torus"] = new Explicit(
        //    torus, 500, 100, 500, 40, 40, 1,
        //    (-radius-1, radius+1, radius/10), (-radius-1, radius+1, radius/10)
        //).Flagged(DrawMode.OUTER);
        //Origin["myTorus"] = new Explicit(
        //    torus, 500, 100, 500, 40, 40, 1, Sampling.Spiral,
        //    (-radius, radius, radius / 20), (-radius, radius, radius / 20)
        //).Flagged(DrawMode.OUTER);

        Oper parabola = Var("x").Pow(Val(2));
        Node myGeo = new Explicit(parabola, 0, 0, 0, 1, 1, 0, (-50, 50, 5)).Flagged(DrawMode.PLOT);
        Origin["parabola"] = myGeo;

        //Implicit sdf = new(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Minus(Val(75)), 0, 0, 0, 80, 80, 2, new int[]{0,2,1}, (-20, 20, 1), (-20, 20, 1), (-20, 20, 1));

        // Approximate a tanglecube
        //Equation tanglecube = new(
        //    Var("x").Pow(Val(4)).Plus(Var("y").Pow(Val(4))).Plus(Var("z").Pow(Val(4))).Divide(Val(2)).Plus(Val(60)),
        //    Fulcrum.EQUALS,
        //    Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Mult(Val(8))
        //);
        //Core.Maps.IRelation tcs = tanglecube.Solved();
        //Origin["tanglecube"] = new Explicit(
        //    tcs, -500, 100, 500, 40, 40, 1,
        //    (-4, 4, 0.12), (-4, 4, 0.12)
        //).Flagged(DrawMode.POINTS);
    }
}