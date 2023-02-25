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

        }

        public override void Loop()
        {
            Renderer.Control.Clear();
            Origin["my star"].RotatedZ(0.01);
            Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

            Origin["memTest"] = new IOMap(1,
                x => 100 * Math.Sin(x / 2 + Time / 4),
                y => 100 * Math.Cos(y / 7 + Time / 4)
            )
            .TextAlong(-40, 40, 0.3, "WOaAoAoAHOAHOAHH!! I'm flying!!!", new HSLA(Time / 10, 1, 1, 222), 32, 30, -100)
            ;

            // Perform a Matrix rotation
            Matrix mmx = new Matrix(Origin["btn"]);
            Matrix result = Matrix.Rotation(0.004).Mult(mmx);
            Origin["btn"].Positioned(result);
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
            // A cube
            Origin["cube"] = Create.Cube(0, 0, 0, 200)
            .Sub(
                m =>
                m.Sub(
                    n =>
                    n.DrivenPM(
                    p => p + 0*0.01,
                    m => m
                )
                )
            )
            ;
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
                        x => x + 10.1,
                        y => y
                    )
                )
            ;

            Origin["cube"].Sub(
                m => m
                //.RevolvedX(-0.0000003)
                .RotatedY(-0.03)
                .RotatedZ(-0.015)
                .RotatedX(0.02)
            );
        }
    }
}
