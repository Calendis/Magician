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
                Spellbook.Cache(new RandomSpinner());
            }
            );

        }

        public override void Loop()
        {
            Renderer.Control.Clear();
            Origin["my star"].Rotated(0.01);
            Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

            Origin["memTest"] = new IOMap(1,
                x => 100 * Math.Sin(x / 2 + Time / 4),
                y => 100 * Math.Cos(y / 7 + Time / 4)
            )
            .TextAlong(-40, 40, 0.3, "WOaAoAoAHOAHOAHH!! I'm flying!!!", new HSLA(Time / 10, 1, 1, 222), 32, 30, -100)
            ;
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

    public class RandomSpinner : Spell
    {
        Brush? b;
        
        public override void PreLoop()
        {
            b = new Brush(new CustomMap(x => Events.MouseX), new CustomMap(y => Events.MouseY));
            // A spinning shape
            Origin["myMulti"] = Create.RegularPolygon(Data.Rand.RNG.Next(3, 10), 120).Colored(HSLA.Random(saturation: 1, lightness: 1, alpha: 100))
            .Sub(
                m => m
                .DrivenPM(
                    ph => ph + 0.025,
                    m => m
                )
            );
        }

        public override void Loop()
        {
            Renderer.Control.Clear();
            // Brush testing
            b!.Paint(Events.Click ? 1 : 0,
                Geo.Create.Star(5, 29, 53).Colored(new HSLA(Time, 1, 1, 120))
                //.Rotated(Time)
            )
            .Sub(
                    m =>
                    m.DrivenPM(
                        x => x+10.1,
                        y => y
                    )
                )
            ;
        }
    }
}
