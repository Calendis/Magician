namespace Magician.Library
{
    public static class Spellcaster
    {
        // I never knew 'static public' was allowed
        static public List<Spell> Spellbook { get; set; }
        static int toSwitchTo = -1;
        public static Spell CurrentSpell
        {
            get
            {
                if (Spellbook.Count == 0)
                {
                    Scribe.Error("You must load a Spell first");
                }
                return Spellbook[idx];
            }
        }
        static public int idx = 0;
        static Spellcaster()
        {
            Spellbook = new List<Spell>();
        }

        public static void PrepareSpell(int i)
        {
            CurrentSpell.Time = 0;
            Geo.Ref.Origin = Spellbook[i].GetOrigin();
            Spellbook[i].PreLoop();
        }

        public static void Clean()
        {
            Geo.Ref.Origin.DisposeAllTextures();
        }

        public static void Cache(Spell s)
        {
            Spellbook.Add(s);
            toSwitchTo = Spellbook.Count - 1;
        }

        static void Load(Spell s)
        {
            Cache(s);
            DoSwitch();
        }

        public static void SwapTo(int i)
        {
            toSwitchTo = i < Spellbook.Count ? i : Spellbook.Count;
        }
        public static void Loop(double t)
        {
            if (toSwitchTo > 0)
            {
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
