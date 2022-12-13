/*
*  A Multi exists as a point (two quantities), and as a list of constituent Multis that exist relative to the parent
*/
using System.Collections;
using static SDL2.SDL;

namespace Magician
{
    public enum DrawMode
    {
        INVISIBLE = (short)0b0000,
        OUTER = (short)0b1100,
        INNER = (short)0b0010,
        FULL = (short)0b1110,
        PLOT = (short)0b1000,
        POINT = (short)0b0001
    }

    public class Multi : Quantity, Drawable, Driveable, ICollection<Multi>
    {
        Quantity x = new Quantity(0);
        Quantity y = new Quantity(0);
        public Multi? parent;
        // Multis are recursive, positioned relative to the parent
        List<Multi> constituents;
        public Multi this[int key]
        {
            get => constituents[key];
            set => constituents[key] = value;
        }

        /*
        *  Positional Properties
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
                return x.GetDelta(parent.X.Evaluate());
            }

            set => x = value;
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
                return y.GetDelta(parent.Y.Evaluate());
            }
            set => y = value;
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
        public Quantity Magnitude
        {
            get => new Quantity(Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate()));
            set
            {
                ScaleTo(this, value.Evaluate());
            }
        }

        DrawMode drawMode;
        protected Color col;

        // Full constructor
        public Multi(Multi? parent, double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs) : base(0)
        {
            this.parent = parent;
            this.x.Set(x);
            this.y.Set(y);
            this.col = col;
            this.drawMode = dm;

            constituents = new List<Multi> { };
            foreach (Multi c in cs)
            {
                Add(c);
            }
        }

        // Create a multi and define its position, colour, and drawing properties
        public Multi(double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs)
        : this(Multi.Origin, x, y, col, dm, cs) { }
        public Multi(double x, double y) : this(x, y, Globals.fgCol) { }
        // Create a multi from a list of multis
        public Multi(params Multi[] cs) : this(0, 0, Globals.fgCol, DrawMode.FULL, cs) { }

        public Color Col
        {
            get => col;
            set => col = value;
        }

        public List<Driver> Drivers
        {
            get => drivers;
        }

        public Multi Modify(params Multi[] cs)
        {
            constituents.Clear();
            constituents.AddRange(cs);
            return this;
        }

        public void SetX(double offset)
        {
            SetX(this, offset);
        }
        public void IncrX(double offset)
        {
            Translate(this, offset, 0);
        }
        public void SetY(double offset)
        {
            SetY(this, offset);
        }
        public void IncrY(double offset)
        {
            Translate(this, 0, offset);
        }
        public void SetPhase(double offset)
        {
            RotateTo(this, offset);
        }
        public void IncrPhase(double offset)
        {
            Rotate(this, offset);
        }
        public void SetMagnitude(double offset)
        {
            ScaleTo(this, offset);
        }
        public void IncrMagnitude(double offset)
        {
            ScaleShift(this, offset);
        }

        public double XCartesian(double offset)
        {
            return Globals.winWidth / 2 + X.Evaluate(offset);
        }
        public double YCartesian(double offset)
        {
            return Globals.winHeight / 2 - Y.Evaluate(offset);
        }

        public Multi Positioned(double x, double y)
        {
            SetX(this, x);
            SetY(this, y);
            return this;
        }

        public Multi Rotated(double theta)
        {
            Rotate(this, theta);
            return this;
        }

        public Multi Scaled(double mag)
        {
            Scale(this, mag);
            return this;
        }

        public Multi Colored(Color c)
        {
            SetColor(this, c);
            return this;
        }


        public new void Go(params double[] x)
        {
            foreach (Driver d in drivers)
            {
                d.Go(x);
            }
            foreach (Multi c in constituents)
            {
                c.Go(x);
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

        public Multi Driven(Func<double[], double> df, string s)
        {
            Drive(this, df, s);
            return this;
        }
        public new Multi Driven(Func<double[], double> df)
        {
            Drive(this, new Driver(df));
            return this;
        }
        public Multi Driven(Func<double[], double> xFunc, Func<double[], double> yFunc)
        {
            x.Driven(xFunc);
            y.Driven(yFunc);
            return this;
        }

        public Multi Copy()
        {
            Multi copy = new Multi(x.Evaluate(), y.Evaluate(), col.Copy(), drawMode);
            copy.parent = parent;

            // Copy the drivers
            for (int i = 0; i < drivers.Count; i++)
            {
                copy.drivers.Add(drivers[i].CopiedTo(copy));
            }
            constituents.ForEach(m => m.Copy());
            copy.Add(constituents.ToArray());

            return copy;
        }

        // Create a new Multi with the constituents of both Multis
        public Multi FlatAdjoin(Multi m)
        {
            constituents.AddRange(m.constituents);
            return this;
        }

        // Add both multis to a new parent Multi
        public Multi Adjoin(Multi m, double xOffset = 0, double yOffset = 0)
        {
            Multi nm = new Multi(xOffset, yOffset);
            nm.Add(this, m);
            return nm;
        }

        public Multi Where(Func<Multi, bool> predicate)
        {
            return Modify(constituents.Where(predicate).ToList());
        }

        public Multi Sub(Action<Multi> action, Func<double, double>? truth=null, double threshold=0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            foreach (Multi c in this)
            {
                if (truth.Invoke(c.Index) >= threshold)
                {
                    action.Invoke(c);
                }
            }
            return this;
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
        public Multi Surrounding(Multi inner, Func<Multi, Multi> F)
        {
            return Surrounding(F(inner));
        }

        public Multi Recursed()
        {
            return Wielding(Copy());
        }
        public Multi Recursed(Func<Multi, Multi> F)
        {
            return Wielding(F.Invoke(Copy()));
        }

        public Multi SubScaled(double factor)
        {
            foreach (Multi c in constituents)
            {
                Scale(c, factor);
            }
            return this;
        }

        public Multi Invisible()
        {
            drawMode = DrawMode.INVISIBLE;
            return this;
        }

        public Multi DrawFlags(DrawMode dm)
        {
            drawMode = dm;
            return this;
        }
        public int Index
        {
            get
            {
                if (parent is null)
                {
                    return 0;
                }
                return parent.constituents.IndexOf(this);
            }
        }
        // TODO: rename this
        public double Normal
        {
            get
            {
                if (parent is null)
                {
                    return 0;
                }
                return (double)Index / parent.Count;
            }
        }
        public int Count => constituents.Count;
        public bool IsReadOnly => false;

        public Multi Prev()
        {
            if (parent is null)
            {
                return this;
            }
            Multi p = parent;
            int i = Index;
            i = i == 0 ? p.Count - 1 : i - 1;
            return p.constituents[i];
        }
        public Multi Next()
        {
            if (parent is null)
            {
                return this;
            }
            Multi p = parent;
            int i = Index;
            i = i == p.Count - 1 ? 0 : i + 1;
            return p.constituents[i];
        }
        public void Draw(ref IntPtr renderer, double xOffset = 0, double yOffset = 0)
        {
            double r = col.R;
            double g = col.G;
            double b = col.B;
            double a = col.A;

            // For each constituent, draw something!
            for (int i = 0; i < constituents.Count - 1; i++)
            {
                // If lined, draw lines between the constituents as if they were vertices in a polygon
                if ((drawMode & DrawMode.PLOT) > 0)
                {
                    Drawable p0 = constituents[i];
                    Drawable p1 = constituents[i + 1];
                    double subr = p0.Col.R;
                    double subg = p0.Col.G;
                    double subb = p0.Col.B;
                    double suba = p0.Col.A;

                    SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                    SDL_SetRenderDrawColor(renderer, (byte)subr, (byte)subg, (byte)subb, (byte)suba);
                    SDL_RenderDrawLineF(renderer,
                    
                    (float)p0.XCartesian(xOffset), (float)p0.YCartesian(yOffset),
                    (float)p1.XCartesian(xOffset), (float)p1.YCartesian(yOffset));

                }

                // Recursively draw the constituents
                Multi d = this[i];
                d.Draw(ref renderer, ((Multi)d).X.Evaluate(xOffset), ((Multi)d).Y.Evaluate(yOffset));
            }

            if (((drawMode & DrawMode.FULL) > DrawMode.PLOT) && constituents.Count > 0)
            {
                Drawable pLast = constituents[constituents.Count - 1];
                Drawable pFirst = constituents[0];

                double subr = pLast.Col.R;
                double subg = pLast.Col.G;
                double subb = pLast.Col.B;
                double suba = pLast.Col.A;

                SDL_SetRenderDrawColor(renderer, (byte)subr, (byte)subg, (byte)subb, (byte)suba);
                SDL_RenderDrawLineF(renderer,
                (float)pLast.XCartesian(xOffset), (float)pLast.YCartesian(yOffset),
                (float)pFirst.XCartesian(xOffset), (float)pFirst.YCartesian(yOffset));
            }


            foreach (Drawable d in constituents)
            {
                //d.Draw(ref renderer, xOffset, yOffset);
                d.Draw(ref renderer, ((Multi)d).X.Evaluate(xOffset), ((Multi)d).Y.Evaluate(yOffset));
            }

            if ((drawMode & DrawMode.POINT) > 0)
            {
                SDL_SetRenderDrawColor(renderer, (byte)r, (byte)g, (byte)b, (byte)a);
                //SDL_RenderDrawPoint(renderer, (int)((Drawable)this).XCartesian(xOffset), (int)((Drawable)this).YCartesian(yOffset));
                SDL_RenderDrawPointF(renderer, (float)XCartesian(xOffset), (float)YCartesian(yOffset));
            }

            // And finally
            if (((drawMode & DrawMode.INNER) > 0) && Count >= 3)
            {
                /* Entering the wild and wacky world of the Renderer! Prepare to crash */
                try
                {
                    List<int[]> vertices = Renderer.Geo.Triangulate(this);
                    int numTriangles = (Count - 2);
                    SDL_Vertex[] vs = new SDL_Vertex[numTriangles * 3];
                    // Assemble the triangles from the renderer into vertices for SDL
                    for (int i = 0; i < numTriangles; i++)
                    {
                        int[] vertexIndices = vertices[i];
                        int tri0 = vertexIndices[0];
                        int tri1 = vertexIndices[1];
                        int tri2 = vertexIndices[2];
                        // If all vertex indices are 0, we're done

                        if ((vertexIndices[0] + vertexIndices[1] + vertexIndices[2] == 0))
                            continue;


                        SDL_FPoint p0, p1, p2;
                        //p.x = (float)constituents[i].XCartesian
                        //p.y = (float)constituents[i].YCartesian(yOffset);
                        p0.x = (float)constituents[tri0 - 1].XCartesian(xOffset);
                        p0.y = (float)constituents[tri0 - 1].YCartesian(yOffset);
                        p1.x = (float)constituents[tri1 - 1].XCartesian(xOffset);
                        p1.y = (float)constituents[tri1 - 1].YCartesian(yOffset);
                        p2.x = (float)constituents[tri2 - 1].XCartesian(xOffset);
                        p2.y = (float)constituents[tri2 - 1].YCartesian(yOffset);

                        vs[3 * i] = new SDL_Vertex();
                        vs[3 * i].position.x = p0.x;
                        vs[3 * i].position.y = p0.y;
                        vs[3 * i + 1] = new SDL_Vertex();
                        vs[3 * i + 1].position.x = p1.x;
                        vs[3 * i + 1].position.y = p1.y;
                        vs[3 * i + 2] = new SDL_Vertex();
                        vs[3 * i + 2].position.x = p2.x;
                        vs[3 * i + 2].position.y = p2.y;

                        SDL_Color c;
                        c.r = (byte)Col.R;
                        c.g = (byte)Col.G;
                        c.b = (byte)Col.B;
                        c.a = (byte)Col.A;

                        // Randomly-coloured triangles for debugging
                        /*
                        Random rnd = new Random(i);
                        byte rndRed = (byte)rnd.Next(256);
                        byte rndGrn = (byte)rnd.Next(256);
                        byte rndBlu = (byte)rnd.Next(256);
                        c.r = rndRed;
                        c.g = rndGrn;
                        c.b = rndBlu;
                        c.a = (byte)Col.A;
                        */

                        vs[3 * i].color = c;
                        vs[3 * i + 1].color = c;
                        vs[3 * i + 2].color = c;
                    }

                    IntPtr ip = new IntPtr();
                    SDL_RenderGeometry(renderer, ip, vs, vs.Length, null, 0);
                }
                catch (System.Exception)
                {
                    Console.WriteLine($"Bad Omen: failed to render {this}");
                    //throw;
                }

            }

        }

        public override string ToString()
        {
            string s = "";
            switch (Count)
            {
                case (0):
                    s += "Empty Multi";
                    break;
                case (1):
                    s += "Lonely Multi";
                    break;
                case (2):
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
                s += $" at ({x.Evaluate()}, {y.Evaluate()}) relative";
            }
            return s;
        }

        /*
        *  Common types of Multis you might want to create
        */

        // The Origin is the eventual parent Multi for all Multis
        public static Multi Origin = Point(null, 0, 0, Globals.fgCol).DrawFlags(DrawMode.INVISIBLE);

        // Create a point
        public static Multi Point(Multi? parent, double x, double y, Color col)
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
        // Create a line
        public static Multi Line(Multi p1, Multi p2, Color col)
        {
            double x1 = p1.x.Evaluate();
            double y1 = p1.y.Evaluate();
            double x2 = p2.x.Evaluate();
            double y2 = p2.y.Evaluate();

            return new Multi(x1, y1, col, DrawMode.PLOT,
            Point(0, 0, col),
            Point(x2 - x1, y2 - y1, col));
        }
        public static Multi Line(Multi p1, Multi p2)
        {
            return Line(p1, p2, Globals.fgCol);
        }
        // Create a regular polygon with a position, number of sides, color, and magnitude
        public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
        {
            List<Multi> ps = new List<Multi>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude * Math.Cos(angle * i / 180 * Math.PI);
                double y = magnitude * Math.Sin(angle * i / 180 * Math.PI);
                ps.Add(Point(x, y, col));
            }

            return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Globals.fgCol, sides, magnitude);
        }
        public static Multi RegularPolygon(int sides, double magnitude)
        {
            return RegularPolygon(0, 0, sides, magnitude);
        }

        public static Multi Star(double xOffset, double yOffset, Color col, int sides, double innerRadius, double outerRadius)
        {
            List<Multi> ps = new List<Multi>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double innerX = innerRadius * Math.Cos(angle * i / 180 * Math.PI);
                double innerY = innerRadius * Math.Sin(angle * i / 180 * Math.PI);
                double outerX = outerRadius * Math.Cos((angle * i + angle / 2) / 180 * Math.PI);
                double outerY = outerRadius * Math.Sin((angle * i + angle / 2) / 180 * Math.PI);
                ps.Add(Point(innerX, innerY, col));
                ps.Add(Point(outerX, outerY, col));
            }

            return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        }
        public static Multi Star(double xOffset, double yOffset, int sides, double innerRadius, double outerRadius)
        {
            return Star(xOffset, yOffset, Globals.fgCol, sides, innerRadius, outerRadius);
        }
        public static Multi Star(int sides, double innerRadius, double outerRadius)
        {
            return Star(0, 0, sides, innerRadius, outerRadius);
        }

        public static Action<double> StringMap(Multi m, string s)
        {
            Action<double> o;
            s = s.ToUpper();
            switch (s)
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
                case "R":
                    o = ((Drawable)m).SetR;
                    break;

                case "G":
                    o = ((Drawable)m).SetG;
                    break;
                case "B":
                    o = ((Drawable)m).SetB;
                    break;
                case "A":
                    o = ((Drawable)m).SetA;
                    break;
                case "H":
                    o = ((Drawable)m).SetH;
                    break;
                case "S":
                    o = ((Drawable)m).SetS;
                    break;
                case "L":
                    o = ((Drawable)m).SetL;
                    break;
                case "R+":
                    o = ((Drawable)m).IncrR;
                    break;
                case "G+":
                    o = ((Drawable)m).IncrG;
                    break;
                case "B+":
                    o = ((Drawable)m).IncrB;
                    break;
                case "A+":
                    o = ((Drawable)m).IncrA;
                    break;
                case "H+":
                    o = ((Drawable)m).IncrH;
                    break;
                case "S+":
                    o = ((Drawable)m).IncrS;
                    break;
                case "L+":
                    o = ((Drawable)m).IncrL;
                    break;

                default:
                    throw new NotImplementedException($"Unknown driver string {s}");
            }
            return o;
        }

        /*
        * static void Multi methods
        */
        public static void SetX(Multi m, double x)
        {
            m.x.Set(x);
        }
        public static void SetY(Multi m, double y)
        {
            m.y.Set(y);
        }

        public static void Translate(Multi m, double x, double y)
        {
            m.x.Incr(x);
            m.y.Incr(y);
        }

        public static void RotateTo(Multi m, double theta)
        {
            double mag = m.Magnitude.Evaluate();
            m.x.Set(mag * Math.Cos(theta));
            m.y.Set(mag * Math.Sin(theta));
        }
        
        public static void Rotate(Multi m, double theta)
        {
            RotateTo(m , theta+m.Phase.Evaluate());
        }

        public static void ScaleTo(Multi m, double mag)
        {
            double ph = m.Phase.Evaluate();
            SetX(m, mag*Math.Cos(ph));
            SetY(m, mag*Math.Sin(ph));
        }

        public static void ScaleShift(Multi m, double mag)
        {
            ScaleTo(m, mag+m.Magnitude.Evaluate());
        }

        public static void Scale(Multi m, double mag)
        {
            ScaleTo(m, mag*m.Magnitude.Evaluate());
        }

        public static void Drive(Multi m, Driver d)
        {
            m.AddDriver(d);
        }
        public static void Drive(Multi m, Func<double[], double> df, string s)
        {
            Action<double> output = StringMap(m, s);
            Driver d = new Driver(df, output);
            d.ActionString = s;
            Drive(m, d);
        }

        public static void SetColor(Multi m, Color c)
        {
            m.Col = c;
        }

        // Interface methods

        public IEnumerator<Multi> GetEnumerator()
        {
            return ((IEnumerable<Multi>)constituents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)constituents).GetEnumerator();
        }

        public void Add(Multi item)
        {
            constituents.Add(item);
        }
        public void Add(params Multi[] items)
        {
            constituents.AddRange(items);
        }

        public void Clear()
        {
            constituents.Clear();
        }

        public bool Contains(Multi item)
        {
            return constituents.Contains(item);
        }

        public void CopyTo(Multi[] array, int arrayIndex)
        {
            constituents.CopyTo(0, array, arrayIndex, Math.Min(array.Length, Count));
        }

        public bool Remove(Multi item)
        {
            return constituents.Remove(item);
        }

    }

}