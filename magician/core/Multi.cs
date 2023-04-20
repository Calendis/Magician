/*
*  A Multi exists as a point (two quantities), and as a list of constituent Multis that exist relative to the parent
*/
using System.Collections;
using Magician.Geo;
using Magician.Renderer;
using Silk.NET.Maths;

namespace Magician;

public enum DrawMode : short
{
    INVISIBLE = 0b0000,
    PLOT = 0b1000,
    CONNECTINGLINE = 0b0100,
    FILLED = 0b0010,
    POINTS = 0b0001,
    OUTER = 0b1100,
    FULL = 0b1110,
    OUTERP = 0b1101
}

public class Multi : Vec, IDriveable, ICollection<Multi>
{
    Multi? _parent;
    protected List<Multi> csts;
    Dictionary<string, Multi> tags = new Dictionary<string, Multi>();
    Quantity[] headings = new Quantity[3] { new(0), new(0), new(0) };  // I only support 3 axes of rotation
    double internalVal = 0;
    double tempX = 0;
    double tempY = 0;
    // Keep references to the rendered RDrawables so they can be removed
    List<RDrawable> drawables = new List<RDrawable>();
    bool stale = true; // Does the Multi need to be re-rendered?

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

    public IReadOnlyList<Multi> Constituents
    {
        get => csts;
    }
    public DrawMode DrawFlags
    {
        get => drawMode;
    }

    /*
    *  Positional Properties
    */
    Quantity RecursX
    {
        get
        {
            // Base case (top of the tree)
            if (Ref.AllowedOrphans.Contains(this))
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
            if (Ref.AllowedOrphans.Contains(this))
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
            if (Ref.AllowedOrphans.Contains(this))
            {
                return z;
            }
            return z.GetDelta(Parent.RecursZ.Evaluate());
        }
    }
    /* Inherent phase, or "heading" */
    public double HeadingX { get => headings[0].Evaluate(); set => headings[0].Set(value); }
    public double HeadingY { get => headings[1].Evaluate(); set => headings[1].Set(value); }
    public double HeadingZ { get => headings[2].Evaluate(); set => headings[2].Set(value); }
    Quantity RecursHeadingX
    {
        get
        {
            if (Ref.AllowedOrphans.Contains(this))
            {
                return headings[0];
            }
            return headings[0].GetDelta(Parent.RecursHeadingX.Evaluate());
        }
    }
    Quantity RecursHeadingY
    {
        get
        {
            if (Ref.AllowedOrphans.Contains(this))
            {
                return headings[1];
            }
            return headings[1].GetDelta(Parent.RecursHeadingY.Evaluate());
        }
    }
    Quantity RecursHeadingZ
    {
        get
        {
            if (Ref.AllowedOrphans.Contains(this))
            {
                return headings[2];
            }
            return headings[2].GetDelta(Parent.RecursHeadingZ.Evaluate());
        }
    }
    // Theta_x
    public double thX => RecursHeadingX.Evaluate();
    public double thY => RecursHeadingY.Evaluate();
    public double thZ => RecursHeadingZ.Evaluate();

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
    // These values are set by the driving process
    public double LastX { get => tempX; }
    public double LastY { get => tempY; }

    /* USERS, NEVER REASSIGN A MULTI VARIABLE LIKE THIS: */
    ///////////////////////////////////////////////////
    // Multi m = (blah...)
    // loop {
    //      m = (blah...);
    // }
    /* This will cause a memory when using textures.     */
    /* INSTEAD, USE THIS SETTER! It disposes of all      */
    /* textures and handily sets the parent too!         */
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
            RDrawable.drawables.RemoveAll(rd => drawables.Contains(rd));
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
            tags[tag].DisposeAllTextures();
            RDrawable.drawables.RemoveAll(rd => drawables.Contains(rd));
            Remove(tags[tag]);
            tags[tag] = value;
            Add(value);
        }
    }

    protected _SDLTexture? texture;
    public _SDLTexture Texture
    {
        get => texture ?? throw Scribe.Error($"Got null texture of {this}");
    }

    protected DrawMode drawMode;
    protected Color col;

    // Full constructor
    public Multi(Multi? parent, double x, double y, double z, Color? col = null, DrawMode dm = DrawMode.FULL, params Multi[] cs) : base(3, 0, 0, 0)
    {
        this._parent = parent ?? Ref.Origin;
        this.x.Set(x);
        this.y.Set(y);
        this.z.Set(z);
        this.col = col ?? new RGBA(0xff00ffd0);
        this.drawMode = dm;

        csts = new List<Multi> { };
        foreach (Multi c in cs)
        {
            Add(c);
        }
    }

    // Create a multi and define its position, colour, and drawing properties
    public Multi(double x, double y, double z, Color? col, DrawMode dm = DrawMode.FULL, params Multi[] cs) : this(Ref.Origin, x, y, z, col, dm, cs) { }
    public Multi(double x, double y, Color? col, DrawMode dm = DrawMode.FULL, params Multi[] cs) : this(x, y, 0, col, dm, cs) { }
    public Multi(double x, double y, double z = 0) : this(x, y, z, Data.Col.UIDefault.FG) { }
    // Create a multi from a list of multis
    public Multi(params Multi[] cs) : this(0, 0, 0, Data.Col.UIDefault.FG, DrawMode.FULL, cs) { }

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
        return Colored(m.Col).WithFlags(m.drawMode);
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
    public Multi Translated(double x, double y, double? z = null)
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

    /* Rotation methods */
    public static void _RevolveToZ(Multi m, double theta)
    {
        double mag = m.MagnitudeZ;
        m.x.Set(mag * Math.Cos(theta));
        m.y.Set(mag * Math.Sin(theta));
    }
    public static void _RevolveZ(Multi m, double theta)
    {
        _RevolveToZ(m, theta + m.PhaseZ);
    }
    public Multi RevolvedZ(double theta)
    {
        _RevolveZ(this, theta);
        return this;
    }
    public Multi RevolvedToZ(double offset)
    {
        _RevolveToZ(this, offset);
        return this;
    }
    // Rotation, in terms of revolution
    public Multi RotatedZ(double theta)
    {
        HeadingZ += theta;
        return Sub(
            m =>
            m.RevolvedZ(theta)
        );
    }
    public static void _RevolveToY(Multi m, double theta)
    {
        double mag = m.MagnitudeY;
        m.x.Set(mag * Math.Cos(theta));
        m.z.Set(mag * Math.Sin(theta));
    }
    public static void _RevolveY(Multi m, double theta)
    {
        _RevolveToY(m, theta + m.PhaseY);
    }
    public Multi RevolvedY(double theta)
    {
        _RevolveY(this, theta);
        return this;
    }
    public Multi RevolvedToY(double offset)
    {
        _RevolveToY(this, offset);
        return this;
    }
    // Rotation, in terms of revolution
    public Multi RotatedY(double theta)
    {
        HeadingY += theta;
        return Sub(
            m =>
            m.RevolvedY(theta)
        );
    }
    public static void _RevolveToX(Multi m, double theta)
    {
        double mag = m.MagnitudeX;
        m.y.Set(mag * Math.Cos(theta));
        m.z.Set(mag * Math.Sin(theta));
    }
    public static void _RevolveX(Multi m, double theta)
    {
        _RevolveToX(m, theta + m.PhaseX);
    }
    public Multi RevolvedX(double theta)
    {
        _RevolveX(this, theta);
        return this;
    }
    public Multi RevolvedToX(double offset)
    {
        _RevolveToX(this, offset);
        return this;
    }
    // Rotation, in terms of revolution
    public Multi RotatedX(double theta)
    {
        HeadingX += theta;
        return Sub(
            m =>
            m.RevolvedX(theta)
        );
    }

    /* Scaling methods */
    public static void _AbsoluteScale(Multi m, double mag)
    {
        double ph = m.PhaseZ;
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
        _AbsoluteScale(this, mag + this.MagnitudeZ);
        return this;
    }

    // Scale is implemented in terms of absolute scale
    public static void _Scale(Multi m, double mag)
    {
        _AbsoluteScale(m, mag * m.MagnitudeZ);
    }
    public Multi Scaled(double mag)
    {
        _Scale(this, mag);
        return this;
    }

    /* public Multi TransformedPoint(Matrix mx)
    {
        Matrix result = new Matrix(this)
        .Mult(mx)
        ;
        return new Multi(result.Get(0,0), result.Get(1,1), result.Get(2,2));
    } */

    public static void _Texture(Multi m, Renderer._SDLTexture t)
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
    public Multi Textured(Renderer._SDLTexture t)
    {
        _Texture(this, t);
        return this;
    }

    public virtual void Update()
    {
        //
    }

    /* Driving methods */
    // Activates all the drivers
    public void DriveQuants(params double[] ds)
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
            c.DriveQuants(xOffset, yOffset);
        }


        int count = x.GetDrivers().Count;
        if (count != y.GetDrivers().Count)
        {
            throw new InvalidDataException("TODO: implement a fix for this");
        }
        // This drives x and y. If the driving IMap is flagged as absolute, we offset the
        // result by the parent position. This corrects the offset and allows a user to
        // easily drive multis based on their children
        // TODO: support z-driving
        double xResult = x.Evaluate();
        double yResult = y.Evaluate();
        //double zResult = z.Evaluate();                
        for (int i = 0; i < count; i++)
        {
            IMap xDriver = x.GetDrivers()[i];
            IMap yDriver = y.GetDrivers()[i];
            //IMap zDriver = z.GetDrivers()[i];

            // TODO: add support for absolute phase driving as well. IsAbs will need to be
            // replaced with a reference to an offset value
            double tempXResult = xResult;
            double tempYResult = yResult;
            xResult = xDriver.Evaluate(tempXResult) - (_parent is null ? 0 : ((xDriver.IsAbs ? 1 : 0) * _parent.X));
            yResult = yDriver.Evaluate(tempYResult) - (_parent is null ? 0 : ((yDriver.IsAbs ? 1 : 0) * _parent.Y));
            //zResult = yDriver.Evaluate(zResult) - (_parent is null ? 0 : ((yDriver.IsAbs ? 1 : 0) * _parent.Y));

            x.Set(xResult);
            y.Set(yResult);
            HeadingZ = PhaseZ;
        }
        //x.Delta(-tempX);
        //y.Delta(-tempY);
        tempX = X;
        tempY = Y;
    }

    // Remove all the drivers
    public void Eject()
    {
        x.Eject();
        y.Eject();
        z.Eject();
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
        x.Driven(x => MagnitudeZ * Math.Cos(imPh.Evaluate(PhaseZ)));
        y.Driven(y => MagnitudeZ * Math.Sin(imPh.Evaluate(PhaseZ)));

        // Driving of magnitude
        x.Driven(x => imMg.Evaluate(MagnitudeZ) * Math.Cos(PhaseZ));
        y.Driven(y => imMg.Evaluate(MagnitudeZ) * Math.Sin(PhaseZ));
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
        m.internalVal = d;
    }
    public Multi Written(double d)
    {
        _Write(this, d);
        return this;
    }
    public double Read()
    {
        return internalVal;
    }

    public Multi WithFlags(DrawMode dm)
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
    static void _Parent(Multi m, Multi? p)
    {
        m._parent = p;
    }
    public Multi Parented(Multi? m)
    {
        _Parent(this, m);
        return this;
    }
    static bool _IsOrphan(Multi m)
    {
        if (Ref.AllowedOrphans.Contains(m)) { return false; }
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
    public virtual Multi Copy()
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
            copy.Add(c.Copy());
        }

        // headings, internalval, tempx, tempy
        copy.headings[0].From(headings[0]);
        copy.headings[1].From(headings[1]);
        copy.headings[2].From(headings[2]);
        copy.internalVal = internalVal;
        copy.tempX = tempX;
        copy.tempY = tempY;

        return copy;
    }
    /* The two paste methods must match!! */
    public Multi Paste()
    {
        Parent[$"{tag}_paste{x}{y}"] = Copy();
        return this;
    }
    public Multi Pasted()
    {
        Paste();
        return Parent[$"{tag}_paste{x}{y}"];
    }

    public Multi Unique()
    {
        List<double> xs = new List<double>();
        List<double> ys = new List<double>();
        Multi c = new Multi().Positioned(x.Evaluate(), y.Evaluate(), z.Evaluate());
        foreach (Multi cst in csts)
        {
            bool addMe = true;
            // Check for this position
            for (int i = 0; i < xs.Count; i++)
            {
                if (xs[i] == cst.x.Evaluate() && ys[i] == cst.y.Evaluate())
                {
                    addMe = false;
                    break;
                }
            }
            if (addMe)
            {
                c.Add(cst);
            }
        }
        return c;
    }

    // Inherit the constituents of another multi
    public Multi FlatAdjoin(Multi m)
    {
        csts.AddRange(m.csts);
        return this;
    }

    // Replace a constituent
    public void AddAt(Multi m, int n)
    {
        m.Translated(csts[n].X, csts[n].Y);
        csts[n] = m;
    }

    // Add both multis to a new parent Multi
    public Multi Adjoined(Multi m, double xOffset = 0, double yOffset = 0, double zOffset = 0)
    {
        Multi nm = new Multi(xOffset, yOffset, zOffset, col, drawMode);
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
            Multi outerCopy = outer.Copy();
            csts[i].Become(outerCopy);
        }

        return this;
    }

    // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
    public Multi Surrounding(Multi inner)
    {
        Eject();
        return inner.Wielding(Copy());
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
                return (int)index!;
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

    // TODO: write a better comment
    public virtual void Render(double xOffset, double yOffset, double zOffset, bool scale3d = false)
    {
        if (stale)
        {
            //Scribe.Info($"cleaning stale");
            //RDrawable.drawables.RemoveAll(rd => drawables.Contains(rd));
            //drawables.Clear();
        }
        else
        {
            return;
        }

        double r = col.R;
        double g = col.G;
        double b = col.B;
        double a = col.A;

        // Get a projection of each constituent
        double[][] projectedVerts = new double[Count][];
        for (int i = 0; i < Count; i++)
        {
            double xp, yp, zp;
            xp = csts[i].X - Ref.Perspective.X;
            yp = csts[i].Y - Ref.Perspective.Y;
            zp = csts[i].Z - Ref.Perspective.Z;

            if (scale3d)
            {
                Matrix4X4<double> perspMat = Matrix4X4.CreatePerspectiveFieldOfView<double>(Ref.FOV / 360d * 2 * Math.PI, Data.Globals.winWidth / Data.Globals.winHeight, 0.1, 800.1);
                Vector4D<double> triVec = Vector4D<double>.Zero;
                triVec.X = xp / zp * Data.Globals.winWidth;
                triVec.Y = yp / zp * Data.Globals.winWidth;
                triVec.Z = -zp;
                triVec.W = 1;
                Vector4D<double> perspTriVec = Vector4D.Multiply<double>(triVec, perspMat);
                projectedVerts[i] = new double[]
                {
                    perspTriVec.X, perspTriVec.Y, perspTriVec.Z, perspTriVec.W
                };
            }
            else
            {
                projectedVerts[i] = new double[] { xp * 2, yp * 2, zp };
            }
        }

        // Draw each constituent recursively            
        foreach (Multi m in this)
        {
            m.Render(xOffset, yOffset, zOffset);
        }

        // Draw points
        if ((drawMode & DrawMode.POINTS) > 0)
        {
            int numPoints = Count;
            RPoint[] rPointArray = new RPoint[numPoints];
            RPoints rPoints;

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = new RPoint(projectedVerts[i][0], projectedVerts[i][1], projectedVerts[i][2],
                    csts[i].Col.R, csts[i].Col.G, csts[i].Col.B, 255);
            }
            rPoints = new(rPointArray);
            drawables.Add(rPoints);
            RDrawable.drawables.Add(rPoints);
        }

        // Draw lines
        if ((drawMode & DrawMode.PLOT) > 0)
        {
            bool connected = (drawMode & DrawMode.CONNECTINGLINE) > 0 && Count >= 3;
            int numLines = Count - (connected ? 0 : 1);
            if (numLines < 1)
                return;
            
            RLine[] rLineArray = new RLine[numLines];
            RLines rLines;

            for (int i = 0; i < Count - 1; i++)
            {
                double x0 = projectedVerts[i][0]; double x1 = projectedVerts[i + 1][0];
                double y0 = projectedVerts[i][1]; double y1 = projectedVerts[i + 1][1];
                double z0 = projectedVerts[i][2]; double z1 = projectedVerts[i + 1][2];
                rLineArray[i] = new RLine(x0, y0, z0, x1, y1, z1, csts[i].Col.R, csts[i].Col.G, csts[i].Col.B, csts[i].Col.A);
            }
            // If the Multi is a closed shape, connect the first and last constituent with a line
            if (connected && csts.Count >= 3)
            {
                double[] pLast = projectedVerts[csts.Count - 1];
                double[] pFirst = projectedVerts[0];

                double subr = csts[csts.Count - 1].Col.R;
                double subg = csts[csts.Count - 1].Col.G;
                double subb = csts[csts.Count - 1].Col.B;
                double suba = csts[csts.Count - 1].Col.A;

                rLineArray[rLineArray.Length - 1] = new RLine(pLast[0], pLast[1], pLast[2], pFirst[0], pFirst[1], pFirst[2], subr, subb, subg, suba);
            }
            rLines = new(rLineArray);
            drawables.Add(rLines);
            RDrawable.drawables.Add(rLines);
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        if (((drawMode & DrawMode.FILLED) > 0) && Count >= 3)
        {
            /* Entering the wild and wacky world of the Renderer! Prepare to crash */
            try
            {
                List<int[]> projectedTriangleVertices = Seidel.Triangulator.Triangulate(projectedVerts);
                // If the render fails for some reason, try with reverse order
                // This is a hack, but oh well. Maybe I should specify ccw or cw?
                if (projectedTriangleVertices[0][0] + projectedTriangleVertices[0][1] + projectedTriangleVertices[0][2] + projectedTriangleVertices[1][0] + projectedTriangleVertices[1][1] + projectedTriangleVertices[1][2] == 0)
                {
                    double[][] reverseProjectedVerts = new double[Count][];
                    for (int revI = 0; revI < Count; revI++)
                    {
                        reverseProjectedVerts[Count - revI - 1] = projectedVerts[revI];
                    }
                    projectedTriangleVertices = Seidel.Triangulator.Triangulate(reverseProjectedVerts);
                }

                int numTriangles = Count - 2;  // This is guaranteed by Seidel's algorithm
                RTriangle[] rTriArray = new RTriangle[numTriangles];
                RTriangles rTris;

                for (int i = 0; i < numTriangles; i++)
                {
                    int[] vertexIndices = projectedTriangleVertices[i];
                    int tri0 = vertexIndices[0];
                    int tri1 = vertexIndices[1];
                    int tri2 = vertexIndices[2];

                    // If all vertex indices are 0, we're done
                    if ((vertexIndices[0] + vertexIndices[1] + vertexIndices[2] == 0))
                        break;

                    RTriangle rTri = new(
                        projectedVerts[tri0 - 1][0], projectedVerts[tri0 - 1][1], projectedVerts[tri0 - 1][2],
                        projectedVerts[tri1 - 1][0], projectedVerts[tri1 - 1][1], projectedVerts[tri1 - 1][2],
                        projectedVerts[tri2 - 1][0], projectedVerts[tri2 - 1][1], projectedVerts[tri2 - 1][2],
                        Col.R, Col.G, Col.B, Col.A
                    );
                    rTriArray[i] = rTri;
                }

                rTris = new RTriangles(rTriArray);
                drawables.Add(rTris);
                RDrawable.drawables.Add(rTris);
            }
            catch (System.Exception)
            {
                if (drawMode == DrawMode.OUTERP)
                {
                    throw Scribe.Issue($"The triangulator has failed");
                }

                Scribe.Warn($"Failed to render {this}. Falling back to OUTERP");
                WithFlags(DrawMode.OUTERP);
            }

        }

        // If not null, draw the texture
        if (texture != null)
        {
            texture.Draw(XCartesian(xOffset), YCartesian(yOffset));
        }
    }

    string Title()
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
        string zAbs = Z.ToString("F1");
        string zRel = z.Evaluate().ToString("F1");
        s += $" at ({xRel},{yRel},{zRel})rel, ({xAbs},{yAbs},{zAbs})abs";

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
            throw Scribe.Error($"A Multi may not have itself as a consituent! Offending Multi: {this}, belonging to {Parent}");
        }
        if (item == Parent)
        {
            throw Scribe.Error("A Multi may not have its parent as a constituent");
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
    public void AddCautiously(Multi m)
    {
        // Don't add empty pains
        if (m.Tag == "empty paint")
        {
            return;
        }
        // If the Multi has a tag, add it through the tag system
        if (m.Tag != "")
        {
            this[m.Tag] = m;
            return;
        }
        Add(m);
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

    public Multi Reversed()
    {
        csts.Reverse();
        return this;
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