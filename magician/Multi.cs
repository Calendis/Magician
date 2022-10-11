/*
*  A Multi represents a collection of one more more Multis, with some functionality inherited from Single
*  ALL mathematical objects created in Magician are Multis, even simple objects like Points
*/
using static SDL2.SDL;

namespace Magician
{
    public class Multi : Quantity, Drawable, Driveable
    {
        protected double[] pos = new double[]{0,0};
        protected List<Drawable> constituents;
        public List<Drawable> Constituents
        {
            get => constituents;
        }
        // If true, a Multi will be drawn with its constituents connected by lines
        // This is useful for plots, polygons, etc
        protected bool lined = false;
        
        // If true, the last constituent in a Multi will be drawn with a line connecting to the first one
        // This is desirable for say, a polygon, but undesirable for say, a plot
        protected bool linedCompleted = false;
        protected Color col;

        // Create a multi from a list of multis
        public Multi(params Multi[] cs) : base(0)
        {
            constituents = new List<Drawable> {};
            constituents.AddRange(cs);
            foreach (Multi c in constituents)
            {
                c.SetParent(this);
            }
            col = Globals.fgCol;
        }

        // Create a multi and define its position, colour, and drawing properties
        public Multi(double x, double y, Color col, bool lined, bool linedCompleted, params Multi[] cs) : this(cs)
        {
            //SetX(x);
            //SetY(y);
            pos[0] = x;
            pos[1] = y;
            this.col = col;
            this.lined = lined;
            this.linedCompleted = linedCompleted;
        }

        public Multi(double x, double y, Color col, params Multi[] cs) : this(x, y, col, true, false, cs) {}

        public double XAbsolute(double offset)
        {
            return pos[0] + offset;
        }
        public double YAbsolute(double offset)
        {
            return pos[1] + offset;
        }

        public Color Col
        {
            get => col;
            set => col = value;
        }

        public void SetX(double x)
        {
            double parentOffset = 0;
            
            if (Parent() is not null)
            {
                parentOffset = Parent().XAbsolute(0);
            }
            pos[0] = x - parentOffset;
        }
        public void SetY(double x)
        {
            double parentOffset = 0;
            if (Parent() is not null)
            {
                parentOffset = Parent().YAbsolute(0);
            }
            pos[1] = x - parentOffset;
        }

        public int Count
        {
            get => constituents.Count;
        }

        public List<Driver> Drivers
        {
            get => drivers;
        }

        public bool Lined
        {
            set => lined = value;
        }

        public void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
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
                    Drawable p0 = constituents[i].GetPoint();
                    Drawable p1 = constituents[i+1].GetPoint();
                    
                    SDL_SetRenderDrawColor(renderer, r, g, b, a);
                    
                    SDL_RenderDrawLine(renderer,
                    (int)p0.XCartesian(pos[0]+xOffset), (int)p0.YCartesian(pos[1]+yOffset),
                    (int)p1.XCartesian(pos[0]+xOffset), (int)p1.YCartesian(pos[1]+yOffset));

                }
                
                // Recursively draw the constituents
                Drawable d = constituents[i];
                d.Draw(ref renderer, xOffset+pos[0], yOffset+pos[1]);
            }
            
            if (linedCompleted && constituents.Count > 0)
            {
                Drawable pLast = constituents[constituents.Count-1].GetPoint();
                Drawable pFirst = constituents[0].GetPoint();
                
                SDL_SetRenderDrawColor(renderer, r, g, b, a);                
                SDL_RenderDrawLine(renderer,
                (int)pLast.XCartesian(pos[0]+xOffset), (int)pLast.YCartesian(pos[1]+yOffset),
                (int)pFirst.XCartesian(pos[0]+xOffset), (int)pFirst.YCartesian(pos[1]+yOffset));
            }

            
            foreach (Drawable d in constituents)
            {
                // Make sure constituents are drawn relative to parent Multi
                d.Draw(ref renderer, xOffset+pos[0], yOffset+pos[1]);
            }
        }

        /**/
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
                ((Multi)constituents[i]).AddDriver(ds[i]);
            }
        }

        public Multi Driven(Func<double[], double> df, string s)
        {
            //Multi copy = Copy();
            Action<double> output = StringMap(this, s);
            Driver d = new Driver(df, output);
            d.ActionString = s;
            AddDriver(d);
            return this;
        }

        public Multi SubDriven(Func<double[], double> df, string s)
        {
            //Multi copy = Copy();
            foreach(Multi c in /*copy.*/constituents)
            {
                Action<double> output = StringMap(c, s);
                Driver d = new Driver(df, output);
                d.ActionString = s;
                c.AddDriver(d);
            }
            return this;
        }

        public Multi Copy()
        {
            Multi copy = new Multi(pos[0], pos[1], col.Copy(), lined, linedCompleted);
            
            // Copy the drivers
            for (int i = 0; i < drivers.Count; i++)
            {
                copy.drivers.Add(drivers[i].CopiedTo(copy));
            }

            // Copy the constituents
            Drawable[] cs = new Drawable[Count];
            for (int i = 0; i < Count; i++)
            {
                cs[i] = ((Multi)constituents[i]).Copy();
            }
            copy.constituents.AddRange(cs);
            return copy;
        }

        // Wield is a form of recursion where each constituent is replaced with a copy of the given Multi
        public Multi Wielding(Multi outer)
        {
            Multi innerCopy = Copy();
            for (int i = 0; i < Count; i++)
            {
                // Make a copy of the outer Multi and position it against the inner Multi
                Drawable outerCopy = outer.Copy();
                outerCopy.SetX(constituents[i].XAbsolute(0));
                outerCopy.SetY(constituents[i].YAbsolute(0));
                
                // Set that copy as the respective constituent of the Multi
                innerCopy.constituents[i] = outerCopy;

                // Copy over drivers from each constituent of the Multi to the outer copy
                for (int j = 0; j < ((Multi)constituents[i]).drivers.Count; j++)
                {
                    Multi c = (Multi)constituents[i];
                    Driver originalSubDriver = c.drivers[j];
                    Multi innerCopyC = (Multi)((Multi)innerCopy).constituents[i];
                    innerCopyC.AddDriver(originalSubDriver.CopiedTo(innerCopyC));
                }
            }

            return innerCopy;
        }
        public Multi Wielding(Multi outer, Func<Multi, Multi> F)
        {
            return Wielding(F(outer));
        }

        
        // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
        public Multi Surrounding(Multi inner)
        {
            Drawable thisSurroundingInner = inner.Wielding(this);
            thisSurroundingInner.SetX(XAbsolute(0));//parent.XAbsolute(0)));
            thisSurroundingInner.SetY(YAbsolute(0));//(parent.YAbsolute(0)));
            return ((Multi)thisSurroundingInner).Wielding(this);
        }
        public Multi Surrounding (Multi inner, Func<Multi, Multi> F)
        {
            return Surrounding(F(inner));
        }
        
        public Multi Recursed()
        {
            return Wielding(this);
        }
        public Multi Recursed(Func<Multi, Multi> F)
        {
            return Wielding(F.Invoke(this));
        }

        public void Scale(double factor)
        {
            foreach (Drawable c in constituents)
            {
                c.SetMagnitude(c.Magnitude * factor);
                c.Scale(factor);
            }
        }

        public Multi Scaled(double factor)
        {
            Multi copy = Copy();
            copy.Scale(factor);
            return copy;
        }
        ////

        public Multi Invisible()
        {
            lined = false;
            linedCompleted = false;
            return this;
        }

        // A Multi keeps a reference to its "parent", which is a Multi that has this Multi as
        // a constituent
        public new Drawable Parent()
        {
            return parent;
        }
        
        public void SetParent(Multi m)
        {
            parent = m;
        }

        public override string ToString()
        {
            string s = "";
            switch (Count)
            {
                case(0):
                    s += "Empty Multi";
                    break;
                case(1):
                    s += "Lonely Multi";
                    break;
                case(2):
                    s += $"Point Multi ({constituents[0]}, {constituents[1]})";
                    break;
                default:
                    s += $"{Count}-Multi (";
                    foreach (Multi m in constituents)
                    {
                        s += m.ToString() + "), ";
                    }
                    break;
            }
            s += $" at {((Drawable)this).XCartesian(0)}, {((Drawable)this).YCartesian(0)}";
            return s;
        }

        /*
        *  Static methods
        */
        // Create a regular polygon with a position, number of sides, color, and magnitude
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

            return new Multi(xOffset, yOffset, col, true, true, ps.ToArray());;
        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Globals.fgCol, sides, magnitude);
        }
        public static Multi RegularPolygon(int sides, double magnitude)
        {
            return RegularPolygon(0, 0, sides, magnitude);
        }

        public static Action<double> StringMap(Drawable m, string s)
        {
            Action<double> o;
            s = s.ToUpper();
            switch(s)
            {
                case "X":
                    o = m.SetX;
                    break;
                case "X+":
                    o = m.IncrX;
                    break;
                
                case "Y":
                    o = m.SetY;
                    break;
                case "Y+":
                    o = m.IncrY;
                    break;

                case "PHASE":
                    o = m.SetPhase;
                    break;
                case "PHASE+":
                    o = m.IncrPhase;
                    break;

                case "MAGNITUDE":
                    o = m.SetMagnitude;
                    break;
                
                case "MAGNITUDE+":
                    o = m.IncrMagnitude;
                    break;

                case "COL0":
                    o = m.SetCol0;
                    break;
                
                case "COL1":
                    o = m.SetCol1;
                    break;

                case "COL2":
                    o = m.SetCol2;
                    break;

                case "COL3":
                    o = m.SetAlpha;
                    break;

                case "COL0+":
                    o = m.IncrCol0;
                    break;
                
                case "COL1+":
                    o = m.IncrCol1;
                    break;

                case "COL2+":
                    o = m.IncrCol2;
                    break;

                case "COL3+":
                    o = m.IncrAlpha;
                    break;
                
                default:
                    Console.WriteLine($"ERROR: Unknown driver string {s}");
                    throw new NotImplementedException();
            }
            return o;
        }
    }
}