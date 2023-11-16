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
    SolvedEquation spt3d;
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

        Var("time").Val = 0.2 * Math.Sin(Time / 3) + 0.1;
        Origin["pt3d"] = spt3d.Plot(AxisSpecifier.Y,
            (Var("x"), new(AxisSpecifier.X, new(-500, 500, 20))),
            (Var("z"), new(AxisSpecifier.Z, new(-500, 500, 40)))
        );
        // Optionally, reset the variable to an unknown state
        Var("time").Reset();
        
        //Origin["pt3d"] = spt3d.Plot(AxisSpecifier.Y,
        //    new PlotOptions(AxisSpecifier.X, new(-500, 500, 20)),
        //    new(AxisSpecifier.Z, new(-500, 500, 40))
        //);
    }

    public override void PreLoop()
    {
        Equation plotTest3d = new(
            new Fraction(
                Var("y"),
                Var("time")),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );
        spt3d = plotTest3d.Solved();
    }
}