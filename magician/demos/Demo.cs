using Magician.Geo;
using Magician.Interactive;
using Magician.Library;

namespace Magician.Demos;
public class DefaultSpell : Spell
{

    DirectMap? mo;
    double spin = 0.014;
    Driver rotator;
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
            Spellcaster.Cache(new Demos.Tests.Proto3D());
            Scribe.Info("Switching Spells...");
        }

        );
        //Origin["testRect"] = Geo.Create.Rect(-150, 100, 300, 250).Colored(new RGBA(0x70ff00d0));
        Origin["myMulti"] = new Multi().WithFlags(DrawMode.FILLED);
        Origin["savMyMulti"] = new Multi().WithFlags(DrawMode.INVISIBLE);

        Scribe.Info($"Star heading: {Origin["my star"].Heading}");
        Origin["my star"].Heading = new Vec3(1, 1, -1).Normalized();
        Scribe.Info($"Star heading: {Origin["my star"].Heading}");

        Origin["my star"].PhaseXY = 0;

        // Spin the star
        ParamMap pm = new(x => 0, y => 0.02, z => 0);
        rotator = new Driver(Origin["my star"], pm, CoordMode.POLAR, DriverMode.INCR, TargetMode.DIRECT);

    }
    Brush b = new Brush(
        new DirectMap(x => Events.MouseX),
        new DirectMap(x => Events.MouseY)
    );

    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["btn"].Update();
        //Origin["my star"].Forward(1);
        Origin["my star"].RotatedZ(0.02);
        Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));
        rotator.Drive(Time);

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
                Origin[$"savMyMulti"].Add(Origin["myMulti"].Copy().Colored(HSLA.RandomVisible()));
                Origin["myMulti"].Clear();
            }
        }
    }
}