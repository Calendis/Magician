namespace Magician.Library
{
    public static class Spellbook
    {
        // I never knew 'static public' was allowed
        static public List<Spell> Spells { get; set; }
        static int toSwitchTo = -1;
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
        static Spellbook()
        {
            Spells = new List<Spell>();
        }

        public static void PrepareSpell(int i)
        {
            CurrentSpell.Time = 0;
            Geo.Ref.Origin = Spells[i].GetOrigin();
            Spells[i].PreLoop();
        }

        public static void Clean()
        {
            Geo.Ref.Origin.DisposeAllTextures();
        }

        public static void Cache(Spell s)
        {
            Spells.Add(s);
            toSwitchTo = Spells.Count - 1;
        }

        public static void Load(Spell s)
        {
            Cache(s);
            DoSwitch();
        }

        public static void SwapTo(int i)
        {
            toSwitchTo = i < Spells.Count ? i : Spells.Count;
        }
        public static void Loop(double t)
        {
            if (toSwitchTo > 0)
            {
                Interactive.Events.Click = false;
                DoSwitch();
                toSwitchTo = -1;
            }
            CurrentSpell.Time = t;
            CurrentSpell.Loop();
        }

        public static void DoSwitch()
        {
            idx = toSwitchTo;
            PrepareSpell(toSwitchTo);
        }
    }
}
