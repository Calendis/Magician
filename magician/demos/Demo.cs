using Magician.Geo;
using Magician.Interactive;
using Magician.Library;

namespace Magician.Demos
{
    public class DefaultSpell : Spell
    {

        IMap? mo;
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
            Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140);
            mo = Interactive.Sensor.MouseOver(Origin["my star"]);

            /* Testing area */

            Origin["btn"] = new Interactive.Button(-300, 250, 200, 180,
            () =>
            {
                Spellbook.Cache(new TestingSpell());
            }

            );
            //Origin["testRect"] = Geo.Create.Rect(-150, 100, 300, 250).Colored(new RGBA(0x70ff00d0));
            Origin["myMulti"] = new Multi().DrawFlags(DrawMode.INNER);
            Origin["savMyMulti"] = new Multi().DrawFlags(DrawMode.INVISIBLE);

        }
        Brush b = new Brush(
            new CustomMap(x => Events.MouseX),
            new CustomMap(x => Events.MouseY)
        );

        public override void Loop()
        {
            Renderer.Control.Clear();
            Origin["my star"].RotatedZ(0.01);
            Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

            // Perform a Matrix rotation
            //Matrix mmx = new Matrix(Origin["btn"]);
            //Matrix result = Matrix.Rotation(0.004).Mult(mmx);
            //Origin["btn"].Positioned(result);
            //Origin["testRect"].RotatedX(0.04);
            //Origin["testRect"].RotatedY(0.02);
            //Origin["btn"].RotatedZ(0.01);
            Origin["myMulti"].AddCautiously(b.Paint(Events.Click ? 1 : 0, new Multi().DrawFlags(DrawMode.POINT)));
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


            if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_SPACE])
            {
                if (Origin["myMulti"].Count >= 4)
                {
                    //Origin["myMulti"].Written(Origin["myMulti"].Evaluate()+1);
                    Origin[$"savMyMulti"].Add(Origin["myMulti"].Copied().Colored(HSLA.RandomVisible()));
                    Origin["myMulti"].Clear();
                }
            }
        }
    }

    public class WavingText : Spell
    {
        public override void PreLoop()
        {
            Renderer.Control.Clear();
        }

        // For stuff that needs to redefined every frame
        public override void Loop()
        {
            Renderer.Control.Clear();
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

        public override void PreLoop()
        {
            b = new Brush(new CustomMap(x => Events.MouseX), new CustomMap(y => Events.MouseY));
            Origin["cube"] = Create.Cube(0, 0, 0, 140)
            .DrivenXY(
                x=>x,
                y=> y+Math.Sin(Time/10)
            )
            ;
            //Origin["cube"] = Create.ImpractiCube(0, 0, 0, 250);
        }

        public override void Loop()
        {
            Renderer.Control.Clear();
            // Brush testing
            Origin.AddCautiously(b!.Paint(Events.Click ? 1 : 0,
                Geo.Create.Star(5, 29, 53).Colored(new HSLA(Time, 1, 1, 120))
            )
            .Sub(
                    m =>
                    m.DrivenPM(
                        x => x + 10.1,
                        y => y
                    )
                ))
            ;

            if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_z])
            {
                Origin["cube"].RotatedZ(0.01);
            }
            if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_y])
            {
                Origin["cube"].RotatedY(0.01);
            }
            if (Events.keys[SDL2.SDL.SDL_Keycode.SDLK_x])
            {
                Origin["cube"].RotatedX(0.01);
            }
        }
    }
}
