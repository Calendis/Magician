/* 
    The Spell provides the environment for the simulation. It does nothing until you pass it to the
    Spellcaster.Cast method
 */
using Magician.Geo;
using Magician.Data;

namespace Magician.Library
{
    public abstract class Spell
    {
        public double Time { get; set; }
        public Random RNG = new Random();
        public double RandX => RNG.NextDouble() * Globals.winWidth - Globals.winWidth / 2;
        public double RandY => RNG.NextDouble() * Globals.winHeight - Globals.winHeight / 2;

        // The Origin is the root for a tree of Multis
        public Multi Origin {get; set;} = Create.Point(null, 0, 0, Data.Col.UIDefault.FG)
        .Flagged(DrawMode.INVISIBLE)
        .Tagged("Spell origin")
        ;

        // Sets the initial condirions for the Spell. This is called automatically by the Spellcaster
        public abstract void PreLoop();
        // The main loop of the spell. This is called automatically by the spellcaster
        public abstract void Loop();
    }
}