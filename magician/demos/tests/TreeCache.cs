namespace Magician.Demos.Tests;

using Magician.Core.Caster;
using Magician.Geo;
using Magician.Alg.Symbols;
using static Magician.Alg.Notate;

public class TreeCache : Spell
{
    public override void Loop()
    {       
        // camera controls
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_W]) { Ref.Perspective.Forward(2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_A]) { Ref.Perspective.Strafe(-2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_S]) { Ref.Perspective.Forward(-2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_D]) { Ref.Perspective.Strafe(2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_SPACE]) { Ref.Perspective.Lift(2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT]) { Ref.Perspective.Lift(-2.0); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_J]) { Ref.Perspective.RotatedY(0.015); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_L]) { Ref.Perspective.RotatedY(-0.015); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_K]) { Ref.Perspective.RotatedX(0.015); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_I]) { Ref.Perspective.RotatedX(-0.015); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_U]) { Ref.Perspective.RotatedZ(0.015); }
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_O]) { Ref.Perspective.RotatedZ(-0.015); }
        // info
        if (Interactive.Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_E]) { Scribe.Info($"\n\t{Origin["sq"][0].Title()}: {Origin["sq"][0].range}\n\t{Origin["sq"].Title()}: {Origin["sq"].range}\n\t{Origin["cube"].Title()}: {Origin["cube"].range}"); }
    }

    public override void PreLoop()
    {
        // Test capability to cache simple Nodes
        //Node myStar = Create.Star(5, 55, 118);
        //Origin["star"] = myStar;
        //Origin["star2"] = Create.Star(5, 55, 118).To(200, 0, 0).RotatedY(Math.PI/2);

        // Nested simple nodes
        Node mySquare = Create.Rect(10, 10, 140, 140).Colored(HSLA.RandomVisible()).Flagged(DrawMode.OUTER);
        Node myNestedSquare = Create.Rect(0, 0, 140/2, 140/2).Colored(HSLA.RandomVisible()).Translated(0,0,10).Flagged(DrawMode.INNER);
        myNestedSquare.To(mySquare[0].x.Get(), mySquare[0].y.Get(), mySquare[0].z.Get());
        mySquare[0] = myNestedSquare;
        mySquare[3].Tagged("last");
        Origin["sq"] = mySquare;
        Origin["sq"][0].Tagged("0");
        Origin["sq"][1].Tagged("1");
        Origin["sq"][2].Tagged("2");
        Origin["sq"][0][0].Tagged("nested0");
        Origin["sq"][0][1].Tagged("nested1");
        Origin["sq"][0][2].Tagged("nested2");
        Origin["sq"][0][3].Tagged("nested3");

        // More deeply nested nodes
        //Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();

        // Test capability to cache Nodes with faces
        Node myCube = Create.Cube(0, 0, -600, 180).RotatedX(0.1);
        Origin["cube"] = myCube.Flagged(DrawMode.OUTER);

        //Oper sphereRadiusFour = Val(16).Minus(Var("x").Pow(Val(2))).Minus(Var("z").Pow(Val(2))).Root(Val(2));
        //Oper plot = Var("x").Pow(Val(4)).Plus(Var("y").Pow(Val(4))).Plus(Var("z").Pow(Val(4))).Divide(Val(2)).Plus(Val(60)).Minus(Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Mult(Val(8)));
        //Origin["myPlot"] = new Implicit(
        //    plot, 0, 0, -300, 45, 1, new int[]{0,2,1},
        //    (-5, 5, 0.5), (-5, 5, 0.5), (-5, 5, 0.5)
        //).Flagged(DrawMode.INNER);
    }
}