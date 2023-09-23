using Magician.Geo;
using Magician.Interactive;
using Magician.Library;
using Magician.Symbols;
using static Magician.Symbols.Notate;

namespace Magician.Demos.Tests;

public class PlotView : Spell
{
    Brush? b;
    double walkSpeed = 4.0;
    Equation plotTest3d;
    public override void Loop()
    {
        Renderer.RControl.Clear();

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w])
        {
            Ref.Perspective.Forward(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            Ref.Perspective.Strafe(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s])
        {
            Ref.Perspective.Forward(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d])
        {
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

        //Equation plotTest3d = new(
        //    new Fraction(
        //        new SumDiff(Var("z"), Val(4), Val(1), Val(0), Val(1), Val(0.33), new SumDiff(Val(3)))
        //    ),
        //    Equation.Fulcrum.EQUALS,
        //    new SumDiff(
        //    new Fraction(
        //        new SumDiff(
        //            new Fraction(Var("x"), Val(230), Var("x")),
        //            new Fraction(Var("y"), Val(230), Var("y"))
        //        )
        //        ,new SumDiff(Val(1), new SumDiff(Val(1), Val(1)), new SumDiff(Val(2), new Fraction(Val(6))), new SumDiff(Val(3), Val(1)))
        //        ,Val(0.33*Math.Sin(Time/6))
        //    ),Var("z"))
        //);
        //plotTest3d

        //Equation plotTest3d = new(
        //    new SumDiff(
        //        Var("y")
        //        ,Var("x")
        //        ,Var("y")
        //    ),
        //    Equation.Fulcrum.EQUALS,
        //    new SumDiff(
        //        new Fraction(
        //            Var("x"),
        //            Val(69900000),
        //            Var("x"),
        //            Val(1),
        //            Var("z"),
        //            Val(1),
        //            Var("z")
        //        ),
        //        new Fraction(
        //            new SumDiff
        //            (
        //                new Fraction(Var("x"), Val(1000+500*Math.Sin(Time/1.5)), Var("x")),
        //                Val(Time),
        //                new Fraction(Var("z"), Val(1000+500*Math.Cos(Time/3)), Var("z"))
        //            ),
        //            Val(1)
        //        )
        //        
        //    )
        //);

        //Origin["pt3d"] = plotTest3d.Plot(
        //    (Var("y"), Equation.AxisSpecifier.Y, -500, 500, 20),
        //    (Var("x"), Equation.AxisSpecifier.X, -500, 500, 20),
        //    (Var("z"), Equation.AxisSpecifier.Z, -500, 500, 40)
        //);

    }

    public override void PreLoop()
    {

        //Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        //Origin["pt3d"].Colored(HSLA.RandomVisible());

        //plotTest3d = new(
        //    new SumDiff(
        //        new Fraction(Var("x"), Val(50), Var("y")),
        //        Val(0),
        //        new Fraction(Var("x"), Val(50), Var("x"), Val(1), Var("y")),
        //        Val(0),
        //        new Fraction(Var("z"), Val(2), Var("y")),
        //        Val(0),
        //        new Fraction(Var("z"), Val(40), Var("z"), Val(1), Var("y"))),
        //    Equation.Fulcrum.EQUALS,
        //    Val(100000)
        //);

        Equation plotTest3d = new(
            new SumDiff(new Fraction(
                Var("y"),
                Val(0.33)
            ), Val(0), Var("y")),
            Equation.Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );

        Origin["pt3d"] = plotTest3d.Plot(
            (Var("y"), Equation.AxisSpecifier.Y, 0, 0, 0),
            (Var("x"), Equation.AxisSpecifier.X, -500, 500, 20),
            (Var("z"), Equation.AxisSpecifier.Z, -500, 500, 40)
        );

        //Equation threePlane = new(
        //    Let("y"),
        //    Equation.Fulcrum.EQUALS,
        //    new SumDiff(Let("x"), N(3), new Fraction(Let("z"), N(0.5)))
        //);
        //Origin["planePlot"] = threePlane.Plot(
        //    (Let("y"), Equation.AxisSpecifier.Y, 0, 0, 0d),
        //    (Let("x"), Equation.AxisSpecifier.X, -600, 600, 50.1d),
        //    (Let("z"), Equation.AxisSpecifier.Z, -500, 100, 19d)
        //);
    }
}