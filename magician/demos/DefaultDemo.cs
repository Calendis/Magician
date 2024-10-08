namespace Magician.Demos;
using Core;
using Core.Maps;
using Core.Caster;
using Geo;
using Interactive;

public class DefaultSpell : Spell
{

    Direct? mo;
    double spin = 0.014;
    public override void PreLoop()
    {
        // bg
        Origin["bg"] = new UI.RuledAxes(100, 10, 100, 10).Render();

        // Hexagonal grid
        Origin["hex grid"] = new Geo.Tiles.Hexagonal(7, 7).Render(45).To(300, 0);

        /* Multi-line text */
        Origin["paragraph1"] = new UI.RichParagraph(0, 0, HSLA.RandomVisible(), 16, UI.Justification.CENTRE,
            $"{UI.TFS.RGB(255, 0, 0)}Rich paragraph{UI.TFS.Back} now supports",
            $"{UI.TFS.RGB(255, 128, 0)}HTML-style{UI.TFS.RGB(255, 255, 0)} nesting,",
            $"{UI.TFS.Back}because {UI.TFS.Back}why{new UI.TextFormatSetting(HSLA.RandomVisible(), 12)} not{UI.TFS.Back}?",
            "Also, text can now be justified", "to left, right, or centre"
        );

        Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140).Flagged(DrawMode.INNER)
        //.Driven(m => 0, th => 0+spin/4, ph => 0, CoordMode.POLAR, DriverMode.INCR, TargetMode.DIRECT)  // spins the star
        ;
        mo = new Interactive.Sensor.MouseOver(Origin["my star"]);

        /* Testing area */
        Origin["btn"] = new Interactive.Button(-300, 250, 200, 180,
        () =>
        {
            Scribe.Info("Switching Spells...");
            Spellbook.Prepare(new Demos.Tests.EqPlotting());
            Spellbook.Cast();
        }

        );

        Origin["myMulti"] = new Node().Flagged(DrawMode.INNER);
        Origin["savMyMulti"] = new Node().Flagged(DrawMode.INVISIBLE);
    }
    Brush b = new(
        new Direct(x => Events.MouseX),
        new Direct(x => Events.MouseY)
    );

    public override void Loop()
    {
        if (Events.scans[SDL2.SDL.SDL_Scancode.SDL_SCANCODE_E]) { Scribe.Info($"\n{Origin["my star"].range}\n{Origin["hex grid"][0].range}\n{Origin["bg"][0].range}"); }
        
        Paint.Renderer.Clear();
        Origin["btn"].Update();
        //Origin["my star"].Update();
        Origin["my star"].RotatedZ(0.002);
        Origin["my star"].Colored(new RGBA(0, mo!.Evaluate().Get()*255, 255, 255));

        Origin["myMulti"].AddFiltered(b.Paint(Events.Click ? 1 : 0, new Node().Flagged(DrawMode.POINTS)));
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
            Ref.Perspective.z.Incr(Events.keys[SDL2.SDL.SDL_Keycode.SDLK_LSHIFT] ? -spin * 100 : spin * 100);
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
                Node cp = Origin["myMulti"].Copy().Colored(HSLA.RandomVisible());
                Origin[$"savMyMulti"].Add(cp);
                Origin["savMyMulti"].MODIFY(true);
                Origin["myMulti"].Clear();
            }
        }
    }
}