namespace Magician.Geo;
using Core;
using Core.Maps;
using Paint;

using System.Collections;
using Silk.NET.Maths;
using System.Numerics;


[Flags]
public enum DrawMode : short
{
    INVISIBLE = 0,
    POINTS = 1 << 0,
    INNER = 1 << 1,
    CONNECTINGLINE = 1 << 2,
    PLOT = 1 << 3,
    OUTER = PLOT | CONNECTINGLINE,
    FULL = PLOT | CONNECTINGLINE | INNER,
    OUTERP = OUTER | POINTS,
}

/* A Node is a drawable tree of 3-vectors (more Multis) */
public class Node : Vec3, ICollection<Node>
{
    // The origin will have a null parent
    Node? parent;
    protected List<Node> constituents;
    protected Mesh? faces;
    readonly Dictionary<string, Node> constituentTags = new();
    //public double pitch = 0; public double yaw = 0; public double roll = 0;
    public double Val { get; set; } = 0;
    // Keep references to the rendered RDrawables so they can be removed
    //public List<RDrawable> drawables = new();
    bool stale = true; // Does the Multi need to be re-rendered? (does nothing so far)
    List<Driver> drivers = new();

    public Node Parent
    {
        get
        {
            if (parent is null)
            {
                if (this == Ref.Origin)
                    throw Scribe.Error($"Cannot get parent of origin");
                throw Scribe.Error($"Orphan detected");
            }
            return parent;
        }
    }

    public IReadOnlyList<Node> Constituents
    {
        get => constituents;
    }
    public DrawMode DrawFlags
    {
        get => drawMode;
    }

    /*
    *  Positional Properties
    */
    //internal Vector3D<double> Heading
    //{
    //    get
    //    {
    //        //Matrix4X4<double> rotMat = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, roll);
    //        Vector3D<double> rotated = Vector3D.Transform(new Vector3D<double>(Ref.DefaultHeading.x.Get(), Ref.DefaultHeading.y.Get(), Ref.DefaultHeading.z.Get()), Rotation);
    //        return rotated;
    //    }
    //    // TODO: remove this setter
    //    set
    //    {
    //        pitch = -Math.Asin(-value.Y);
    //        yaw = -Math.Atan2(value.X, value.Z);
    //        roll = Math.Atan2(value.Y, Math.Sqrt(value.X * value.X + value.Z * value.Z));
    //    }
    //}
    internal Quaternion<double> Rotation = new(0, 0, 0, 1);
    internal Vector3D<double> Heading => Vector3D.Transform(new(Ref.DefaultHeading.x.Get(), Ref.DefaultHeading.y.Get(), Ref.DefaultHeading.z.Get()), Rotation);
    // TODO: clean up these methods
    double RecursX
    {
        get
        {
            // Base case (top of the tree)
            if (parent is null)
                return x.Get();
            // Recurse up the tree of Multis to find your position relative to the origin
            return x.Get() + Parent.RecursX;
        }
    }
    double RecursY
    {
        get
        {
            if (parent is null)
                return y.Get();
            return y.Get() + Parent.RecursY;
        }
    }
    double RecursZ
    {
        get
        {
            if (parent is null)
                return z.Get();
            return z.Get() + Parent.RecursZ;
        }
    }
    double RecursHeadingX
    {
        get
        {
            if (parent is null)
                return Heading.X;
            return x.Get() + Parent.RecursHeadingX;
        }
    }
    double RecursHeadingY
    {
        get
        {
            if (parent is null)
                return Heading.Y;
            return y.Get() + Parent.RecursHeadingY;
        }
    }
    double RecursHeadingZ
    {
        get
        {
            if (parent is null)
                return Heading.Z;
            return z.Get() + Parent.RecursHeadingZ;
        }
    }

    // Big X is the x-position relative to (0, 0)
    public double X
    {
        get => RecursX;
    }
    // Big Y is the Y-position relative to (0, 0)
    public double Y
    {
        get => RecursY;
    }
    // Big Z is the z-position relative to (0, 0)
    public double Z
    {
        get => RecursZ;
    }

    /* NEVER REASSIGN A MULTI VARIABLE LIKE THIS: */
    ///////////////////////////////////////////////////
    // Multi m = (blah...)
    // loop {
    //      m = (blah...);
    // }
    /* This will cause a memory when using textures.     */
    /* INSTEAD, USE THIS SETTER! It disposes of all      */
    /* textures and handily sets the parent too!         */
    public Node this[int i]
    {
        get
        {
            if (i >= Count) { throw new IndexOutOfRangeException($"Tried to get index {i} of {this}"); }
            return constituents[i];
        }
        set
        {
            if (i >= Count) { throw new IndexOutOfRangeException($"Tried to get index {i} of {this}"); }
            constituents[i].DisposeAllTextures();
            //Renderer.Drawables.RemoveAll(rd => drawables.Contains(rd));
            constituents[i] = value.Parented(this);
        }
    }
    public Node this[string tag]
    {
        get
        {
            if (constituentTags.ContainsKey(tag)) { return constituentTags[tag]; }
            throw new KeyNotFoundException($"tag {tag} does not exist in {this}");
        }
        set
        {
            // Create new Multi associated with the tag
            if (!constituentTags.ContainsKey(tag))
            {
                constituentTags.Add(tag, value);
                Add(value.Tagged(tag));
                return;
            }

            // Destroy the old Multi, and tag the new one with the same tag
            constituentTags[tag].DisposeAllTextures();
            //Renderer.Drawables.RemoveAll(rd => drawables.Contains(rd));
            Remove(constituentTags[tag]);
            constituentTags[tag] = value;
            Add(value);
        }
    }

    protected _SDLTexture? texture;
    public _SDLTexture Texture { get => texture ?? throw Scribe.Error($"Got null texture of {this}"); }
    protected DrawMode drawMode;
    protected Color col;

    // Full constructor
    public Node(Node? parent, double x, double y, double z, Color? col = null, DrawMode dm = DrawMode.FULL, params Node[] cs) : base(x, y, z)
    {
        this.parent = parent ?? Ref.Origin;
        this.x.Set(x);
        this.y.Set(y);
        this.z.Set(z);

        this.col = col ?? new RGBA(0xff00ffd0);
        this.drawMode = dm;

        constituents = new List<Node> { };
        foreach (Node c in cs)
        {
            Add(c);
        }
    }

    // Create a multi and define its position, colour, and drawing properties
    public Node(double x, double y, double z, Color? col, DrawMode dm = DrawMode.FULL, params Node[] cs) : this(Ref.Origin, x, y, z, col, dm, cs) { }
    public Node(double x, double y, Color? col, DrawMode dm = DrawMode.FULL, params Node[] cs) : this(x, y, 0, col, dm, cs) { }
    public Node(double x, double y, double z = 0) : this(x, y, z, Runes.Col.UIDefault.FG) { }
    // Create a multi from a list of multis
    public Node(params Node[] cs) : this(0, 0, 0, Runes.Col.UIDefault.FG, DrawMode.FULL, cs) { }

    /* Meshed constructors */
    public Node(double x, double y, double z, Mesh? mesh = null, Color? col = null, DrawMode dm = DrawMode.FULL, params Node[] points) : this(x, y, z, col, dm, points) { faces = mesh; }
    public Node(Node m, Mesh mesh) : this(m.x.Get(), m.y.Get(), m.z.Get(), mesh, m.Col, m.DrawFlags, m.Constituents.ToArray()) { }
    public Node(double x, double y, double z, Mesh mesh, params Node[] points) : this(x, y, z, mesh, null, DrawMode.FULL, points) { }

    public Color Col
    {
        get => col;
    }

    public Node Become(Node m)
    {
        Clear();
        foreach (Node c in m)
        {
            Add(c);
        }
        return Colored(m.Col).Flagged(m.drawMode);
    }


    public double XCartesian(double offset)
    {
        return Runes.Globals.winWidth / 2 + X + offset;
    }
    public double YCartesian(double offset)
    {
        return Runes.Globals.winHeight / 2 - Y + offset;
    }


    /* Colour methods */
    public Node Colored(Color c)
    {
        col = c;
        foreach (Node cst in Constituents)
        {
            cst.col = c;
        }
        return this;
    }
    public Node R(double r)
    {
        Col.R = r;
        return this;
    }
    public Node G(double g)
    {
        Col.G = g;
        return this;
    }
    public Node B(double b)
    {
        Col.B = b;
        return this;
    }
    public Node A(double b)
    {
        Col.A = b;
        return this;
    }
    public Node H(double h)
    {
        Col.H = h;
        return this;
    }
    public Node S(double s)
    {
        Col.S = s;
        return this;
    }
    public Node L(double l)
    {
        Col.L = l;
        return this;
    }

    public Node Translated(double xOffset, double yOffset, double zOffset = 0)
    {
        IVal.Add(x, xOffset, x);
        IVal.Add(y, yOffset, y);
        IVal.Add(z, zOffset, z);
        return this;
    }
    public Node To(double x, double y, double? z = null)
    {
        this.x.Set(x);
        this.y.Set(y);
        if (z != null)
        {
            this.z.Set((double)z);
        }
        return this;
    }
    public Node To(IVal mv)
    {
        if (mv.Dims > 3)
            throw Scribe.Error($"Could not move Multi to {mv}");
        double[] pos = new double[3];
        for (int i = 0; i < mv.Dims; i++)
        {
            pos[i] = mv.Get(i);
        }
        return To(pos[0], pos[1], pos[2]);
    }

    /* Rotation methods */
    public Node RotatedY(double theta)
    {
        //yaw = (yaw + theta) % (2 * Math.PI);
        //yaw += yaw > 0 ? 0 : 2 * Math.PI;
        Quaternion<double> yaw = Quaternion<double>.CreateFromYawPitchRoll(theta, 0, 0);
        Rotation *= yaw;
        return Sub(
            m =>
            m.PhaseXZ += theta
        );
    }
    public Node RotatedX(double theta)
    {
        //pitch = (pitch + theta);// % (2 * Math.PI);
        Quaternion<double> pitch = Quaternion<double>.CreateFromYawPitchRoll(0, theta, 0);
        Rotation *= pitch;
        return Sub(
            m =>
            m.PhaseYZ += theta
        );
    }
    public Node RotatedZ(double theta)
    {
        //roll = (roll + theta) % (2 * Math.PI);
        Quaternion<double> roll = Quaternion<double>.CreateFromYawPitchRoll(0, 0, theta);
        Rotation *= roll;
        return Sub(
            m =>
            m.PhaseXY += theta
        );
    }

    /* Scaling methods */
    public Node Scaled(double mag)
    {
        throw Scribe.Issue("TODO: re-implement scaling");
        //return this;
    }

    public static void _Texture(Node m, Paint._SDLTexture t)
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
    public Node Textured(Paint._SDLTexture t)
    {
        _Texture(this, t);
        return this;
    }

    public virtual void Update()
    {
        foreach (Driver d in drivers)
        {
            d.Drive(0);
        }
    }

    /*
        Custom driving for Multis, specified as parametric functions on XYZ, or magnitude, XY-angle, YZ-angle
        The driver can either set the valuese or just increment them, operating on either the target Multi or
        its constituents. 
     */
    public Node Driven(
        Func<double, double> f0, Func<double, double> f1, Func<double, double> f2,
        CoordMode cm = CoordMode.XYZ, DriverMode dm = DriverMode.SET, TargetMode tm = TargetMode.DIRECT)
    {
        Driver d = new(this, new Direct(f0), new Direct(f1), new Direct(f2), cm, dm, tm);
        drivers.Add(d);
        return this;
    }

    /*
        Movement methods 
    */
    public void Forward(double amount)
    {
        double newX = x.Get() + Heading.X * amount;
        double newY = y.Get() + Heading.Y * amount;
        double newZ = z.Get() + Heading.Z * amount;
        x.Set(newX);
        y.Set(newY);
        z.Set(newZ);
    }
    public void Strafe(double amount)
    {
        Vector3D<double> rotated = Vector3D.Transform(new(Ref.DefaultRight.x.Get(), Ref.DefaultRight.y.Get(), Ref.DefaultRight.z.Get()), Rotation);
        double newX = x.Get() + rotated.X * amount;
        double newY = y.Get() + rotated.Y * amount;
        double newZ = z.Get() + rotated.Z * amount;
        x.Set(newX);
        y.Set(newY);
        z.Set(newZ);
    }
    public void Lift(double amount)
    {
        Vector3D<double> rotated = Vector3D.Transform(new(Ref.DefaultUp.x.Get(), Ref.DefaultUp.y.Get(), Ref.DefaultUp.z.Get()), Rotation);
        double newX = x.Get() + rotated.X * amount;
        double newY = y.Get() + rotated.Y * amount;
        double newZ = z.Get() + rotated.Z * amount;
        x.Set(newX);
        y.Set(newY);
        z.Set(newZ);
    }

    /* Internal state methods */
    // TODO: get rid of this. It's only used for FOV, which can be stored somewhere else
    public Node Written(double d)
    {
        Val = d;
        return this;
    }

    // Sets the draw flags
    public Node Flagged(DrawMode dm)
    {
        drawMode = dm;
        return this;
    }

    // Indexes the constituents of a Multi in the internal values of the constituents
    // This is useful because getting the index using IndexOf repeatedly is too expensive
    public static void IndexConstituents(Node m)
    {
        for (int i = 0; i < m.Count; i++)
        {
            m.constituents[i].index = i;
        }
    }

    /* Parenting/tagging methods */
    // TODO: get rid of this method. This can be done with ctor, and with .Add solely
    public Node Parented(Node? m)
    {
        parent = m;
        return this;
    }

    // The tag is the name of the Node. A node can be referenced via parent[tag]
    public Node Tagged(string tag)
    {
        this.tag = tag;
        return this;
    }

    // Create a copy of the Multi
    public virtual Node Copy()
    {
        Node copy = new Node(x.Get(), y.Get(), col.Copy(), drawMode);
        if (faces is not null)
            copy.faces = new Mesh(faces.Faces);
        // Don't copy the texture, or create reference to it!
        //copy.texture = texture;

        // Copy the drivers
        // TODO: fix this
        //x.TransferDrivers(copy.x);
        //y.TransferDrivers(copy.y);

        copy.x.Set(x);
        copy.y.Set(y);

        // TODO: re-implement driver copying
        //foreach (IMap d in x.GetDrivers())
        //{
        //    copy.x.Driven(d);
        //}
        //foreach (IMap d in y.GetDrivers())
        //{
        //    copy.y.Driven(d);
        //}

        // Copy the constituents
        foreach (Node c in this)
        {
            copy.Add(c.Copy());
        }

        // headings, internalval, tempx, tempy
        //copy.Heading = Heading;
        copy.Rotation = new Quaternion<double>(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
        copy.Val = Val;

        return copy;
    }
    /* The two paste methods must match!! */
    public Node Paste()
    {
        Parent[$"{tag}_paste{x}{y}"] = Copy();
        return this;
    }
    public Node Pasted()
    {
        Paste();
        return Parent[$"{tag}_paste{x}{y}"];
    }

    public Node Unique()
    {
        List<double> xs = new List<double>();
        List<double> ys = new List<double>();
        Node c = new Node().To(x.Get(), y.Get(), z.Get());
        foreach (Node cst in constituents)
        {
            bool addMe = true;
            // Check for this position
            for (int i = 0; i < xs.Count; i++)
            {
                if (xs[i] == cst.x.Get() && ys[i] == cst.y.Get())
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
    public Node FlatAdjoin(Node m)
    {
        constituents.AddRange(m.constituents);
        return this;
    }

    // Replace a constituent
    public void AddAt(Node m, int n)
    {
        m.Translated(constituents[n].X, constituents[n].Y);
        constituents[n] = m;
    }

    // Add both multis to a new parent Multi
    public Node Adjoined(Node m, double xOffset = 0, double yOffset = 0, double zOffset = 0)
    {
        Node nm = new Node(xOffset, yOffset, zOffset, col, drawMode);
        nm.Add(this, m);
        return nm;
    }

    public Node Sub(Action<Node> action, Func<double, double>? truth = null, double threshold = 0)
    {
        return Sub((x, _i) => action(x), truth, threshold);
    }

    public Node Sub(Action<Node, int> action, Func<double, double>? truth = null, double threshold = 0)
    {
        int i = 0;
        foreach (Node c in this)
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

    public Node DeepSub(Action<Node> action, Func<double, double>? truth = null, double threshold = 0)
    {
        Sub(action, truth, threshold);
        foreach (Node c in this)
        {
            c.DeepSub(action, truth, threshold);
        }
        return this;
    }
    public Node IterSub(int iters, Action<Node> action, Func<double, double>? truth = null, double threshold = 0)
    {
        for (int i = 0; i < iters; i++)
        {
            Sub(action, truth, threshold);
        }
        return this;
    }

    // Wield is a form of recursion where each constituent is replaced with a copy of the given Multi
    public Node Wielding(Node outer)
    {
        // TODO: re-implement
        //Eject();
        for (int i = 0; i < Count; i++)
        {
            Node outerCopy = outer.Copy();
            constituents[i].Become(outerCopy);
        }

        return this;
    }

    // Surround is a form of recursion where the Multi is placed in the constituents of a given Multi
    public Node Surrounding(Node inner)
    {
        // TODO: re-implement
        //Eject();
        return inner.Wielding(Copy());
        //thisSurroundingInner.x.Set(x.Evaluate());
        //thisSurroundingInner.y.Set(y.Evaluate());
        //return thisSurroundingInner;//.Wielding(this);
    }

    public Node Recursed()
    {
        return Wielding(Copy());
    }

    /* Getter properties for indices and tags */
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

                //Scribe.Info($"{this.Parent} is distributing indices...");
                IndexConstituents(Parent);
                return (int)index!;
            }
            return (int)index;
        }
    }

    public double NormIdx
    {
        get => (double)Index / Parent.Count;
    }
    public string Tag
    {
        get => tag;
    }

    public int Count => constituents.Count;
    public int DeepCount
    {
        get
        {
            int x = Count;
            foreach (Node c in this)
            {
                x += c.DeepCount;
            }
            return x;
        }
    }
    public bool IsReadOnly => false;

    /* 
        Renders the Multi's geometry. The DrawFlags will affect the draw behahviour, (filled, lines, points)
     */
    public virtual void Render(double xOffset, double yOffset, double zOffset)
    {
        // TODO: implement render cache
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

        List<double[]> vertices = this.Select(n => new double[] { n.x.Get() + x.Get() + xOffset, n.y.Get() + y.Get() + yOffset, n.z.Get() + z.Get() + zOffset }).ToList();
        // No mesh, treat as a single face
        if (faces is null)
        {
            Paint.Render.Polygon(vertices, drawMode, constituents.Select(c => c.Col).ToList(), this);
            texture?.Draw(XCartesian(xOffset), YCartesian(yOffset));
            // Draw each constituent recursively
            foreach (Node m in this)
            {
                m.Render(xOffset + x.Get(), yOffset + y.Get(), zOffset + z.Get());
            }
        }
        // Meshed node, render each face
        else
        {
            foreach (int[] face in faces.Faces)
            {
                //Paint.Render.Polygon(face.Select(f => new double[]{this[f].x.Get()+xOffset, this[f].y.Get()+yOffset, this[f].z.Get()+zOffset,}).ToList(),drawMode, face.Select(i => this[i].Col).ToList(), this);
                Paint.Render.Polygon(face.Select(f => new double[] { this[f].x.Get() + x.Get() + xOffset, this[f].y.Get() + y.Get() + yOffset, this[f].z.Get() + z.Get() + zOffset, }).ToList(), drawMode, face.Select(i => (Color)new HSLA((double)i * 2 / Count, 1, 1, 255)).ToList(), this);
            }
            //foreach (Node m in this)
            //{
            //    m.Render(xOffset + x.Get(), yOffset + y.Get(), zOffset + z.Get());
            //}
        }
    }

    string Title()
    {
        string s = "";
        s += Count switch
        {
            0 => "Empty Node",
            1 => "Node",
            _ => $"{Count}-Node",
        };
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
        string xRel = x.Get().ToString("F1");
        string yAbs = Y.ToString("F1");
        string yRel = y.Get().ToString("F1");
        string zAbs = Z.ToString("F1");
        string zRel = z.Get().ToString("F1");
        s += $" at ({xRel},{yRel},{zRel})rel, ({xAbs},{yAbs},{zAbs})abs";

        foreach (Node m in constituents)
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
    public IEnumerator<Node> GetEnumerator()
    {
        return ((IEnumerable<Node>)constituents).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)constituents).GetEnumerator();
    }

    public void Add(Node item)
    {
        if (item == this)
        {
            throw Scribe.Error($"A Multi may not have itself as a consituent. Offending Multi: {this}, belonging to {Parent}");
        }
        if (item == Parent)
        {
            throw Scribe.Error("A Multi may not have its parent as a constituent!");
        }
        item.parent = this;
        constituents.Add(item);
    }
    public Node Add(params Node[] items)
    {
        foreach (Node m in items)
        {
            Add(m);
        }
        return this;
    }
    public void AddFiltered(Node m, string filter = "empty paint")
    {
        if (m.Tag == filter)
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
        foreach (Node c in constituents)
        {
            c.DisposeAllTextures();
        }
        constituents.Clear();
    }

    public bool Contains(Node item)
    {
        return constituents.Contains(item);
    }

    public Node Reversed()
    {
        constituents.Reverse();
        return this;
    }

    // Some interface method
    public void CopyTo(Node[] array, int arrayIndex)
    {
        constituents.CopyTo(0, array, arrayIndex, Math.Min(array.Length, Count));
    }

    public bool Remove(Node item)
    {
        return constituents.Remove(item);
    }

    public void DisposeAllTextures()
    {
        if (texture != null)
        {
            texture.Dispose();
        }
        foreach (Node m in this)
        {
            m.DisposeAllTextures();
        }
    }
}