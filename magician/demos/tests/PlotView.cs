using Magician.Geo;
using Magician.Interactive;
using Magician.Library;
using Magician.Symbols;
using static Magician.Symbols.Notate;

namespace Magician.Demos.Tests;

public class EqPlotting : Spell
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
        //    new SumDiff(new Fraction(
        //        Var("y"),
        //        Val(Math.Sin(Time / 3) / 6)
        //        ),
        //        Val(0), Var("y"), Val(0), new Fraction(Var("y"), Val(1), Val(0.2))),
        //    Fulcrum.EQUALS,
        //    new Fraction(
        //        new SumDiff(
        //            new Fraction(Var("x"), Val(230), Var("x")),
        //            new Fraction(Var("z"), Val(230), Var("z"))
        //        )
        //    )
        //);
        //
        //SolvedEquation spt3d = plotTest3d.Solved();
        //Origin["pt3d"] = spt3d.Plot(AxisSpecifier.Y,
        //    new(AxisSpecifier.X, new(-500, 500, 20)),
        //    new(AxisSpecifier.Z, new(-500, 500, 40))
        //);

    }

    public override void PreLoop()
    {
        Equation plotTest3d = new(
            new SumDiff(new Fraction(
                Var("y"),
                Val(0.3)
                ),
                Val(0), Var("y"), Val(0), new Fraction(Var("y"), Val(1), Val(0.2))),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );

        SolvedEquation spt3d = plotTest3d.Solved();
        Origin["pt3d"] = spt3d.Plot(AxisSpecifier.Y,
            new(AxisSpecifier.X, new(-500, 500, 20)),
            new(AxisSpecifier.Z, new(-500, 500, 40))
        );

        /* Extended testing */
        Equation unsolved = new(
            new SumDiff(
                new List<Oper>{
                    Var("x"),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("y")),
                    Var("x"),
                    new Fraction(Val(3.3), Val(1), Var("z")),
                    new Fraction(Var("x"), Val(1), Var("y")),
                    new Fraction(Var("x"), Val(1), Var("z")),
                    new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
                },
                new List<Oper> { }
            ),
            Fulcrum.EQUALS,
            new Fraction(Val(2), Val(1), Var("y"), Val(1), Var("x"), Val(1), Var("z"))
        );
        //Scribe.Info(unsolved.RHS.Ord());
        SolvedEquation s = unsolved.Solved();
    }
}