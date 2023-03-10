// This class is where you create your stuff
// It's static for now

using Magician.Renderer;
using Magician.UI;
using Magician.Geo;
using Magician.Interactive;
using Magician.Data;

namespace Magician.Library
{
    public abstract class Spell
    {
        public double Time { get; set; }
        public Random RNG = new Random();
        public double RandX => RNG.NextDouble() * Globals.winWidth - Globals.winWidth / 2;
        public double RandY => RNG.NextDouble() * Globals.winHeight - Globals.winHeight / 2;

        // The Origin is the eventual parent Multi for all Multis. Each Spell has its own Origin, and ..
        // ... when a Spell is loaded into the Spellcaster, Geo.Ref.Origin is set to that Spell's Origin
        protected Multi Origin = Create.Point(null, 0, 0, Data.Col.UIDefault.FG)
        .WithFlags(DrawMode.INVISIBLE)
        .Tagged("Spell origin")
        ;

        // Initializations
        public Spell()
        {
            //
        }

        public abstract void PreLoop();
        public abstract void Loop();

        public Multi GetOrigin()
        {
            return Origin;
        }

    }
}