// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using Magician.UI;
using Magician.Geo;
using static Magician.Geo.Create;
using static Magician.Geo.Ref;

using static SDL2.SDL;
using Magician.Interactive;

namespace Magician
{
    static class Spell
    {
        // For stuff that can be defined once and left alone
        static Random rng = new Random();
        static UI.Grid uiGrid;

        // Initializations
        static Spell()
        {
            uiGrid = UI.Presets.Graph.Cartesian();
        }

        public static void PreLoop()
        {
            Renderer.Control.Clear();
            // Add a grid
            Origin.Add(uiGrid.Render());

            Origin.Add(
                ((IMap)new DirectMap(x => 100*Math.Sin(x/30)))
                .MultisAlong(-200, 200, 10.2, RegularPolygon(5, 10))
            );

            Origin.Add(RegularPolygon(2, 100, 3, 100)
                //.DrivenPM(x => x+0.01, y => y)
                .Sub(
                    m => m
                    .DrivenPM(x => x-0.01, y => y+5*Math.Sin(Environment.Time))
                ).Colored(new RGBA(0xffd0d080))
            );

            Multi limeSq = RegularPolygon(4, 100).Sub(m => m.Rotated(Math.PI/4)).Colored(new RGBA(0x00ff00ff));
            IMap mo = Sensor.MouseOver(limeSq);
            Origin.Add(
                limeSq.DrivenXY(mo, new DirectMap(y => y))
            );

            /*
            Origin.Add(
                mm.MultisAlong(-200, 200, 10.2, RegularPolygon(5, 10))
            );
            */

        }

        // For stuff that needs to redefined every frame
        public static void Loop()
        { 
            Renderer.Control.Clear();
            //uiGrid = uiGrid.Update();
            //Origin[0].DisposeAllTextures();
            //Origin[0] = uiGrid.Render();


            //Origin[1].DisposeAllTextures();
            
            /*
            Origin[1] = (((IMap)new DirectMap(x => 16 * Math.Sin(x / 40 + Environment.Time)))
                .TextAlong(-200, 600, 5 * Math.PI, "Hello there, my friendsssssssssssssssssss", new HSLA(Environment.Time, 1, 1, 255)));
            */
        }
    }
}