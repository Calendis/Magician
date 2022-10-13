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
        public Multi? parent;
        protected List<Multi> constituents;

        public Multi this[int key]
        {
            get => constituents[key];
            set => constituents[key] = value;
        }
        /*
        *  Positional Properties and Methods
        */
        public Quantity X
        {
            get
            {
                if (parent is null)
                {
                    return x;
                }
                // Recursive getting of parent position
                return x.GetDelta(((Multi)parent).X.Evaluate());
            }
            
            set => x = value;
        }
        public void SetX(double offset)
        {
            x.Set(offset);
        }
        public void IncrX(double offset)
        {
            x.Delta(offset);
        }
        public Quantity Y
        {
            get
            {
                if (parent is null)
                {
                    return y;
                }
                // Recursive getting of parent position
                return y.GetDelta(((Multi)parent).Y.Evaluate());
            }
            set => y = value;
        }
        public void SetY(double offset)
        {
            y.Set(offset);
        }
        public void IncrY(double offset)
        {
            y.Delta(offset);
        }

        public Quantity Phase
        {
            get
            {
                double p = Math.Atan2(y.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return new Quantity(p);
            }
            set
            {
                SetPhase(value.Evaluate());
            }
        }
        public void SetPhase(double offset)
        {
            double m = Magnitude.Evaluate();
            x.Set(m*Math.Cos(offset));
            y.Set(m*Math.Sin(offset));
        }
        public void IncrPhase(double offset)
        {
            SetPhase(Phase.Evaluate() + offset);
        }
        public Quantity Magnitude
        {
            get => new Quantity(Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate()));
            set
            {
                SetMagnitude(value.Evaluate());
            }
        }
        public void SetMagnitude(double offset)
        {
            double p = Phase.Evaluate();
            SetX(offset*Math.Cos(p));
            SetY(offset*Math.Sin(p));
        }
        public void IncrMagnitude(double offset)
        {
            SetMagnitude(Magnitude.Evaluate() + offset);
        }

        public double XCartesian(double offset)
        {
            return Globals.winWidth / 2 + X.Evaluate(offset);
        }
        public double YCartesian(double offset)
        {
            return Globals.winHeight / 2 - Y.Evaluate(offset);
        }
        
        public IEnumerable<Multi> Constituents(Func<double, double> truth, double truthThreshold=0)
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
        
        // Full constructor
        public Multi(Multi parent, double x, double y, Color col, bool lined=false, bool linedCompleted=false, bool drawPoint=true, params Multi[] cs) : base(0)
        {
            this.parent = parent;
            this.x.Set(x);
            this.y.Set(y);
            this.col = col;
            this.lined = lined;
            this.linedCompleted = linedCompleted;
            this.drawPoint = drawPoint;

            constituents = new List<Multi> {};
            foreach (Multi c in cs)
            {
                Add(c);
            }
        }

        // Create a multi and define its position, colour, and drawing properties
        public Multi(double x, double y, Color col, bool lined=false, bool linedCompleted=false, bool drawPoint=true, params Multi[] cs)
        : this(Multi.Origin, x, y, col, lined, linedCompleted, drawPoint, cs) {}

        public Multi(double x, double y) : this(x, y, Globals.fgCol) {}

        // Create a multi from a list of multis
        public Multi(params Multi[] cs) : this(0, 0, Globals.fgCol, false, false, true, cs) {}

        public Color Col
        {
            get => col;
            set => col = value;
        }

        public int Count
        {
            get => constituents.Count;
        }
        public void Add(params Multi[] ms)
        {
            constituents.AddRange(ms);
            foreach (Multi m in ms)
            {
                //m.SetParent(this);
                m.parent = this;
            }
        }
        public void Remove(Multi m)
        {
            constituents.Remove(m);
        }

        public List<Driver> Drivers
        {
            get => drivers;
        }

        public bool Lined
        {
            set => lined = value;
        }

        public Multi LinedCompleted(bool b)
        {
            linedCompleted = b;
            return this;
        }

        public Multi Modify(List<Multi> cs)
        {
            constituents = cs;
            return this;
        }

        public Multi Filter(Func<double, double> f, double thresh=0)
        {
            constituents = Constituents(f, thresh).ToList();
            return this;
        }

        public Multi Where(Func<Multi, Multi> nm)
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
                    Drawable p0 = constituents[i];
                    Drawable p1 = constituents[i+1];
                    byte subr = p0.Col.R;
                    byte subg = p0.Col.G;
                    byte subb = p0.Col.B;
                    byte suba = 50;//p0.Col.A;
                    SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    
                    SDL_SetRenderDrawColor(renderer, subr, subg, subb, suba);
                    
                    
                    SDL_RenderDrawLineF(renderer,
                    (float)p0.XCartesian(xOffset), (float)p0.YCartesian(yOffset),
                    (float)p1.XCartesian(xOffset), (float)p1.YCartesian(yOffset));

                }
                
                // Recursively draw the constituents
                Drawable d = constituents[i];
                d.Draw(ref renderer, xOffset+x.Evaluate(), yOffset+y.Evaluate());
            }
            
            if (linedCompleted && constituents.Count > 0)
            {
                Drawable pLast = constituents[constituents.Count-1];
                Drawable pFirst = constituents[0];

                byte subr = pLast.Col.R;
                byte subg = pLast.Col.G;
                byte subb = pLast.Col.B;
                byte suba = pLast.Col.A;
                
                SDL_SetRenderDrawColor(renderer, subr, subg, subb, suba);                
                SDL_RenderDrawLineF(renderer,
                (float)pLast.XCartesian(xOffset), (float)pLast.YCartesian(yOffset),
                (float)pFirst.XCartesian(xOffset), (float)pFirst.YCartesian(yOffset));
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
                SDL_RenderDrawPointF(renderer, (float)XCartesian(xOffset), (float)YCartesian(yOffset));
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
            x.Driven(xFunc);
            y.Driven(yFunc);
            return this;
        }

        public Multi SubDriven(Func<double[], double> df, string s)
        {
            if (Count == 0)
            {
                Console.WriteLine("WARNING: subdriven had no effect!");
            }
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
            Console.WriteLine("Begincopy");
            Console.WriteLine($"Copying {this}");
            Multi copy = new Multi(x.Evaluate(), y.Evaluate(), col.Copy(), lined, linedCompleted, drawPoint);
            
            // Copy the drivers
            Console.WriteLine("   Copying the drivers...");
            for (int i = 0; i < drivers.Count; i++)
            {
                copy.drivers.Add(drivers[i].CopiedTo(copy));
            }
            Console.WriteLine("....Done");

            // Copy the constituents
            Multi[] cs = new Multi[Count];
            for (int i = 0; i < Count; i++)
            {
                cs[i] = constituents[i].Copy();
            }
            copy.Add(cs);
            
            return copy;
        }

        // Wield is a form of recursion where each constituent is replaced with a copy of the given Multi
        public Multi Wielding(Multi outer)
        {
            for (int i = 0; i < Count; i++)
            {
                // Make a copy of the outer Multi and position it against the inner Multi
                Multi outerCopy = outer.Copy();
                outerCopy.SetX(constituents[i].x.Evaluate());
                outerCopy.SetY(constituents[i].y.Evaluate());
                
                Multi c = constituents[i];
                constituents[i] = outerCopy;
            }

            return this;
        }
        public Multi Wielding(Multi outer, Func<Multi, Multi> F)
        {
            return Wielding(F(outer));
        }

        
        // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
        public Multi Surrounding(Multi inner)
        {
            return inner.Wielding(this);
            //thisSurroundingInner.x.Set(x.Evaluate());
            //thisSurroundingInner.y.Set(y.Evaluate());
            //return thisSurroundingInner;//.Wielding(this);
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

        public Multi Scaled(double factor)
        {
            foreach (Multi c in constituents)
            {
                c.SetMagnitude(c.Magnitude.Evaluate() * factor);
                c.Scaled(factor);
            }
            return this;
        }

        public Multi Invisible()
        {
            lined = false;
            linedCompleted = false;
            return this;
        }
        
        /*
        public void SetParent(Multi m)
        {
            parent = m;
        }
        */

        public int Index
        {
            get => parent.constituents.IndexOf(this);
        }
        public double Normal
        {
            get => (double)Index / parent.Count;
        }
        public Multi Prev()
        {
            Multi p = parent;
            int i = Index;
            if (i == -1)
            {
                throw new InvalidDataException($"{this} not found in parent {parent}");
            }
            i = i == 0 ? p.Count - 1 : i - 1;
            return (Multi)p.constituents[i];
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
                    s += $"Pair Multi ({constituents[0]}, {constituents[1]})";
                    break;
                default:
                    s += $"{Count}-Multi (";
                    foreach (Multi m in constituents)
                    {
                        s += m.ToString() + "), ";
                    }
                    break;
            }
            if (this is null)
            {
                s += "nothing";
            }
            else if (parent is null)
            {
                s += $" Origin at {x.Evaluate()}, {y.Evaluate()}";
            }
            else
            {
                s += $" at ({x.Evaluate()}, {y.Evaluate()}) relative, ({X.Evaluate()} {Y.Evaluate()}) absolute";
            }
            return s;
        }

        /*
        *  Common types of Multis you might want to create
        */
        public static Multi Origin = Point(null, 0, 0, Globals.fgCol);
        public static Multi Point(Multi parent, double x, double y, Color col)
        {
            return new Multi(parent, x, y, col);
        }
        public static Multi Point(double x, double y, Color col)
        {
            return new Multi(x, y, col);
        }
        public static Multi Point(double x, double y)
        {
            return Point(x, y, Globals.fgCol);
        }
        // Create a regular polygon with a position, number of sides, color, and magnitude
        public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
        {
            List<Multi> ps = new List<Multi>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude*Math.Cos(angle*i/180*Math.PI);
                double y = magnitude*Math.Sin(angle*i/180*Math.PI);
                ps.Add(Point(x, y, col));
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

        public static Action<double> StringMap(Multi m, string s)
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
                    o = ((Drawable)m).SetCol0;
                    break;
                
                case "COL1":
                    o = ((Drawable)m).SetCol1;
                    break;

                case "COL2":
                    o = ((Drawable)m).SetCol2;
                    break;

                case "COL3":
                    o = ((Drawable)m).SetAlpha;
                    break;

                case "COL0+":
                    o = ((Drawable)m).IncrCol0;
                    break;
                
                case "COL1+":
                    o = ((Drawable)m).IncrCol1;
                    break;

                case "COL2+":
                    o = ((Drawable)m).IncrCol2;
                    break;

                case "COL3+":
                    o = ((Drawable)m).IncrAlpha;
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