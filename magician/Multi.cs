/*
*  A Multi exists as a point (two quantities), and as a list of constituent Multis that exist relative to the parent
*/
using static SDL2.SDL;

namespace Magician
{
    public class Multi : Quantity, Drawable, Driveable
    {
        //protected double[] pos = new double[]{0,0};
        protected Quantity x = new Quantity(0);
        protected Quantity y = new Quantity(0);

        public Drawable this[int key]
        {
            get => constituents[key];
            set => constituents[key] = value;
        }
        public Quantity X
        {
            get => x;
            set => x = value;
        }
        public Quantity Y
        {
            get => y;
            set => y = value;
        }
        /*public Quantity Phase
        {
            get => 
        }*/
        protected List<Drawable> constituents;
        public IEnumerable<Drawable> Constituents(Func<double, double> truth, double truthThreshold=0)
        {
            for (int i = 0; i < constituents.Count; i++)
            {
                if (truth.Invoke(i) > truthThreshold)
                {
                    yield return constituents[i];
                }
                else
                {
                    continue;
                }
            }
        }
        public IEnumerable<Drawable> Constituents()
        {
            for (int i = 0; i < constituents.Count; i++)
                {
                    yield return constituents[i];
                }
        }
        // If true, a Multi will be drawn with its constituents connected by lines
        // This is useful for plots, polygons, etc
        protected bool lined;
        
        // If true, the last constituent in a Multi will be drawn with a line connecting to the first one
        // This is desirable for say, a polygon, but undesirable for say, a plot
        protected bool linedCompleted;
        protected bool drawPoint;
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
        public Multi(double x, double y, Color col, bool lined, bool linedCompleted, bool drawPoint, params Multi[] cs) : this(cs)
        {
            //SetX(x);
            //SetY(y);
            this.x.Set(x);
            this.y.Set(y);
            this.col = col;
            this.lined = lined;
            this.linedCompleted = linedCompleted;
            this.drawPoint = drawPoint;
        }

        public Multi(double x, double y, Color col, params Multi[] cs) : this(x, y, col, true, false, false, cs) {}

        public double XAbsolute(double offset)
        {
            return x.Evaluate() + offset;
        }
        public double YAbsolute(double offset)
        {
            return y.Evaluate() + offset;
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
            this.x.Set(x - parentOffset);
        }
        public void SetY(double x)
        {
            double parentOffset = 0;
            if (Parent() is not null)
            {
                parentOffset = Parent().YAbsolute(0);
            }
            this.y.Set(x - parentOffset);
        }

        public int Count
        {
            get => constituents.Count;
        }
        public void Add(Drawable d)
        {
            constituents.Add(d);
        }

        public List<Driver> Drivers
        {
            get => drivers;
        }

        public bool Lined
        {
            set => lined = value;
        }

        public Multi Modify(List<Drawable> cs)
        {
            constituents = cs;
            return this;
        }

        public Multi Filter(Func<double, double> f, double thresh=0)
        {
            constituents = Constituents(f, thresh).ToList();
            return this;
        }

        public Multi Where(Func<Drawable, Drawable> nm)
        {
            for (int i = 0; i < Count; i++)
            {
                constituents[i] = nm(constituents[i]);
            }
            return this;
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
                    
                    SDL_RenderDrawLineF(renderer,
                    (float)p0.XCartesian(x.Evaluate()+xOffset), (float)p0.YCartesian(y.Evaluate()+yOffset),
                    (float)p1.XCartesian(x.Evaluate()+xOffset), (float)p1.YCartesian(y.Evaluate()+yOffset));

                }
                
                // Recursively draw the constituents
                Drawable d = constituents[i];
                d.Draw(ref renderer, xOffset+x.Evaluate(), yOffset+y.Evaluate());
            }
            
            if (linedCompleted && constituents.Count > 0)
            {
                Drawable pLast = constituents[constituents.Count-1].GetPoint();
                Drawable pFirst = constituents[0].GetPoint();
                
                SDL_SetRenderDrawColor(renderer, r, g, b, a);                
                SDL_RenderDrawLineF(renderer,
                (float)pLast.XCartesian(x.Evaluate()+xOffset), (float)pLast.YCartesian(y.Evaluate()+yOffset),
                (float)pFirst.XCartesian(x.Evaluate()+xOffset), (float)pFirst.YCartesian(y.Evaluate()+yOffset));
            }

            
            foreach (Drawable d in constituents)
            {
                // Make sure constituents are drawn relative to parent Multi
                d.Draw(ref renderer, xOffset+x.Evaluate(), yOffset+y.Evaluate());
            }

            if (drawPoint)
            {
                SDL_SetRenderDrawColor(renderer, r, g, b, a);
                //SDL_RenderDrawPoint(renderer, (int)((Drawable)this).XCartesian(xOffset), (int)((Drawable)this).YCartesian(yOffset));
                SDL_RenderDrawPointF(renderer, (float)((Drawable)this).XCartesian(xOffset), (float)((Drawable)this).YCartesian(yOffset));
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

        public Multi Eject()
        {
            drivers.Clear();
            foreach (Multi m in constituents)
            {
                m.Eject();
            }
            return this;
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
        public new Multi Driven(Func<double[], double> df)
        {
            Driver d = new Driver(df);
            AddDriver(d);
            return this;
        }
        public Multi Driven(Func<double[], double> xFunc, Func<double[], double> yFunc)
        {
            X = X.Driven(xFunc);
            Y = Y.Driven(yFunc);
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

        public Drawable Copy()
        {
            Multi copy = new Multi(x.Evaluate(), y.Evaluate(), col.Copy(), lined, linedCompleted, drawPoint);
            
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
            Multi innerCopy = (Multi)Copy();
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
            Multi copy = (Multi)Copy();
            copy.Scale(factor);
            return copy;
        }

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

        public Multi Prev()
        {
            Multi p = (Multi)parent;
            int i = p.constituents.IndexOf(this);
            if (i == -1)
            {
                throw new InvalidDataException($"{this} not found in parent {parent}");
            }
            Console.WriteLine($"Current: {this} is {p.constituents.IndexOf(this)} / {Count} in {p}");
            return (Multi)p.constituents[p.constituents.IndexOf(this) - 1];
        }
        public Multi Next()
        {
            Multi p = (Multi)parent;
            return (Multi)p.constituents[p.constituents.IndexOf(this) + 1];
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

            return new Multi(xOffset, yOffset, col, true, true, false, ps.ToArray());;
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

                /*
                case "NONE":
                    o = null;
                    break;*/
                
                default:
                    Console.WriteLine($"ERROR: Unknown driver string {s}");
                    throw new NotImplementedException();
            }
            return o;
        }
    }
}