namespace Magician.Demos.Tests;
using Core.Maps;
using Geo;
using Interactive;
using Core.Caster;
using Alg.Symbols;
using Alg;
using static Magician.Alg.Notate;

public class MapPlotting : Spell
{
    double walkSpeed = 4;
    public override void PreLoop()
    {
        Origin["plot1"] = new Implicit(new Parametric(t => 60 * Math.Sin(t), t => 60 * Math.Cos(t / 5)), 0, 0, -200, 20, 5, (-10d, 10d, 0.2)).Flagged(DrawMode.PLOT);
        Origin["plot2"] = new Implicit(new Parametric(t => 60 * Math.Sin(t / 4), t => t+150), 0, 0, 0, 20, 5, (0d, 20*Math.PI, 0.5)).Flagged(DrawMode.PLOT);
            
        
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
        // TODO: add an api for switching the axis
        Origin["paraZ"] = new Implicit(twoDEqZ.Solved(), 0, 0, 200, 3, 3, (-80, 80, 4)).Flagged(DrawMode.PLOT).Colored(new HSLA(0,1,1,255));
        //Origin["linX"] = new Implicit(twoDEqX.Solved(), 0, 9, 400, 1, 1, (-80, 80, 4)).Flagged(DrawMode.PLOT);
        

        //Relational ipm = new(new Func<double[], double[]>(vars => new double[]{-600000d / (vars[0] * vars[0] + vars[1] * vars[1])}), 2);
        //Origin["plot3"] = new Implicit(ipm, 0, 0, 600, 2, 2, (-250, 250, 20), (-250, 250, 20));
        //Origin["plot4"] = new Implicit(new Fraction(Val(-600000),
        //        new SumDiff(
        //            Var("x").Pow(Val(2)),
        //            Val(0),
        //            Var("z").Pow(Val(2))
        //        )
        //), 0, 0, 1800, 2, 2, (-250, 250, 20), (-250, 250, 20));
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