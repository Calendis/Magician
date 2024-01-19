namespace Magician.Geo;
using Core;
using Core.Maps;
using Renderer;

using System.Collections;
using Silk.NET.Maths;


public enum DrawMode : short
{
    INVISIBLE = 0b0000,
    PLOT = 0b1000,
    CONNECTINGLINE = 0b0100,
    INNER = 0b0010,
    POINTS = 0b0001,
    OUTER = 0b1100,
    FULL = 0b1110,
    OUTERP = 0b1101
}

/* A Node is a drawable tree of 3-vectors (more Multis) */
public class Node : Vec3, ICollection<Node>
{
    // The origin will have a null parent
    Node? parent;
    protected List<Node> constituents;
    Dictionary<string, Node> constituentTags = new Dictionary<string, Node>();
    double pitch = 0; double yaw = 0; double roll = 0;
    public double Val { get; set; } = 0;
    // Keep references to the rendered RDrawables so they can be removed
    List<RDrawable> drawables = new List<RDrawable>();
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
    public Vec3 Abs
    {
        get => new Vec3(X, Y, Z);
    }
    public Vec3 Heading
    {
        get => Geo.Ref.DefaultHeading.YawPitchRotated(-yaw, pitch);
        set
        {
            pitch = -Math.Asin(-value.y.Get());
            yaw = -Math.Atan2(value.x.Get(), value.z.Get());
        }
    }
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
                return Heading.x.Get();
            return x.Get() + Parent.RecursHeadingX;
        }
    }
    double RecursHeadingY
    {
        get
        {
            if (parent is null)
                return Heading.y.Get();
            return y.Get() + Parent.RecursHeadingY;
        }
    }
    double RecursHeadingZ
    {
        get
        {
            if (parent is null)
                return Heading.z.Get();
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
            if (i >= Count)
            {
                throw new IndexOutOfRangeException($"Tried to get index {i} of {this}");
            }
            return constituents[i];
        }
        set
        {
            if (i >= Count)
            {
                throw new IndexOutOfRangeException($"Tried to get index {i} of {this}");
            }
            constituents[i].DisposeAllTextures();
            RDrawable.drawables.RemoveAll(rd => drawables.Contains(rd));
            constituents[i] = value.Parented(this);
        }
    }
    public Node this[string tag]
    {
        get
        {
            if (constituentTags.ContainsKey(tag))
            {
                return constituentTags[tag];
            }
            throw new KeyNotFoundException($"tag {tag} does not exist in {this}");
        }
        set
        {
            // Create new Multi associated with the tag
            if (!constituentTags.ContainsKey(tag))
            {
                //Scribe.Info($"Creating tag \"{tag}\"");
                constituentTags.Add(tag, value);
                Add(value.Tagged(tag));
                return;
            }

            // Destroy the old Multi, and tag the new one with the same tag
            constituentTags[tag].DisposeAllTextures();
            RDrawable.drawables.RemoveAll(rd => drawables.Contains(rd));
            Remove(constituentTags[tag]);
            constituentTags[tag] = value;
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
    public Node(Vec pt3d) : this(pt3d.x.Get(), pt3d.y.Get(), pt3d.z.Get()) { }

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
            //cst.col = c;
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
    public Node RotatedZ(double theta)
    {
        roll = (roll + theta) % (2 * Math.PI);
        return Sub(
            m =>
            m.PhaseXY += theta
        );
    }
    public Node RotatedY(double theta)
    {
        yaw = (yaw + theta) % (2 * Math.PI);
        yaw += yaw > 0 ? 0 : 2 * Math.PI;
        return Sub(
            m =>
            m.PhaseXZ += theta
        );
    }
    public Node RotatedX(double theta)
    {
        pitch = (pitch + theta) % (2 * Math.PI);
        return Sub(
            m =>
            m.PhaseYZ += theta
        );
    }

    /* Scaling methods */
    public Node Scaled(double mag)
    {
        throw Scribe.Issue("TODO: re-implement scaling");
        //return this;
    }

    public static void _Texture(Node m, Renderer._SDLTexture t)
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
    public Node Textured(Renderer._SDLTexture t)
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
        Vec newPos = new(this + (IVec)Heading * amount);
        x.Set(newPos.x);
        y.Set(newPos.y);
        z.Set(newPos.z);
    }
    public void Strafe(double amount)
    {
        Vec newPos = new(this + (IVec)Heading.YawPitchRotated(-Math.PI / 2, 0) * amount);
        x.Set(newPos.x);
        y.Set(newPos.y);
        z.Set(newPos.z);
    }

    /* Internal state methods */
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
    // This is useful because getting the index using IndexOf is too expensive
    public static void IndexConstituents(Node m)
    {
        for (int i = 0; i < m.Count; i++)
        {
            m.constituents[i].index = i;
        }
    }

    /* Parenting/tagging methods */
    public Node Parented(Node? m)
    {
        parent = m;
        return this;
    }

    // The tag is the name of the Spell. A spell can be referenced via parent[tag]
    public Node Tagged(string tag)
    {
        this.tag = tag;
        return this;
    }

    // Create a copy of the Multi
    public virtual Node Copy()
    {
        Node copy = new Node(x.Get(), y.Get(), col.Copy(), drawMode);
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
        copy.Heading.x.Set(Heading.x);
        copy.Heading.y.Set(Heading.y);
        copy.Heading.z.Set(Heading.z);
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

        double r = col.R;
        double g = col.G;
        double b = col.B;
        double a = col.A;

        // Get a projection of each constituent point
        double[][] unclippedVerts = new double[Count][];
        for (int i = 0; i < Count; i++)
        {
            // we don't need these for anything
            //Vector3D<double> modelCoords = new(
            //    this[i].x.Evaluate(),
            //    this[i].y.Evaluate(),
            //    this[i].z.Evaluate()
            //);

            // The absolute position of the multi
            Vector3D<double> worldCoords = new(
                this[i].X,
                this[i].Y,
                this[i].Z
            );

            // These two vectors define the camera
            Vec3 targV = new Vec((IVec)Ref.Perspective + Ref.Perspective.Heading).ToVec3();
            Vec3 upV = targV.YawPitchRotated(0, Math.PI / 2);

            // TODO: move these out of the loop
            // Matrix magic
            Matrix4X4<double> view = Matrix4X4.CreateLookAt<double>(
                new(Ref.Perspective.X, Ref.Perspective.Y, Ref.Perspective.Z),
                new(targV.x.Get(), targV.y.Get(), targV.z.Get()),
                new(0, 1, 0)
            );
            Matrix4X4<double> projection = Matrix4X4.CreatePerspectiveFieldOfView<double>(
                Ref.FOV / 180d * Math.PI,
                Runes.Globals.winWidth / Runes.Globals.winHeight,
                0.1, 2000
            );
            Vector4D<double> intermediate = Vector4D.Transform<double>(worldCoords, view);
            Vector4D<double> final = Vector4D.Transform<double>(intermediate, projection);

            // Format the projected vertices for GLSL
            unclippedVerts[i] = new double[]
            {
                final.X/-final.Z,
                final.Y/-final.Z,
                -final.Z,
                1+0*final.W
            };
        }

        List<double[]> clippedVerts = new();
        int counter = 0;
        double avgX = 0;
        double avgY = 0;
        foreach (double[] v in unclippedVerts)
        {
            // Check to see if the constituent's z-coordinate is out-of-bounds
            // It is considered OOB when it is not in front of the camera along the axis parallel to the camera
            bool zInBounds;
            Vec3 absPos = this[counter++].Abs;
            Vec3 camPos = Ref.Perspective;
            // Rotate so that we can compare straight along the axis using a >=
            absPos = absPos.YawPitchRotated(Ref.Perspective.yaw, -Ref.Perspective.pitch);
            camPos = camPos.YawPitchRotated(Ref.Perspective.yaw, -Ref.Perspective.pitch);
            zInBounds = absPos.z.Get() - camPos.z.Get() >= 0;

            if (zInBounds)
            {
                clippedVerts.Add(v);
                avgX += v[0];
                avgY += v[1];
            }
            else
            {
                // Seems to work fine without calculating clipping intersections, so do nothing
            }
        }

        // The vertices are GLSL-ready
        double[][] projectedVerts = clippedVerts.ToArray();

        // Draw points
        if ((drawMode & DrawMode.POINTS) > 0)
        {
            int numPoints = projectedVerts.Length;
            RPoint[] rPointArray = new RPoint[numPoints];
            RPoints rPoints;

            for (int i = 0; i < numPoints; i++)
            {
                rPointArray[i] = new RPoint(projectedVerts[i][0], projectedVerts[i][1], projectedVerts[i][2],
                    constituents[i].Col.R, constituents[i].Col.G, constituents[i].Col.B, 255);
            }
            rPoints = new(rPointArray);
            drawables.Add(rPoints);
            RDrawable.drawables.Add(rPoints);
        }

        // Draw lines
        if ((drawMode & DrawMode.PLOT) > 0)
        {
            bool connected = (drawMode & DrawMode.CONNECTINGLINE) > 0 && Count >= 3;
            int numLines = projectedVerts.Length - (connected ? 0 : 1);
            if (numLines < 1)
                return;

            RLine[] rLineArray = new RLine[numLines];
            RLines rLines;

            for (int i = 0; i < projectedVerts.Length - 1; i++)
            {
                double x0 = projectedVerts[i][0]; double x1 = projectedVerts[i + 1][0];
                double y0 = projectedVerts[i][1]; double y1 = projectedVerts[i + 1][1];
                double z0 = projectedVerts[i][2]; double z1 = projectedVerts[i + 1][2];
                rLineArray[i] = new RLine(x0, y0, z0, x1, y1, z1, constituents[i].Col.R, constituents[i].Col.G, constituents[i].Col.B, constituents[i].Col.A);
            }
            // If the Multi is a closed shape, connect the first and last constituent with a line
            if (connected)
            {
                double[] pLast = projectedVerts[projectedVerts.Length - 1];
                double[] pFirst = projectedVerts[0];

                double subr = this[Count - 1].Col.R;
                double subg = this[Count - 1].Col.G;
                double subb = this[Count - 1].Col.B;
                double suba = this[Count - 1].Col.A;

                rLineArray[^1] = new RLine(pLast[0], pLast[1], pLast[2], pFirst[0], pFirst[1], pFirst[2], subr, subb, subg, suba);
            }
            rLines = new(rLineArray);
            drawables.Add(rLines);
            RDrawable.drawables.Add(rLines);
        }

        // If the flag is set, and there are at least 3 constituents, fill the shape
        if (((drawMode & DrawMode.INNER) > 0) && Count >= 3)
        {
            List<int> ect = EarCut.Triangulate(projectedVerts);
            if (ect.Count % 3 != 0)
            {
                throw Scribe.Issue("Triangulator broke");
            }
            List<int[]> triVertexIndices = new();
            for (int i = 0; i < ect.Count; i += 3)
            {
                triVertexIndices.Add(new int[] { ect[i], ect[i + 1], ect[i + 2] });
            }

            int numTriangles = triVertexIndices.Count;
            RTriangle[] rTriArray = new RTriangle[numTriangles];
            RTriangles rTris;

            for (int i = 0; i < numTriangles; i++)
            {
                int[] vertexIndices = triVertexIndices[i];
                int tri0 = vertexIndices[0];
                int tri1 = vertexIndices[1];
                int tri2 = vertexIndices[2];

                RTriangle rTri = new(
                    projectedVerts[tri0][0], projectedVerts[tri0][1], projectedVerts[tri0][2],
                    projectedVerts[tri1][0], projectedVerts[tri1][1], projectedVerts[tri1][2],
                    projectedVerts[tri2][0], projectedVerts[tri2][1], projectedVerts[tri2][2],
                    Col.R, Col.G, Col.B, Col.A
                );
                rTriArray[i] = rTri;
            }

            rTris = new RTriangles(rTriArray);
            drawables.Add(rTris);
            RDrawable.drawables.Add(rTris);
        }

        // If not null, draw the texture
        if (texture != null)
        {
            texture.Draw(XCartesian(xOffset), YCartesian(yOffset));
        }

        // Draw each constituent recursively
        foreach (Node m in this)
        {
            m.Render(xOffset, yOffset, zOffset);
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