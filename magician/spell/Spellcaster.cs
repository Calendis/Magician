using System;

namespace Magician.Library
{
    public class Spellcaster
    {
        public List<Spell> Spellbook {get; set;}
        public Spell CurrentSpell
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
        public int idx = 0;
        public Spellcaster()
        {
            Spellbook = new List<Spell>();
        }

        public void PrepareSpell()
        {
            CurrentSpell.Time = 0;
            if (Geo.Ref.Origin is not null)
            {
                Geo.Ref.Origin.DisposeAllTextures();
            }
            Geo.Ref.Origin = CurrentSpell.Origin;
        }
    }
}
