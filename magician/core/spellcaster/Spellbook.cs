/*
    The static Spellcaster is the outer layer of the Magician library. It is responsible for managing
    Spells, which are views of 3D (or 2D) Geometry, represented by Multis
 */
using Magician.Geo;
using Magician.Paint;

namespace Magician.Core.Caster
{
    public static class Spellbook
    {
        static public List<Spell> Spells { get; set; } = new();
        // Index of the cached Spell
        static int toSwitchTo = 0;  // 0 for no switch
        static bool delta = false;
        public static Spell CurrentSpell
        {
            get
            {
                if (Spells.Count == 0)
                {
                    Scribe.Error("You must load a Spell first");
                }
                return Spells[idx];
            }
        }
        static int idx = 0;

        public static void Cast()
        {
            delta = false;
            idx = toSwitchTo;
            Renderer.Drawables.Clear();
        }

        public static void Clean()
        {
            Geo.Ref.Origin.DisposeAllTextures();
        }

        // Clears a spell and readies it for casting by setting its initial conditions through the
        // static Spell.PreLoop method.
        public static void Prepare(Spell s)
        {
            Spells.Add(s);
            toSwitchTo = Spells.Count - 1;
            delta = true;
            CurrentSpell.Time = 0;
            // Sets the static Origin reference to point to our prepared Spell
            Ref.Origin = Spells[toSwitchTo].Origin;
            Render.nodeToSize.Clear();
            Render.StaleAll();
            Renderer.pointBufferSize = 0;
            Renderer.lineBufferSize = 0;
            Renderer.triBufferSize = 0;
            Spells[toSwitchTo].PreLoop();
            Scribe.Info($"Readied {s}");
        }

        public static void Animate(double time)
        {
            // Spell is locked
            if (delta) { return; }
            CurrentSpell.Time = time;
            CurrentSpell.Loop();
            CurrentSpell.Render();
        }
    }
}
