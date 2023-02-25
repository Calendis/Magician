/*
*  A Multi exists as a point (two quantities), and as a list of constituent Multis that exist relative to the parent
*/
using System.Collections;
using System.Runtime.Serialization;
using Magician.Geo;
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
        public Quantity x = new Quantity(0);
        public Quantity y = new Quantity(0);
        public Quantity z = new Quantity(0);
        double tempX = 0;
        double tempY = 0;
        Multi? _parent;
        /* Multis are a recursive structure. They track position relative to their parent */
        List<Multi> csts;
        Dictionary<string, Multi> tags = new Dictionary<string, Multi>();

        /* parent property */
        public Multi Parent
        {
            get
            {
                if (IsOrphan())
                {
                    Scribe.Warn($"Orphan detected {tag}");
                }
                return _parent!;
            }
        }

        /*
        *  Positional Properties
        */
        Quantity RecursX
        {
            get
            {
                // Base case (top of the tree)
                if (this == Ref.Origin)
                {
                    return x;
                }

                // Recurse up the tree of Multis to find your position relative to the origin
                return x.GetDelta(Parent.RecursX.Evaluate());
            }
        }
        Quantity RecursY
        {
            get
            {
                // Base case (top of the tree)
                if (this == Ref.Origin)
                {
                    return y;
                }
                // Recurse up the tree of Multis to find your position relative to the origin
                return y.GetDelta(Parent.RecursY.Evaluate());
            }
        }
        Quantity RecursZ
        {
            get
            {
                if (this == Ref.Origin)
                {
                    return z;
                }
                return z.GetDelta(Parent.RecursZ.Evaluate());
            }
        }
        // Big X is the x-position relative to (0, 0)
        public double X
        {
            get => RecursX.Evaluate();
        }
        // Big Y is the Y-position relative to (0, 0)
        public double Y
        {
            get => RecursY.Evaluate();
        }
        // Big Z is the z-position relative to (0, 0)
        public double Z
        {
            get => RecursZ.Evaluate();
        }
        // These values are set by drivers
        public double LastX { get => tempX; }
        public double LastY { get => tempY; }

        // Phase, relative to the parent
        public double Phase
        {
            get
            {
                double p = Math.Atan2(y.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        // Magnitude, relative to the parent
        public double Magnitude
        {
            get => new Quantity(Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate())).Evaluate();
        }

        /* NEVER REASSIGN A MULTI VARIABLE LIKE THIS:    */
        ///////////////////////////////////////////////////
        // Multi m = (blah...)
        // loop {
        //      m = (blah...);
        // }
        /* This will cause a memory when using textures. */
        /* INSTEAD, USE THIS SETTER! It disposes of all  */
        /* textures and handily sets the parent too!     */
        public Multi this[int i]
        {
            get
            {
                if (i >= Count)
                {
                    throw new IndexOutOfRangeException($"Tried to get index {i} of {this}");
                }
                return csts[i];
            }
            set
            {
                if (i >= Count)
                {
                    throw new IndexOutOfRangeException($"Tried to get index {i} of {this}");
                }
                csts[i].DisposeAllTextures();
                csts[i] = value.Parented(this);
            }
        }
        public Multi this[string tag]
        {
            get
            {
                if (tags.ContainsKey(tag))
                {
                    return tags[tag];
                }
                throw new KeyNotFoundException($"tag {tag} does not exist in {this}");
            }
            set
            {
                // Create new Multi associated with the tag
                if (!tags.ContainsKey(tag))
                {
                    //Scribe.Info($"Creating tag \"{tag}\"");
                    tags.Add(tag, value);
                    Add(value.Tagged(tag));
                    return;
                }

                // Destroy the old Multi, and tag the new one with the same tag
                //Scribe.Info($"Overwriting tag \"{tag}\"");
                tags[tag].DisposeAllTextures();
                Remove(tags[tag]);
                tags[tag] = value;
                Add(value);
            }
        }

        DrawMode drawMode;
        protected Color col;
        // TODO: maybe make this not public
        public Texture? texture;

        // Full constructor
        public Multi(Multi? parent, double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs) : base(0)
        {
            this._parent = parent ?? Ref.Origin;
            this.x.Set(x);
            this.y.Set(y);
            this.col = col;
            this.drawMode = dm;

            csts = new List<Multi> { };
            foreach (Multi c in cs)
            {
                Add(c);
            }
        }

        // Create a multi and define its position, colour, and drawing properties
        public Multi(double x, double y, Color col, DrawMode dm = DrawMode.FULL, params Multi[] cs)
        : this(Ref.Origin, x, y, col, dm, cs) { }
        public Multi(double x, double y) : this(x, y, Data.Col.UIDefault.FG) { }
        // Create a multi from a list of multis
        public Multi(params Multi[] cs) : this(0, 0, Data.Col.UIDefault.FG, DrawMode.FULL, cs) { }

        public Color Col
        {
            get => col;
        }

        public Multi Become(Multi m)
        {
            Clear();
            foreach (Multi c in m)
            {
                Add(c);
            }
            return Colored(m.Col).DrawFlags(m.drawMode);
        }

        public double XCartesian(double offset)
        {
            return Data.Globals.winWidth / 2 + X + offset;
        }
        public double YCartesian(double offset)
        {
            return Data.Globals.winHeight / 2 - Y + offset;
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
        public static void _SetZ(Multi m, double z)
        {
            m.z.Set(z);
        }
        public Multi AtZ(double offset)
        {
            _SetZ(this, offset);
            return this;
        }

        public static void _Translate(Multi m, double x, double y, double? z = null)
        {
            // x, y, and z are Quantities, so increment them like this
            m.x.Incr(x);
            m.y.Incr(y);
            if (z != null)
            {
                m.z.Incr((double)z);
            }
        }
        public Multi Translated(double x, double y, double? z=null)
        {
            _Translate(this, x, y, z);
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
        public Multi ZShifted(double offset)
        {
            _Translate(this, 0, 0, offset);
            return this;
        }
        public Multi Positioned(double x, double y, double? z = null)
        {
            _SetX(this, x);
            _SetY(this, y);
            if (z != null)
            {
                _SetZ(this, (double)z);
            }
            return this;
        }
        public Multi Positioned(Matrix mx)
        {
            double pz = z.Evaluate();
            if (mx.width == 3)
            {
                pz = mx.Get(0, 2);
            }
            return Positioned(mx.Get(0, 0), mx.Get(0, 1), pz);
        }

        /* Rotation methods */
        public static void _RevolveTo(Multi m, double theta)
        {
            double mag = m.Magnitude;
            m.x.Set(mag * Math.Cos(theta));
            m.y.Set(mag * Math.Sin(theta));
        }
        public static void _RevolveBy(Multi m, double theta)
        {
            _RevolveTo(m, theta + m.Phase);
        }
        public Multi Revolved(double theta)
        {
            _RevolveBy(this, theta);
            return this;
        }
        public Multi RevolvedTo(double offset)
        {
            _RevolveTo(this, offset);
            return this;
        }
        // Rotation, in terms of revolution
        public Multi Rotated(double theta)
        {
            return Sub(
                m =>
                m.Revolved(theta)
            );
        }

        /* Scaling methods */
        public static void _AbsoluteScale(Multi m, double mag)
        {
            double ph = m.Phase;
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
            _AbsoluteScale(this, mag + this.Magnitude);
            return this;
        }

        // Scale is implemented in terms of absolute scale
        public static void _Scale(Multi m, double mag)
        {
            _AbsoluteScale(m, mag * m.Magnitude);
        }
        public Multi Scaled(double mag)
        {
            _Scale(this, mag);
            return this;
        }

        public static void _Texture(Multi m, Renderer.Texture t)
        {
            if (m.texture != null)
            {
                Scribe.Info("Overwriting texture");
                m.texture.Dispose();
            }
            else
            {
                //Scribe.Info("Setting new texture");
            }
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

        public virtual void Update()
        {
            //
        }

        /* Driving methods */
        // Activates all the drivers
        public void Drive(params double[] ds)
        {
            Update();
            if (ds.Length < 2)
            {
                ds = new double[] { 0, 0 };
            }
            double xOffset = ds[0];
            double yOffset = ds[1];
            /*                                          TODO:                                                    */
            // All driving should be done concurrently, so we need to traverse the tree and collect all the drivers
            foreach (IDriveable c in csts)
            {
                // Pass the offsets to subdriving
                c.Drive(xOffset, yOffset);
            }


            int count = x.GetDrivers().Count;
            if (count != y.GetDrivers().Count)
            {
                throw new InvalidDataException("TODO: implement a fix for this");
            }
            // This drives x and y. If the driving IMap is flagged as absolute, we offset the
            // result by the parent position. This corrects the offset and allows a user to
            // easily drive multis based on their children
            for (int i = 0; i < count; i++)
            {
                IMap xDriver = x.GetDrivers()[i];
                IMap yDriver = y.GetDrivers()[i];

                // null checking parent all the time is really boring so I had some fun with it
                // TODO: add support for absolute phase driving as well. IsAbs will need to be
                // replaced with a reference to an offset value
                double xResult = xDriver.Evaluate(x.Evaluate()) - (_parent is null ? 0 : ((xDriver.IsAbs ? 1 : 0) * _parent.X));
                double yResult = yDriver.Evaluate(y.Evaluate()) - (_parent is null ? 0 : ((yDriver.IsAbs ? 1 : 0) * _parent.Y));

                x.Set(xResult);
                y.Set(yResult);
            }
            //x.Delta(-tempX);
            //y.Delta(-tempY);
            tempX = X;
            tempY = Y;
        }

        // Remove all the drivers
        public new void Eject()
        {
            x.Eject();
            y.Eject();
        }
        public Multi Ejected()
        {
            Eject();
            return this;
        }
        public Multi DrivenXY(IMap im0, IMap im1)
        {
            x.Driven(im0);
            y.Driven(im1);
            return this;
        }
        public Multi DrivenXY(Func<double, double> f0, Func<double, double> f1)
        {
            return DrivenXY(new CustomMap(f0), new CustomMap(f1));
        }
        public Multi DrivenPM(IMap imPh, IMap imMg)
        {
            // Driving of phase
            x.Driven(x => Magnitude * Math.Cos(imPh.Evaluate(Phase)));
            y.Driven(y => Magnitude * Math.Sin(imPh.Evaluate(Phase)));

            // Driving of magnitude
            x.Driven(x => imMg.Evaluate(Magnitude) * Math.Cos(Phase));
            y.Driven(y => imMg.Evaluate(Magnitude) * Math.Sin(Phase));
            return this;
        }
        public Multi DrivenPM(Func<double, double> fPh, Func<double, double> fMg)
        {
            return DrivenPM(new CustomMap(fPh), new CustomMap(fMg));
        }
        public Multi DrivenAbs(IMap im0, IMap im1)
        {
            if (_parent is null)
            {
                return DrivenXY(im0, im1);
            }
            x.Driven(im0.AsAbsolute());
            y.Driven(im1.AsAbsolute());
            return this;
        }
        public Multi DrivenAbs(Func<double, double> f0, Func<double, double> f1)
        {
            return DrivenAbs(new CustomMap(f0), new CustomMap(f1));
        }

        public Multi DrivenRGBA(Func<double, double> r, Func<double, double> g, Func<double, double> b, Func<double, double> a)
        {
            throw new NotImplementedException("DrivenRGBA not supported yet");
        }


        /* Internal state methods */
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
        public static void _IndexConstituents(Multi m)
        {
            for (int i = 0; i < m.Count; i++)
            {
                m.csts[i].index = i;
            }
        }

        /* Parenting/tagging methods */
        static void _Parent(Multi m, Multi p)
        {
            m._parent = p;
        }
        public Multi Parented(Multi m)
        {
            _Parent(this, m);
            return this;
        }
        static bool _IsOrphan(Multi m)
        {
            if (m == Ref.Origin) { return false; }
            if (m._parent == null) { return true; }
            return false;
        }
        public bool IsOrphan()
        {
            return _IsOrphan(this);
        }

        static void _Tag(Multi m, string tag)
        {
            m.tag = tag;
        }
        public Multi Tagged(string tag)
        {
            _Tag(this, tag);
            return this;
        }

        // Create a copy of the Multi
        public Multi Copied()
        {
            Multi copy = new Multi(x.Evaluate(), y.Evaluate(), col.Copy(), drawMode);
            // Don't copy the texture, or create reference to it!
            //copy.texture = texture;

            // Copy the drivers
            // TODO: fix this
            //x.TransferDrivers(copy.x);
            //y.TransferDrivers(copy.y);

            copy.x = new Quantity(x);
            copy.y = new Quantity(y);

            foreach (IMap d in x.GetDrivers())
            {
                copy.x.Driven(d);
            }
            foreach (IMap d in y.GetDrivers())
            {
                copy.y.Driven(d);
            }

            // Copy the constituents
            foreach (Multi c in this)
            {
                copy.Add(c.Copied());
            }

            return copy;
        }
        /* The two paste methods must match!! */
        public Multi Paste()
        {
            Geo.Ref.Origin[$"{tag}_paste{x}{y}"] = Copied();
            return this;
        }
        public Multi Pasted()
        {
            Paste();
            return Geo.Ref.Origin[$"{tag}_paste{x}{y}"];
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
            m.Translated(csts[n].X, csts[n].Y);
            csts[n] = m;
        }

        // Add both multis to a new parent Multi
        public Multi Adjoin(Multi m, double xOffset = 0, double yOffset = 0)
        {
            Multi nm = new Multi(xOffset, yOffset);
            nm.Add(this, m);
            return nm;
        }

        public Multi Sub(Action<Multi> action, Func<double, double>? truth = null, double threshold = 0)
        {
            return Sub((x, _i) => action(x), truth, threshold);
        }

        public Multi Sub(Action<Multi, int> action, Func<double, double>? truth = null, double threshold = 0)
        {
            int i = 0;
            foreach (Multi c in this)
            {
                int index = i;
                if (truth == null || truth.Invoke(i) > threshold)
                {
                    action.Invoke(c, i);
                }
                i++;
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
            Eject();
            for (int i = 0; i < Count; i++)
            {
                Multi outerCopy = outer.Copied();
                csts[i].Become(outerCopy);
            }

            return this;
        }

        // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
        public Multi Surrounding(Multi inner)
        {
            Eject();
            return inner.Wielding(Copied());
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
            return Wielding(Copied());
        }
        public Multi Recursed(Func<Multi, Multi> F)
        {
            return Wielding(F.Invoke(Copied()));
        }

        /* Getter roperties for indices and tags */
        int? index = null;
        string tag = "";
        public int Index
        {
            get
            {
                // If the index is null, it means it hasn't been indexed yet ...
                // ... so we ask the parent to distribute indices to all children
                if (index is null)
                {
                    // 
                    if (this == Geo.Ref.Origin)
                    {
                        Scribe.Warn("Getting index of Origin");
                        return -1;
                    }

                    Scribe.Info($"{this.Parent} is distributing indices...");
                    _IndexConstituents(Parent);
                    return (int)index!;  // just learned about this bad boy
                }
                return (int)index;
            }
        }

        // TODO: rename this
        public double Normal
        {
            get => (double)Index / Parent.Count;
        }
        public string Tag
        {
            get => tag;
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

        // Do we really need these?
        /* public Multi Prev()
        {
            int i = Index;
            i = i == 0 ? Parent.Count - 1 : i - 1;
            return Parent[i];
        }
        public Multi Next()
        {
            int i = Index;
            i = i == Parent.Count - 1 ? 0 : i + 1;
            return Parent[i];
        } */
        public void Draw(double xOffset, double yOffset)
        {
            Control.SaveTarget();
            SDL_SetRenderTarget(SDLGlobals.renderer, SDLGlobals.renderedTexture);
            double r = col.R;
            double g = col.G;
            double b = col.B;
            double a = col.A;

            // If the flag is set, draw the relative origin
            if ((drawMode & DrawMode.POINT) > 0)
            {
                SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)r, (byte)g, (byte)b, (byte)a);
                //SDL_RenderDrawPointF(SDLGlobals.renderer, (float)XCartesian(xOffset), (float)YCartesian(yOffset));
                //SDL_RenderDrawPointF(SDLGlobals.renderer, (float)XCartesian(0), (float)YCartesianz(0));
                if (_parent != null)
                {
                    SDL_RenderDrawPointF(SDLGlobals.renderer, (float)_parent.XCartesian(xOffset), (float)_parent.YCartesian(yOffset));
                }
            }

            // If lined, draw lines between the constituents as if they were vertices in a polygon
            for (int i = 0; i < csts.Count - 1; i++)
            {
                if ((drawMode & DrawMode.PLOT) > 0)
                {
                    Multi lineP0 = csts[i];
                    Multi lineP1 = csts[i + 1];

                    double subr = lineP0.Col.R;
                    double subg = lineP0.Col.G;
                    double subb = lineP0.Col.B;
                    double suba = lineP0.Col.A;

                    SDL_SetRenderDrawColor(SDLGlobals.renderer, (byte)subr, (byte)subg, (byte)subb, (byte)suba);
                    SDL_RenderDrawLineF(SDLGlobals.renderer,
                    (float)lineP0.XCartesian(xOffset), (float)lineP0.YCartesian(yOffset),
                    (float)lineP1.XCartesian(xOffset), (float)lineP1.YCartesian(yOffset));

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
                m.Draw(xOffset, yOffset);//), (m.Y.Evaluate(yOffset)));
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
                            break;


                        SDL_FPoint p0, p1, p2;
                        //Matrix projection = new Matrix();
                        //Matrix mx0 = projection.Mult(new Matrix())
                        //Matrix mx1 = projection.Mult(new Matrix())
                        //Matrix mx2 = projection.Mult(new Matrix())
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
                    Scribe.Warn($"Failed to render {this}");
                    Scribe.Warn(" You may have forgotten to set draw flags. Falling back to OUTERP...");
                    DrawFlags(DrawMode.OUTERP);
                }

            }

            // If not null, draw the texture
            if (texture != null)
            {
                texture.Draw(XCartesian(xOffset), YCartesian(yOffset));
            }
            Control.RecallTarget();

        }

        /* ToString override */
        public string Title()
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
                    s += $"{Count}-Multi";
                    break;
            }
            if (Tag != "")
            {
                s += $" \"{Tag}\"";
            }

            return s;
        }
        public override string ToString()
        {
            return ToString();
        }
        public string ToString(int depth = 1, bool verbose = false)
        {
            string s = Title(); ;

            string xAbs = X.ToString("F1");
            string xRel = x.Evaluate().ToString("F1");
            string yAbs = Y.ToString("F1");
            string yRel = y.Evaluate().ToString("F1");
            s += $" at ({xRel}, {yRel})rel, ({xAbs}, {yAbs})abs";

            foreach (Multi m in csts)
            {
                s += "\n";
                for (int i = 0; i <= depth; i++)
                {
                    s += " ";
                }
                // Trim excessive output
                if (!verbose)
                {
                    if (s.Split('\n', 16).ToList<string>().Count >= 16)
                    {
                        return s + $"... (trimmed output of {Title()})";
                    }
                }
                s += m.ToString(depth + 2);
            }

            return s;
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
            if (item == this)
            {
                throw new InvalidDataException($"A Multi may not have itself as a consituent! Offending Multi: {this}, belonging to {Parent}");
            }
            item._parent = this;
            csts.Add(item);
        }
        public Multi Add(params Multi[] items)
        {
            foreach (Multi m in items)
            {
                Add(m);
            }
            return this;
        }

        public void Clear()
        {
            foreach (Multi c in csts)
            {
                c.DisposeAllTextures();
            }
            csts.Clear();
        }

        public bool Contains(Multi item)
        {
            return csts.Contains(item);
        }

        // Some interface method
        public void CopyTo(Multi[] array, int arrayIndex)
        {
            csts.CopyTo(0, array, arrayIndex, Math.Min(array.Length, Count));
        }

        public bool Remove(Multi item)
        {
            return csts.Remove(item);
        }

        public void DisposeAllTextures()
        {
            if (texture != null)
            {
                texture.Dispose();
            }
            foreach (Multi m in this)
            {
                m.DisposeAllTextures();
            }
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