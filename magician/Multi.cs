using static SDL2.SDL;

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
        protected bool lined = true;
        protected Color col;
        public Color Col
        {
            get => col;
            set
            {
                col = value;
            }
        }
        public int Count
        {
            get => constituents.Count;
        }
        
        public Multi(params Multi[] cs)
        {
            constituents = new List<Multi> {};
            constituents.AddRange(cs);
            col = Globals.fgCol;
        }

        public Multi(double x, double y, params Multi[] cs) : this(cs)
        {
            SetX(x);
            SetY(y);
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            
            for (int i = 0; i < constituents.Count-1; i++)
            {
                if (lined && constituents.Count > 0)
                {
                    Point p0 = constituents[i].Point();
                    Point p1 = constituents[i+1].Point();
                    SDL_SetRenderDrawColor(renderer, p0.Col.R, p0.Col.G, p0.Col.B, 255);
                    SDL_RenderDrawLine(renderer,
                    (int)p0.XCartesian(pos[0]+xOffset), (int)p0.YCartesian(pos[1]+yOffset),
                    (int)p1.XCartesian(pos[0]+xOffset), (int)p1.YCartesian(pos[1]+yOffset));
                }
                Multi c = constituents[i];
                c.Draw(ref renderer, xOffset+pos[0], yOffset+pos[1]);
            }
            
            if (lined && constituents.Count > 0)
            {
                Point pLast = constituents[constituents.Count-1].Point();
                Point pFirst = constituents[0].Point();
                SDL_SetRenderDrawColor(renderer, pLast.Col.R, pLast.Col.G, pLast.Col.B, 255);
                SDL_RenderDrawLine(renderer,
                (int)pLast.XCartesian(pos[0]+xOffset), (int)pLast.YCartesian(pos[1]+yOffset),
                (int)pFirst.XCartesian(pos[0]+xOffset), (int)pFirst.YCartesian(pos[1]+yOffset));
            }

            
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

        public Multi Driven(Driver d, string s)
        {
            d.SetOutput(Driver.StringMap(this, s));
            AddDriver(d);
            return this;
        }

        public Multi SubDriven(Driver d, string s)
        {
            foreach(Multi c in constituents)
            {
                Driver dc = new Driver(d, c, s);
                //dc.SetRef(c);
                c.AddDriver(dc);
            }
            return this;
        }

        public void SetConstituent(int i, Multi m)
        {
            constituents[i] = m;
        }

        public void Recurse(Multi m)
        {
            for (int i = 0; i < constituents.Count; i++)
            {
                Multi m2 = m.Copy();
                m2.IncrX(constituents[i].XAbsolute(0));
                m2.IncrY(constituents[i].YAbsolute(0));
                SetConstituent(i, m2);
            }
        }

        public void Recurse()
        {
            Recurse(this.Copy());
        }

        public Multi Recursed(Multi m)
        {
            Recurse(m);
            return this;
        }
        public Multi Recursed()
        {
            Recurse();
            return this;
        }
        public Multi Copy()
        {
            Multi[] constituentsCopy = new Multi[Constituents.Count];

            for (int i = 0; i < constituentsCopy.Length; i++)
            {
                constituentsCopy[i] = constituents[i].Copy();
                constituentsCopy[i].SetDriverRefs();
            }
            Multi m2 = new Multi(pos[0], pos[1], constituentsCopy);
            m2.SetDriverRefs();
            return m2;
        }

        public void SetDriverRefs()
        {
            foreach (Driver d in drivers)
            {
                d.SetRef(this);
            }
        }

        public static Multi RegularPolygon(int sides, double magnitude)
        {
            List<Point> ps = new List<Point>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude*Math.Cos(angle*i/180*Math.PI);
                double y = magnitude*Math.Sin(angle*i/180*Math.PI);
                ps.Add(new Point(x, y));
            }
            return new Multi(ps.ToArray());
        }

        public override string ToString()
        {
            string s = "";
            foreach (Multi c in constituents)
            {
                s += c.GetType() + c.ToString() + "\n  ";
            }
            return s;
        }
    }
}