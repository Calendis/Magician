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
        protected bool lined = false;
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

        public List<Driver> Drivers
        {
            get => drivers;
        }
        
        public Multi(params Multi[] cs)
        {
            constituents = new List<Multi> {};
            constituents.AddRange(cs);
            foreach (Multi c in constituents)
            {
                c.SetParent(this);
            }
            col = Globals.fgCol;
        }

        public Multi(double x, double y, Color col, bool lined, params Multi[] cs) : this(cs)
        {
            SetX(x);
            SetY(y);
            this.col = col;
            this.lined = lined;
        }

        public Multi(double x, double y, Color col, params Multi[] cs) : this(x, y, col, false, cs) {}

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            
            byte r = col.R;
            byte g = col.G;
            byte b = col.B;
            byte a = col.A;
            
            for (int i = 0; i < constituents.Count-1; i++)
            {
                // If lined, draw lines between the constituents as if they were vertices in a polygon
                if (lined)
                {
                    Point p0 = constituents[i].Point();
                    Point p1 = constituents[i+1].Point();
                    
                    SDL_SetRenderDrawColor(renderer, r, g, b, a);
                    
                    SDL_RenderDrawLine(renderer,
                    (int)p0.XCartesian(pos[0]+xOffset), (int)p0.YCartesian(pos[1]+yOffset),
                    (int)p1.XCartesian(pos[0]+xOffset), (int)p1.YCartesian(pos[1]+yOffset));
                }
                
                // Recursively draw the constituents
                Multi c = constituents[i];
                c.Draw(ref renderer, xOffset+pos[0], yOffset+pos[1]);
            }
            
            if (lined && constituents.Count > 0)
            {
                Point pLast = constituents[constituents.Count-1].Point();
                Point pFirst = constituents[0].Point();
                
                SDL_SetRenderDrawColor(renderer, r, g, b, a);                
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

        public void AddDrivers(Driver[] ds)
        {
            foreach (Driver d in ds)
            {
                AddDriver(d);
            }
        }
        
        public void AddSubDrivers(Driver[] ds)
        {
            for (int i = 0; i < ds.Length; i++)
            {
                constituents[i].AddDriver(ds[i]);
            }
        }

        public Multi Driven(Driver d, string s)
        {
            Multi copy = Copy();
            Driver dc = new Driver(d, copy, s);
            copy.AddDriver(dc);
            return copy;
        }

        public Multi SubDriven(Driver d, string s)
        {
            Multi copy = Copy();
            foreach(Multi c in copy.constituents)
            {
                Driver dc = new Driver(d, c, s);
                c.AddDriver(dc);
            }
            return copy;
        }

        public void SetConstituent(int i, Multi m)
        {
            m.SetX(constituents[i].XAbsolute(0));
            m.SetY(constituents[i].YAbsolute(0));
            m.SetParent(this);
            constituents[i] = m;
        }

        /*
            DEBUG status:
                The drivers of Multi m are copied correctly
                The subdrivers (constituents' drivers are not!)
        */
        /*
        public Multi Recursed(Multi m)
        {
            Multi m2 = m.Copy();
            
            for (int i = 0; i < m.constituents.Count; i++)
            {
                Multi c = Copy();
                c.IncrX(m.constituents[i].XAbsolute(0));
                c.IncrY(m.constituents[i].YAbsolute(0));
                m2.SetConstituent(i, c);
            }
            return m2;
        }*/

        // Wield is a form of recursion where each constituent is replaced with a copy of the given Multi
        public Multi Wield(Multi outer)
        {
            Multi innerCopy = Copy();
            for (int i = 0; i < Count; i++)
            {
                // Make a copy of the outer Multi and position it against the inner Multi
                Multi outerCopy = outer.Copy();
                outerCopy.SetX(constituents[i].XAbsolute(0));
                outerCopy.SetY(constituents[i].YAbsolute(0));
                
                // Set that copy as the respective constituent of the Multi
                innerCopy.constituents[i] = outerCopy;

                // Copy over drivers from each constituent of the Multi to the outer copy
                for (int j = 0; j < constituents[i].drivers.Count; j++)
                {
                    Driver originalSubDriver = constituents[i].drivers[j];
                    innerCopy.constituents[i].AddDriver(originalSubDriver.CopiedTo(innerCopy.constituents[i]));
                }
            }

            return innerCopy;
        }
        public Multi Wield()
        {
            return Wield(this);
        }

        // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
        public Multi Surround(Multi inner)
        {
            return inner.Wield(this);
        }
        public Multi Surround()
        {
            return Surround(this);
        }

        /*
        public Multi Recursed()
        {
            return Recursed(this);
        }
        */

        public Multi Copy()
        {
            Multi copy = new Multi(pos[0], pos[1], col, lined);
            
            // Copy the drivers
            for (int i = 0; i < drivers.Count; i++)
            {
                copy.drivers.Add(drivers[i].CopiedTo(copy));
            }

            // Copy the constituents
            Multi[] cs = new Multi[Count];
            for (int i = 0; i < Count; i++)
            {
                cs[i] = constituents[i].Copy();
            }
            copy.constituents.AddRange(cs);
            return copy;
        }

        /*
        public Multi Copy()
        {            
            // Copy the constituents
            Multi[] copiedConstituents = new Multi[constituents.Count];
            for (int i = 0; i < constituents.Count; i++)
            {
                copiedConstituents[i] = constituents[i].Copy();
            }
            
            // Copy the drivers
            Driver[] copiedDrivers = new Driver[drivers.Count];
            Multi m2 = new Multi(pos[0], pos[1], col.Copy(), lined, copiedConstituents);
            for (int i = 0; i < copiedDrivers.Length; i++)
            {
                copiedDrivers[i] = drivers[i].CopiedTo(m2);
            }
            m2.AddDrivers(copiedDrivers);
            
            return m2;
        }
        */

        public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
        {
            List<Point> ps = new List<Point>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude*Math.Cos(angle*i/180*Math.PI);
                double y = magnitude*Math.Sin(angle*i/180*Math.PI);
                ps.Add(new Point(x, y, col));
            }
            return new Multi(xOffset, yOffset, col, true, ps.ToArray());
        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Globals.fgCol, sides, magnitude);
        }
        public static Multi RegularPolygon(int sides, double magnitude)
        {
            return RegularPolygon(0, 0, sides, magnitude);
        }

        public void SetParent(Multi m)
        {
            parent = m;
        }

        public override string ToString()
        {
            string s = "";
            foreach (Multi c in constituents)
            {
                s += c.GetType() + ": " + c.ToString() + "\n  ";
            }
            return s;
        }
    }
}