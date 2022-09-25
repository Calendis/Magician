namespace Magician
{
    public class Multi : Single
    {
        protected List<Multi> constituents;
        public List<Multi> Constituents
        {
            get => constituents;
        }
        protected bool filled = false;
        public int Count
        {
            get => constituents.Count;
        }
        public Multi(params Multi[] cs)
        {
            constituents = new List<Multi> {};
            constituents.AddRange(cs);
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            foreach (Multi c in constituents)
            {
                // Make sure constituents are drawn relative to parent Multi
                c.Draw(ref renderer, xOffset+pos[0], yOffset+pos[1]);
            }
        }

        public new void Drive(params double[] x)
        {
            foreach (Driver d in drivers)
            {
                d.Drive(x);
            }
            foreach (Multi c in constituents)
            {
                c.Drive(x);
            }
        }

        public void AddSubDrivers(Driver[] ds)
        {
            for (int i = 0; i < ds.Length; i++)
            {
                constituents[i].AddDriver(ds[i]);
            }
        }

        public void AddSubDrivers(params Driver[][] dss)
        {
            foreach (Driver[] ds in dss)
            {
                AddSubDrivers(ds);
            }
        }

        public void SetConstituent(int i, Multi m)
        {
            Multi c = constituents[i];
            m.IncrX(c.pos[0]);
            m.IncrY(c.pos[1]);
            constituents[i] = m;
        }
    }
}