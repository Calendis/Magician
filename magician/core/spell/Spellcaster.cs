/*
    The static Spellcaster is the outer layer of the Magician library. It is responsible for managing
    Spells, which are views of 3D (or 2D) Geometry, represented by Multis
 */
namespace Magician.Library
{
    public static class Spellcaster
    {
        static public List<Spell> Spells { get; set; } = new();
        // Index of the cached Spell
        static int toSwitchTo = 0;  // 0 for no switch
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
        static public int idx = 0;

        // Clears a spell and readies it for casting by setting its initial conditions through the
        // static Spell.PreLoop method.
        public static void Cast(int i)
        {
            CurrentSpell.Time = 0;
            // Sets the static Origin reference to point to our prepared Spell
            Geo.Ref.Origin = Spells[i].Origin;
            Spells[i].PreLoop();
        }

        public static void Clean()
        {
            Geo.Ref.Origin.DisposeAllTextures();
        }

        // Caches a spell so it can be loaded
        public static void Prepare(Spell s)
        {
            Spells.Add(s);
            toSwitchTo = Spells.Count - 1;
            DoSwitch();
        }

        public static void SetTime(double t)
        {
            CurrentSpell.Time = t;
            CurrentSpell.Loop();
        }

        public static void DoSwitch()
        {
            idx = toSwitchTo;
            Cast(toSwitchTo);
        }
    }
}
