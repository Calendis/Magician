using Magician.Geo;
using Magician.Interactive;
using Magician.Library;
using Magician.Algo;
using static Magician.Algo.Algebra;

namespace Magician.Demos;
public class DefaultSpell : Spell
{

    IMap? mo;
    double spin = 0.014;
    public override void PreLoop()
    {
        // bg
        Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();

        // Hexagonal grid
        Origin["hex grid"] = new Symbols.Hexagonal(7, 7).Render(45).Positioned(300, 0);

        /* Multi-line text */
        Origin["paragraph1"] = new UI.RichParagraph(0, 0, HSLA.RandomVisible(), 16, UI.Justification.CENTRE,

            $"{UI.TFS.RGB(255, 0, 0)}Rich paragraph{UI.TFS.Back} now supports",
            $"{UI.TFS.RGB(255, 128, 0)}HTML-style{UI.TFS.RGB(255, 255, 0)} nesting,",
            $"{UI.TFS.Back}because {UI.TFS.Back}why{new UI.TextFormatSetting(HSLA.RandomVisible(), 12)} not{UI.TFS.Back}?",
            "Also, text can now be justified", "to left, right, or centre"
        );

        // Non-square mouseover
        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140).WithFlags(DrawMode.FILLED);
        mo = Interactive.Sensor.MouseOver(Origin["my star"]);

        /* Testing area */

        Origin["btn"] = new Interactive.Button(-300, 250, 200, 180,
        () =>
        {
            Spellcaster.Cache(new Plots());
            Scribe.Info("Switching Spells...");
        }

        );
        //Origin["testRect"] = Geo.Create.Rect(-150, 100, 300, 250).Colored(new RGBA(0x70ff00d0));
        Origin["myMulti"] = new Multi().WithFlags(DrawMode.FILLED);
        Origin["savMyMulti"] = new Multi().WithFlags(DrawMode.INVISIBLE);

    }
    Brush b = new Brush(
        new CustomMap(x => Events.MouseX),
        new CustomMap(x => Events.MouseY)
    );

    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["my star"].RotatedZ(0.01);
        Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

        // Perform a Matrix rotation
        //Matrix mmx = new Matrix(Origin["btn"]);
        //Matrix result = Matrix.Rotation(0.004).Mult(mmx);
        //Origin["btn"].Positioned(result);
        //Origin["testRect"].RotatedX(0.04);
        //Origin["testRect"].RotatedY(0.02);
        //Origin["btn"].RotatedZ(0.01);
        Origin["myMulti"].AddCautiously(b.Paint(Events.Click ? 1 : 0, new Multi().WithFlags(DrawMode.POINTS)));
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_z])
        {
            Origin["savMyMulti"].RotatedZ(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_y])
        {
            Origin["savMyMulti"].RotatedY(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_x])
        {
            Origin["savMyMulti"].RotatedX(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            Origin["myMulti"].RotatedZ(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_b])
        {
            Origin["myMulti"].RotatedY(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_c])
        {
            Origin["myMulti"].RotatedX(0.03);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_UP])
        {
            Origin["myMulti"].Translated(0, 0.3, 0);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_DOWN])
        {
            Origin["myMulti"].Translated(0, -0.3, 0);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LEFT])
        {
            Origin["myMulti"].Translated(-0.3, 0, 0);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_RIGHT])
        {
            Origin["myMulti"].Translated(0.3, 0, 0);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_k])
        {
            Ref.Perspective.z.Delta(Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT] ? -spin * 100 : spin * 100);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_o])
        {
            Ref.FOV++;
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.FOV--;
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
        {
            if (Origin["myMulti"].Count >= 3)
            {
                //Origin["myMulti"].Written(Origin["myMulti"].Evaluate()+1);
                Origin[$"savMyMulti"].Add(Origin["myMulti"].Copy().Colored(HSLA.RandomVisible()));
                Origin["myMulti"].Clear();
            }
        }
    }
}

public class WavingText : Spell
{
    public override void PreLoop()
    {
        Renderer.RControl.Clear();
    }

    // For stuff that needs to redefined every frame
    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["parametric"] = new IOMap(1,
            x => 180 * Math.Cos(x / 3) + 10 * Math.Sin(Time / 2),
            y => 180 * Math.Sin(y / 7 + Time)
        ).TextAlong(-49, 49, 0.3, "Here's an example of a Multimap with 1 input and two outputs being used to draw text parametrically"
        , new RGBA(0x00ff9080));
    }
}

public class TestingSpell : Spell
{
    Brush? b;
    double walkSpeed = 0.3;

    public override void PreLoop()
    {
        b = new Brush(new CustomMap(x => Events.MouseX), new CustomMap(y => Events.MouseY));
        Origin["cube"] = Create.Cube(0, 0, 90, 10);

        Origin["2"] = Create.Cube(0, 0, 200, 16);
        Origin["tetra"] = Create.TriPyramid(-100, 0, 200, 16);
    }

    public override unsafe void Loop()
    {
        Renderer.RControl.Clear();
        Origin["2"].RotatedZ(-0.009);
        Origin["tetra"].RotatedX(-0.0025);
        Origin["tetra"].RotatedY(-0.003);
        Origin["tetra"].RotatedZ(-0.004);
        

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_w])
        {
            Ref.Perspective.z.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_a])
        {
            Ref.Perspective.x.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_s])
        {
            Ref.Perspective.z.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_d])
        {
            Ref.Perspective.x.Delta(walkSpeed);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
        {
            Ref.Perspective.y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT])
        {
            Ref.Perspective.y.Delta(-walkSpeed);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_x])
        {
            Origin["cube"].RotatedX(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_y])
        {
            Origin["cube"].RotatedY(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_z])
        {
            Origin["cube"].RotatedZ(0.01);
        }

        // TODO: camera rotation
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_o])
        {
            Ref.Origin.RotatedX(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_l])
        {
            Ref.Origin.RotatedY(0.01);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_k])
        {
            Ref.Origin.RotatedZ(0.01);
        }

        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_UP])
        {
            Origin["cube"].y.Delta(walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_DOWN])
        {
            Origin["cube"].y.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LEFT])
        {
            Origin["cube"].x.Delta(-walkSpeed);
        }
        if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_RIGHT])
        {
            Origin["cube"].x.Delta(walkSpeed);
        }
    }
}

public class Spinner10K : Spell
{
    public override void PreLoop()
    {
        Origin["sqs"] = (new IOMap(1, x => x % 1000, y => 20 * Math.Floor(y / 1000))
        .MultisAlong(0, 20000, 20, Create.RegularPolygon(4, 10))
        );
        Origin["sqs"].Sub(d => d.Sub(m => m
            .DrivenPM(p => p + 0.1, m => m)
        )).Positioned(-500, -300);
    }

    public override void Loop()
    {
        Renderer.RControl.Clear();
        //Scribe.Info(Origin);
    }
}

public class Plots : Spell
{
    public override void PreLoop()
    {
        Origin["plot0"] = ((IMap)new CustomMap(x => 60 * Math.Sin(x/6))).Plot(0, 0, 0, 96*Math.PI, 1, HSLA.RandomVisible());
        Origin["plot1"] = new IOMap(1, t => 60*Math.Sin(t), t => 60*Math.Cos(t/5)).Plot(-300, -300, 0, 96*Math.PI, 1, HSLA.RandomVisible());
        //Origin["plot2"] = new IOMap(2, (x, y) => x + Math.Sin(y)).Plot();
        //Origin["plot2"] = new IOMap(2, x => x + Math.Sin(x)).Plot();

        // Algebra testing
        Oper hyperbola = new Mult(Let("x"), Let("y"));
        Oper scale = N(1);
        Fulcrum equalsSign = Fulcrum.EQUALS;
        Equation e = new(hyperbola, equalsSign, scale);
    }

    public override void Loop()
    {
        //
    }
}