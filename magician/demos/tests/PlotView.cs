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

        Equation plotTest3d = new(
            new Fraction(
                Var("y"),
                Val(0.33*Math.Sin(Time/6))
            ),
            Equation.Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );

        //Equation plotTest3d = new(
        //    new SumDiff(
        //        new Fraction(Val(100),Var("x")),
        //        new Fraction(Val(100),Var("y"))
        //    ),
        //    Equation.Fulcrum.EQUALS,
        //    new Fraction(
        //        Val(5),
        //        Var("z")
        //    )
        //);

        Origin["pt3d"] = plotTest3d.Plot(
            (Var("y"), Equation.AxisSpecifier.Y, 0, 0, 0),
            (Var("x"), Equation.AxisSpecifier.X, -500, 500, 20),
            (Var("z"), Equation.AxisSpecifier.Z, -500, 500, 40)
        );

    }

    public override void PreLoop()
    {

        //Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();
        //Origin["pt3d"].Colored(HSLA.RandomVisible());

        Equation plotTest3d = new(
        new SumDiff(
            Var("x"),
            Var("y"),
            new SumDiff(
                Var("y"),
                new SumDiff(
                    Val(10),
                    Var("z")),
                    new SumDiff(
                        Val(3),
                        Var("y")
                    )
                )
            ),
            Equation.Fulcrum.EQUALS,
            new Fraction(
                Val(10000),
                Var("x")
            )
        );        
        
        /* Origin["pt3d"] = plotTest3d.Plot(
            (Var("y"), Equation.AxisSpecifier.Y, 0, 0, 0),
            (Var("x"), Equation.AxisSpecifier.X, -500, 500, 20),
            (Var("z"), Equation.AxisSpecifier.Z, -500, 500, 40)
        ); */

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