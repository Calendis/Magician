namespace Magician.Demos.Tests;
using Core.Maps;
using Geo;
using Interactive;
using Core.Caster;
using Algebra.Symbols;
using Algebra;
using static Magician.Algebra.Notate;

public class MapPlotting : Spell
{
    double walkSpeed = 4;
    public override void PreLoop()
    {
        Origin["plot1"] = new ParamMap(t => 60 * Math.Sin(t), t => 60 * Math.Cos(t / 5))
            .Plot(new(0, 20 * Math.PI, 0.5), HSLA.RandomVisible(), -300);

        Origin["plot2"] = new ParamMap(t => 60 * Math.Sin(t / 4), t => t+150)
            .Plot(new(0, 20 * Math.PI, 0.5), HSLA.RandomVisible(), 10);
        
        Equation twoDEqZ = new(
            Var("y"),
            Fulcrum.EQUALS,
            new Fraction(Var("z"), Val(60), Var("z"))
        );
        Equation twoDEqX = new(
            Var("y"),
            Fulcrum.EQUALS,
            Var("x")
        );
        Origin["paraZ"] = twoDEqZ.Solved().Plot(AxisSpecifier.Y,
            (Var("z"),new (AxisSpecifier.Z, new(-80, 80, 4)))
        );
        Origin["linX"] = twoDEqX.Solved().Plot(AxisSpecifier.Y,
            (Var("x"), new (AxisSpecifier.X, new(-80, 80, 4)))
        );

        InverseParamMap ipm = new(new Func<double[], double[]>(vars => new double[]{-600000d / (vars[0] * vars[0] + vars[1] * vars[1])}), 2);
        Origin["plot3"] = ipm.Plot(
            new PlotOptions(AxisSpecifier.X, new(-250, 250, 20)),
            new(AxisSpecifier.Z, new(-250, 250, 20))
        ).To(0, 0, 600);

        Equation ipmCompare = new(
            Var("y"),
            Fulcrum.EQUALS,
            new Fraction(Val(-600000),
                new SumDiff(
                    new Fraction(new SumDiff(Var("x"), Val(600)), Val(1), new SumDiff(Var("x"), Val(600))),
                    Val(0),
                    new Fraction(new SumDiff(Var("z"), Val(600)), Val(1), new SumDiff(Var("z"), Val(600)))
                )
            )
        );

        SolvedEquation s = ipmCompare.Solved();
        Origin["plot4b"] = s.Plot(AxisSpecifier.Y,
            (Var("x"), new(AxisSpecifier.X, new(-250+600, 250+600, 20))),
            (Var("z"), new(AxisSpecifier.Z, new(-250+600, 250+600, 20)))
        );
    }

    public override void Loop()
    {
        DoControls();
    }

    public void DoControls()
    {
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
            Ref.Perspective.y.Incr(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Incr(-walkSpeed);
        }
    }
}