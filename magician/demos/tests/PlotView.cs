using Magician.Geo;
using Magician.Interactive;
using Magician.Library;
using Magician.Symbols;
using static Magician.Symbols.Algebra;

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
            Ref.Perspective.RotatedY(-0.05);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.Perspective.RotatedY(0.05);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
        {
            Ref.Perspective.y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Delta(-walkSpeed);
        }
    }

    public override void PreLoop()
    {

        Equation plotTest3d = new(
            Let("y"),
            Equation.Fulcrum.EQUALS,
            new Fraction(new Fraction(N(100), N(1), Let("z")), Let("x"))
        );
        Origin["pt3d"] = plotTest3d.Plot(
            (Let("y"), Equation.AxisSpecifier.Y, 0,0,0),
            (Let("x"), Equation.AxisSpecifier.X, -600, 600, 60),
            (Let("z"), Equation.AxisSpecifier.Z, -610, 600, 20d)
        );
        
        //Origin["pt3d"].Colored(HSLA.RandomVisible());

        //Equation plotTest2d = new(
        //    Let("y"),
        //    Equation.Fulcrum.EQUALS,
        //    new Fraction(N(1000), Let("x"))
        //);
        //Origin["pt2d"] = plotTest2d.Plot(
        //    (Let("y"), Equation.AxisSpecifier.Y, -600, 600, 10d),
        //    (Let("x"), Equation.AxisSpecifier.X, -600, 600, 10d)
        //);
        
        //Origin["pt2d"].Colored(HSLA.RandomVisible());

    }
}