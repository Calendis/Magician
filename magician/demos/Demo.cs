using Magician.Geo;
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

            // Text moving in a Lissajous pattern
            Origin["textTest"] = new Multi(100, 0)
            .Textured(
                new Renderer.Text("I am text", HSLA.RandomVisible(), 70).Render()
            )
            .DrivenXY(
                x => x + Math.Cos(Time / 3),
                y => y + Math.Sin(Time / 10)
            );


            // Hexagonal grid
            Origin["hex grid"] = new Symbols.Hexagonal(7, 7).Render(45).Positioned(300, 0);

            /* Multi-line text */
            Origin["paragraph1"] = new UI.RichParagraph(-330, 200, HSLA.RandomVisible(), 32,
                
                $"{UI.TFS.RGB(255, 0, 0)}Rich paragraph{UI.TFS.Back} now supports",
                $"{UI.TFS.RGB(255, 128, 0)}HTML-style{UI.TFS.RGB(255, 255, 0)} nesting,",
                $"{UI.TFS.Back}because {UI.TFS.Back}why{new UI.TextFormatSetting(HSLA.RandomVisible(), 18)} not{UI.TFS.Back}?"
            );

            // Non-square mouseover
            Origin["my star"] = Create.Star(-200, -250, HSLA.RandomVisible(), 10, 40, 140);
            mo = Interactive.Sensor.MouseOver(Origin["my star"]);
        }

        public override void Loop()
        {
            Renderer.Control.Clear();
            Origin["my star"].Rotated(0.01);
            Origin["my star"].Colored(new RGBA(0, 255 * mo!.Evaluate(), 255, 255));

            Origin["memTest"] = new Multimap(1,
                x => 100 * Math.Sin(x / 2 + Time / 4),
                y => 100 * Math.Cos(y / 7 + Time / 4)
            )
            .TextAlong(-40, 40, 0.3, "Wheeeeeeeeeeeeeeeeeee!", new HSLA(Time / 10, 1, 1, 222), 32, 30, -100)
            ;

            /*
            Origin["alert!!"] = new UI.RichParagraph(0, 350,
                $"{new UI.TextFormatSetting(new HSLA(Time+1, 1, 1, 250))}Check {new UI.TextFormatSetting(new HSLA(Time, 1, 1, 250))}THIS{new UI.TextFormatSetting(new HSLA(Time-1, 1, 1, 250))}out", new RGBA(0xff0000ff)
            )
            .Translated(10*Math.Cos(Time), 20*Math.Sin(Time/4))
            ;
            */
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
            Origin["parametric"] = new Multimap(1,
                x => 180 * Math.Cos(x / 3) + 10 * Math.Sin(Time / 2),
                y => 180 * Math.Sin(y / 7 + Time)
            ).TextAlong(-49, 49, 0.3, "Here's an example of a Multimap with 1 input and two outputs being used to draw text parametrically"
            , new RGBA(0x00ff9080));
        }
    }

    public class RandomSpinner : Spell
    {
        public override void PreLoop()
        {
            // Add a Cartesian plane
            Origin["cartPlane"] = new UI.RuledAxes(100, 10, 100, 10).Render();

            Origin["myMulti"] = Create.RegularPolygon(5, 120).Colored(HSLA.Random(saturation: 1, lightness: 1, alpha: 100))
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
        }
    }
}
