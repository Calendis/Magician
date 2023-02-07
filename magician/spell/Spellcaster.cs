namespace Magician.Library
{
    public static class Spellcaster
    {
        // I never knew 'static public' was allowed
        static public List<Spell> Spellbook { get; set; }
        public static Spell CurrentSpell
        {
            get
            {
                if (Spellbook.Count == 0)
                {
                    throw new IndexOutOfRangeException("You must load a spell first");
                }
                return Spellbook[idx];
            }
        }
        static public int idx = 0;
        static Spellcaster()
        {
            Spellbook = new List<Spell>();
        }

        public static void PrepareSpell()
        {
            CurrentSpell.Time = 0;
            if (Geo.Ref.Origin is not null)
            {
                Geo.Ref.Origin.DisposeAllTextures();
            }
            Geo.Ref.Origin = CurrentSpell.GetOrigin();
            CurrentSpell.PreLoop();
        }

        public static void Load(Spell s)
        {
            Spellbook.Add(s);
            idx = Spellbook.Count - 1;
            PrepareSpell();
        }
        public static void SwapTo(int i)
        {
            idx = i < Spellbook.Count ? i : Spellbook.Count;
        }
        public static void Loop(double t)
        {
            CurrentSpell.Time = t;
            CurrentSpell.Loop();
        }
    }
}
