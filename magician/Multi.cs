/*
*  A Multi exists as a point (two quantities), and as a list of constituent Multis that exist relative to the parent
*/
using System.Collections;
using System.Runtime.Serialization;
using Magician.Renderer;
using static SDL2.SDL;

namespace Magician
{
    public enum DrawMode
    {
        INVISIBLE = (short)0b0000,
        PLOT = (short)0b1000,
        CONNECTED = (short)0b0100,
        INNER = (short)0b0010,
        POINT = (short)0b0001,
        OUTER = (short)0b1100,
        FULL = (short)0b1110,
        OUTERP = (short)0b1101
    }

    public class Multi : Quantity, IDriveable, ICollection<Multi>
    {
        Quantity x = new Quantity(0);
        Quantity y = new Quantity(0);
        public Multi? parent;
        // Multis are recursive, positioned relative to the parent
        public List<Multi> csts;

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
        }
        // TODO: experiment with making these Objects that typecheck for double and Quantity
        public Quantity Phase
        {
            get
            {
                double p = Math.Atan2(y.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return new Quantity(p);
            }
        }
        public Quantity Magnitude
        {
            get => new Quantity(Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate()));
        }

        DrawMode drawMode;
        protected Color col;
        public Texture? texture;

        // Full constructor
        public Multi(Multi? parent, double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs) : base(0)
        {
            this.parent = parent;
            this.x.Set(x);
            this.y.Set(y);
            this.col = col;
            this.drawMode = dm;

            csts = new List<Multi> { };
            foreach (Multi c in cs)
            {
                Add(c.Copy());
            }
        }

        // Create a multi and define its position, colour, and drawing properties
        public Multi(double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs)
        : this(Geo.Origin, x, y, col, dm, cs) { }
        public Multi(double x, double y) : this(x, y, Ref.UIDefault.FG) { }
        // Create a multi from a list of multis
        public Multi(params Multi[] cs) : this(0, 0, Ref.UIDefault.FG, DrawMode.FULL, cs) { }

        public Color Col
        {
            get => col;
        }

        public List<Driver> Drivers
        {
            get => drivers;
        }

        public Multi Modify(params Multi[] cs)
        {
            csts.Clear();
            csts.AddRange(cs);
            return this;
        }

        public double XCartesian(double offset)
        {
            return Ref.winWidth / 2 + x.Evaluate(offset);
        }
        public double YCartesian(double offset)
        {
            return Ref.winHeight / 2 - y.Evaluate(offset);
        }


        /* Colour methods */
        public static void _Color(Multi m, Color c)
        {
            m.col = c;
        }
        public Multi Colored(Color c)
        {
            _Color(this, c);
            return this;
        }
        public Multi R(double r)
        {
            Col.R = r;
            return this;
        }
        public Multi G(double g)
        {
            Col.G = g;
            return this;
        }
        public Multi B(double b)
        {
            Col.B = b;
            return this;
        }
        public Multi A(double b)
        {
            Col.A = b;
            return this;
        }
        public Multi H(double h)
        {
            Col.H = h;
            return this;
        }
        public Multi S(double s)
        {
            Col.S = s;
            return this;
        }
        public Multi L(double l)
        {
            Col.L = l;
            return this;
        }
        public Multi RShifted(double r)
        {
            Col.R += r;
            return this;
        }
        public Multi GShifted(double g)
        {
            Col.G += g;
            return this;
        }
        public Multi BShifted(double b)
        {
            Col.B += b;
            return this;
        }
        public Multi AShifted(double b)
        {
            Col.A += b;
            return this;
        }
        public Multi HShifted(double h)
        {
            Col.H += h;
            return this;
        }
        public Multi SShifted(double s)
        {
            Col.S += s;
            return this;
        }
        public Multi LShifted(double l)
        {
            Col.L += l;
            return this;
        }

        /* Translation methods */
        public static void _SetX(Multi m, double x)
        {
            // x is is stored as a Quantity object, so set it like this
            m.x.Set(x);
        }
        public Multi AtX(double offset)
        {
            _SetX(this, offset);
            return this;
        }
        public static void _SetY(Multi m, double y)
        {
            // y is is stored as a Quantity object, so set it like this
            m.y.Set(y);
        }
        public Multi AtY(double offset)
        {
            _SetY(this, offset);
            return this;
        }

        public static void _Translate(Multi m, double x, double y)
        {
            // x and y are Quantities, so increment them like this
            m.x.Incr(x);
            m.y.Incr(y);
        }
        public Multi Translated(double x, double y)
        {
            _Translate(this, x, y);
            return this;
        }
        public Multi XShifted(double offset)
        {
            _Translate(this, offset, 0);
            return this;
        }
        public Multi YShifted(double offset)
        {
            _Translate(this, 0, offset);
            return this;
        }
        public Multi Positioned(double x, double y)
        {
            _SetX(this, x);
            _SetY(this, y);
            return this;
        }

        /* Rotation methods */
        public static void _RotateTo(Multi m, double theta)
        {
            double mag = m.Magnitude.Evaluate();
            m.x.Set(mag * Math.Cos(theta));
            m.y.Set(mag * Math.Sin(theta));
        }
        public static void _RotateBy(Multi m, double theta)
        {
            _RotateTo(m, theta + m.Phase.Evaluate());
        }
        public Multi Rotated(double theta)
        {
            _RotateBy(this, theta);
            return this;
        }
        public Multi RotatedTo(double offset)
        {
            _RotateTo(this, offset);
            return this;
        }

        /* Scaling methods */
        public static void _AbsoluteScale(Multi m, double mag)
        {
            double ph = m.Phase.Evaluate();
            _SetX(m, mag * Math.Cos(ph));
            _SetY(m, mag * Math.Sin(ph));
        }
        public Multi AbsoluteScaled(double mag)
        {
            _AbsoluteScale(this, mag);
            return this;
        }
        public Multi AbsoluteScaleShifted(double mag)
        {
            _AbsoluteScale(this, mag + this.Magnitude.Evaluate());
            return this;
        }

        // Scale is implemented in terms of absolute scale
        public static void _Scale(Multi m, double mag)
        {
            _AbsoluteScale(m, mag * m.Magnitude.Evaluate());
        }
        public Multi Scaled(double mag)
        {
            _Scale(this, mag);
            return this;
        }


        // TODO: Multi methods should follow this format, where static methods follow an underscore
        public static void _Texture(Multi m, Renderer.Texture t)
        {
            m.texture = t;
        }
        public Multi Textured(Renderer.Texture t)
        {
            _Texture(this, t);
            return this;
        }

        /* Transformation methods */
        public static void Affine(double[,] matrix)
        {
            // TODO: implement me
        }
        public static void Affine(double[] matrix)
        {
            // TODO: implement me
        }

        /* Driving methods */
        // Add a driver to a Multi
        public static void _Drive(Multi m, Driver d)
        {
            m.AddDriver(d);
        }
        // Add a driver to a Multi given a double[] => double, and a string for output mapping
        public static void _Drive(Multi m, Func<double[], double> df, string s)
        {
            Func<double, Multi> output = StringMap(m, s);
            Driver d = new Driver(df, output);
            d.ActionString = s;
            _Drive(m, d);
        }
        // Add a driver to a Multi given an IMap
        public static void _Drive(Multi m, IMap df, string? s = null)
        {
            Func<double, Multi>? output = null;
            if (s != null)
            {
                output = StringMap(m, s);
            }
            Driver d = new Driver(df, output);
            if (s != null)
            {
                d.ActionString = s;
            }
            _Drive(m, d);
        }
        public Multi Driven(Func<double[], double> df, string s)
        {
            _Drive(this, df, s);
            return this;
        }
        public new Multi Driven(Func<double[], double> df)
        {
            _Drive(this, new Driver(df));
            return this;
        }
        public Multi Driven(Func<double[], double> xFunc, Func<double[], double> yFunc)
        {
            x.Driven(xFunc);
            y.Driven(yFunc);
            return this;
        }
        public Multi Driven(IMap df, string? s = null)
        {
            _Drive(this, df, s);
            return this;
        }

        public static void _Write(Multi m, double d)
        {
            m.q = d;
        }
        public Multi Written(double d)
        {
            _Write(this, d);
            return this;
        }

        public Multi DrawFlags(DrawMode dm)
        {
            drawMode = dm;
            return this;
        }

        // Indexes the constituents of a Multi in the internal values of the constituents
        // This is useful because getting the index using IndexOf is too expensive
        public static void CreateIndex(Multi m)
        {
            for (int i = 0; i < m.Count; i++)
            {
                m.csts[i].index = i;
            }
        }


        // Activates all the drivers
        public new void Go(params double[] t)
        {
            foreach (Driver d in drivers)
            {
                d.Go(t);
            }
            foreach (Multi c in csts)
            {
                c.Go(t);
            }
            // Automatically drive internal quantities
            x.Go(t);
            y.Go(t);
        }

        // Remove all the drivers
        public Multi Ejected()
        {
            drivers.Clear();
            return this;
        }

        // Create a copy of the Multi
        public Multi Copy()
        {
            Multi copy = new Multi(x.Evaluate(), y.Evaluate(), col.Copy(), drawMode);
            copy.parent = parent;
            copy.texture = texture;

            // Copy the drivers
            for (int i = 0; i < drivers.Count; i++)
            {
                copy.drivers.Add(drivers[i].CopiedTo(copy));
            }
            // Copy the constituents
            foreach (Multi c in this)
            {
                copy.Add(c.Copy());
            }

            return copy;
        }

        // Create a new Multi with the constituents of both Multis
        public Multi FlatAdjoin(Multi m)
        {
            csts.AddRange(m.csts);
            return this;
        }

        // Flat adjoining on a particular constituent
        public void AddAt(Multi m, int n)
        {
            m.Translated(csts[n].X.Evaluate(), csts[n].Y.Evaluate());
            Add(m);
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
            return Modify(csts.Where(predicate).ToArray());
        }

        public Multi Sub(Action<Multi> action, Func<double, double>? truth = null, double threshold = 0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            foreach (Multi c in this)
            {
                if (c.index is null)
                {
                    CreateIndex(this);
                }
                if (truth.Invoke(c.Index) > threshold)
                {
                    action.Invoke(c);
                }
            }
            return this;
        }
        public Multi DeepSub(Action<Multi> action, Func<double, double>? truth = null, double threshold = 0)
        {
            Sub(action, truth, threshold);
            foreach (Multi c in this)
            {
                c.DeepSub(action, truth, threshold);
            }
            return this;
        }
        public Multi IterSub(int iters, Action<Multi> action, Func<double, double>? truth = null, double threshold = 0)
        {
            for (int i = 0; i < iters; i++)
            {
                Sub(action, truth, threshold);
            }
            return this;
        }

        // Wield is a form of recursion where each constituent is replaced with a copy of the given Multi
        public Multi Wielding(Multi outer)
        {
            for (int i = 0; i < Count; i++)
            {
                // Make a copy of the outer Multi and position it against the inner Multi
                Multi outerCopy = outer.Copy().Positioned(csts[i].X.Evaluate(), csts[i].Y.Evaluate());

                // Copy the drivers to the new multi
                foreach (Driver d in csts[i].drivers)
                {
                    outerCopy.AddDriver(d.CopiedTo(outerCopy));
                }

                csts[i] = outerCopy;
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

        int? index = null;
        public int Index
        {
            get
            {
                if (index is null)
                {
                    throw new NullIndexException($"{this} had null index! Did you remember to call CreateIndex on the parent Multi before getting the index?");
                }
                return (int)index;
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
        public int Count => csts.Count;
        public int DeepCount
        {
            get
            {
                int x = Count;
                foreach (Multi c in this)
                {
                    x += c.DeepCount;
                }
                return x;
            }
        }
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
            return p.csts[i];
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
            return p.csts[i];
        }
        public void Draw(double xOffset, double yOffset)
        {
            double r = col.R;
            double g = col.G;
            double b = col.B;
            double a = col.A;

            // If the flag is set, draw the relative origin
            //Console.WriteLine($"{drawMode} / {DrawMode.POINT}, c: {Count}");
            if ((drawMode & DrawMode.POINT) > 0)
            {
                SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)r, (byte)g, (byte)b, (byte)a);
                //SDL_RenderDrawPointF(SDLGlobals.renderer, (float)XCartesian(xOffset), (float)YCartesian(yOffset));
                //SDL_RenderDrawPointF(SDLGlobals.renderer, (float)XCartesian(0), (float)YCartesian(0));
                if (parent != null)
                {
                    SDL_RenderDrawPointF(SDLGlobals.renderer, (float)parent.XCartesian(xOffset), (float)parent.YCartesian(yOffset));
                }
            }

            // If lined, draw lines between the constituents as if they were vertices in a polygon
            for (int i = 0; i < csts.Count - 1; i++)
            {
                if ((drawMode & DrawMode.PLOT) > 0)
                {
                    Multi p0 = csts[i];
                    Multi p1 = csts[i + 1];
                    double subr = p0.Col.R;
                    double subg = p0.Col.G;
                    double subb = p0.Col.B;
                    double suba = p0.Col.A;

                    SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)subr, (byte)subg, (byte)subb, (byte)suba);
                    SDL_RenderDrawLineF(SDLGlobals.renderer,
                    (float)p0.XCartesian(xOffset), (float)p0.YCartesian(yOffset),
                    (float)p1.XCartesian(xOffset), (float)p1.YCartesian(yOffset));

                }
            }

            // If the Multi is a closed shape, connect the first and last constituent with a line
            if ((drawMode & DrawMode.CONNECTED) > 0 && csts.Count > 0)
            {
                Multi pLast = csts[csts.Count - 1];
                Multi pFirst = csts[0];

                double subr = pLast.Col.R;
                double subg = pLast.Col.G;
                double subb = pLast.Col.B;
                double suba = pLast.Col.A;

                SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)subr, (byte)subg, (byte)subb, (byte)suba);
                SDL_RenderDrawLineF(SDLGlobals.renderer,
                (float)pLast.XCartesian(xOffset), (float)pLast.YCartesian(yOffset),
                (float)pFirst.XCartesian(xOffset), (float)pFirst.YCartesian(yOffset));
            }


            // Draw each constituent recursively            
            foreach (Multi m in this)
            {
                m.Draw((m.X.Evaluate(xOffset)), (m.Y.Evaluate(yOffset)));
                //m.Draw(X.Evaluate(xOffset), Y.Evaluate(yOffset));
            }

            // If the flag is set, and there are at least 3 constituents, fill the shape
            if (((drawMode & DrawMode.INNER) > 0) && Count >= 3)
            {
                /* Entering the wild and wacky world of the Renderer! Prepare to crash */
                try
                {
                    List<int[]> vertices = Seidel.Triangulator.Triangulate(this);
                    int numTriangles = (Count - 2);
                    SDL_Vertex[] vs = new SDL_Vertex[numTriangles * 3];
                    // Assemble the triangles from the SDLGlobals.renderer into vertices for SDL
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
                        p0.x = (float)csts[tri0 - 1].XCartesian(xOffset);
                        p0.y = (float)csts[tri0 - 1].YCartesian(yOffset);
                        p1.x = (float)csts[tri1 - 1].XCartesian(xOffset);
                        p1.y = (float)csts[tri1 - 1].YCartesian(yOffset);
                        p2.x = (float)csts[tri2 - 1].XCartesian(xOffset);
                        p2.y = (float)csts[tri2 - 1].YCartesian(yOffset);

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
                    SDL_RenderGeometry(SDLGlobals.renderer, ip, vs, vs.Length, null, 0);
                }
                catch (System.Exception)
                {
                    Console.WriteLine($"Failed to render {this}");
                    //throw;
                }

            }

            // If not null, draw the texture
            if (texture != null && parent != null)
            {
                texture.Draw(parent.XCartesian(xOffset), parent.YCartesian(yOffset));
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
                    s += "Multi";
                    break;
                default:
                    s += $"{Count}-Multi (";
                    foreach (Multi m in csts)
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
                s += $" ORIGIN at {x.Evaluate()}, {y.Evaluate()}";
            }
            else
            {
                s += $" at ({x.Evaluate()}, {y.Evaluate()}) relative";
            }
            return s;
        }

        // Allows you to easily drive position and colour
        public static Func<double, Multi> StringMap(Multi m, string s)
        {
            Func<double, Multi> o;
            s = s.ToUpper();
            switch (s)
            {
                case "X":
                    o = m.AtX;
                    break;
                case "X+":
                    o = m.XShifted;
                    break;
                case "Y":
                    o = m.AtY;
                    break;
                case "Y+":
                    o = m.YShifted;
                    break;
                case "PHASE":
                    o = m.RotatedTo;
                    break;
                case "PHASE+":
                    o = m.Rotated;
                    break;
                case "MAGNITUDE":
                    o = m.AbsoluteScaled;
                    break;
                case "MAGNITUDE+":
                    o = m.AbsoluteScaleShifted;
                    break;
                case "R":
                    o = m.R;
                    break;
                case "G":
                    o = m.G;
                    break;
                case "B":
                    o = m.B;
                    break;
                case "A":
                    o = m.A;
                    break;
                case "H":
                    o = m.H;
                    break;
                case "S":
                    o = m.S;
                    break;
                case "L":
                    o = m.L;
                    break;
                case "R+":
                    o = m.RShifted;
                    break;
                case "G+":
                    o = m.GShifted;
                    break;
                case "B+":
                    o = m.BShifted;
                    break;
                case "A+":
                    o = m.AShifted;
                    break;
                case "H+":
                    o = m.HShifted;
                    break;
                case "S+":
                    o = m.SShifted;
                    break;
                case "L+":
                    o = m.LShifted;
                    break;

                default:
                    throw new NotImplementedException($"Unknown driver string {s}");
            }
            return o;
        }

        // Interface methods

        public IEnumerator<Multi> GetEnumerator()
        {
            return ((IEnumerable<Multi>)csts).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)csts).GetEnumerator();
        }

        public void Add(Multi item)
        {
            csts.Add(item);
        }
        public void Add(params Multi[] items)
        {
            csts.AddRange(items);
        }

        public void Clear()
        {
            csts.Clear();
        }

        public bool Contains(Multi item)
        {
            return csts.Contains(item);
        }

        public void CopyTo(Multi[] array, int arrayIndex)
        {
            csts.CopyTo(0, array, arrayIndex, Math.Min(array.Length, Count));
        }

        public bool Remove(Multi item)
        {
            return csts.Remove(item);
        }

    }

    [Serializable]
    internal class NullIndexException : Exception
    {
        public NullIndexException()
        {
        }

        public NullIndexException(string? message) : base(message)
        {
        }

        public NullIndexException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NullIndexException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}