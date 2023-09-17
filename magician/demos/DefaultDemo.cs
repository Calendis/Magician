using Magician.Geo;
using Magician.Interactive;
using Magician.Library;

namespace Magician.Demos;
public class DefaultSpell : Spell
{

    DirectMap? mo;
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
        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140).Flagged(DrawMode.INNER)
        .Driven(m => 0, th => 0+spin, ph => 0, CoordMode.POLAR, DriverMode.INCR, TargetMode.SUB)  // spins the star
        ;
        mo = Interactive.Sensor.MouseOver(Origin["my star"]);

        /* Testing area */
        Origin["btn"] = new Interactive.Button(-300, 250, 200, 180,
        () =>
        {
            Scribe.Info("Switching Spells...");
            Spellcaster.Prepare(new Demos.Tests.PlotView());
            Spellcaster.Cast();
        }

        );

        Origin["myMulti"] = new Multi().Flagged(DrawMode.INNER);
        Origin["savMyMulti"] = new Multi().Flagged(DrawMode.INVISIBLE);
        Origin["my star"].Heading = new Vec3(1, 1, -1).Normalized();

    }
    Brush b = new(
        new DirectMap(x => Events.MouseX),
        new DirectMap(x => Events.MouseY)
    );

    public override void Loop()
    {
        Renderer.RControl.Clear();
        Origin["btn"].Update();
        Origin["my star"].Update();
        Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

        Origin["myMulti"].AddFiltered(b.Paint(Events.Click ? 1 : 0, new Multi().Flagged(DrawMode.POINTS)));
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