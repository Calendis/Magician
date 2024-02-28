namespace Magician.Geo;
using Core;
using Core.Maps;
using Alg;
using Alg.Symbols;

// TODO: rename this
public class Implicit : Node, IRelation
{
    // represents x, z, y. This means that by default, the y-axis is the output parameter, while x and z are inputs
    static readonly int[] defaultAxes = new int[] { 0, 2, 1 };
    protected (double, double)[] range;
    protected double[] resolution;
    protected IRelation relation;
    //List<Mesh.Region> flatRegions;
    //Mesh.Region[,] regions;
    //(Mesh.Region, double[], IVal)[,] regionsArgs;
    int maxOuts;
    double inScale;
    double outScale;
    int maxSolutions;
    int[] axes;
    Sampling? sampling;
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int domain, int[] axes, Sampling? sampling, params (double, double, double)[] rangeResos) : base(x, y, z)
    {
        relation = r;
        //if (rangeResos.Length != Ins)
        //    throw Scribe.Error($"Number of range-resolution pairs did not match length of inputs ({Ins})");
        range = rangeResos.Select(t => (t.Item1, t.Item2)).ToArray();
        resolution = rangeResos.Select(t => t.Item3).ToArray();
        maxOuts = domain;
        this.sampling = sampling;
        this.inScale = inScale;
        this.outScale = outScale;
        maxSolutions = -1;
        this.axes = axes;
        Refresh();
    }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int[] axes, Sampling? sampling, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, 2, axes, sampling, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int domain, int[] axes, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, domain, axes, null, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int[] axes, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, 2, axes, null, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int domain, Sampling sampling, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, domain, defaultAxes, sampling, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, Sampling sampling, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, 2, defaultAxes, sampling, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int domain, params (double, double, double)[] rangeResos) : this(r, x, y, z, outScale, inScale, domain, defaultAxes, null, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, params (double, double, double)[] rangeResos) : this(r, x, y, z, inScale, outScale, 2, defaultAxes, null, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double scale, params (double, double, double)[] rangeResos) : this(r, x, y, z, scale, scale, 2, defaultAxes, null, rangeResos) { }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, int domain, int[] axes, Sampling? sampling, double reso, params (double, double)[] ranges) : this(r, x, y, z, inScale, outScale, domain, axes, sampling, ranges.Select(r => (r.Item1, r.Item2, reso)).ToArray()) { }
    public IVal Cache => relation.Cache;
    public int Ins => relation.Ins;
    // maxOuts limits the number of paramaters of the output scalar
    // maxOuts=1 will only generate real solutions
    // maxOuts=2 will include complex solutions
    // maxouts>2 allows for additional non-arithmetic data to be passed to the generator
    public void Refresh()
    {
        GenMesh(GenRegions(maxOuts, sampling), (int)((range[0].Item2 - range[0].Item1) / resolution[0]));
    }
    public List<(Mesh.Region, double[], IVal)> GenRegions(int maxOuts, Sampling? sampling)
    {
        NDCounter solveSpace = new(range, resolution);
        List<(Mesh.Region, double[], IVal)> flatRegArgs = new();
        Mesh.Region last = Mesh.Region.BORDER;
        double lastRow = 0;

        // Get all the values out of the function. Because the function may be multivalued, have holes, or 
        // consist of multiple disjoint sections, we need to sort them into separate mesh groups
        do
        {
            double[] inVals = new double[Math.Min(2, Ins)];
            //for (int i = 0; i < Ins; i++)
            for (int i = 0; i < range.Length; i++)
            {
                inVals[i] = solveSpace.Get(i);
            }
            // TODO: Get outVal from the node if it exists
            IVal outVal = EvalCopy(inVals);

            if (sampling is not null)
            {
                IVal tr = sampling.Evaluate(inVals[0] / (range[0].Item2 - range[0].Item1), inVals[1]);
                if (tr.Dims < 2)
                    tr.Push(0);
                inVals = tr.Values.ToArray();
                outVal = EvalCopy(inVals);
            }

            // TODO: clarify this magic threshold
            if (outVal.Trim().Dims > maxOuts && outVal.Dims > 1 && Math.Abs(outVal.Get(1)) < 0.001)
                outVal.Set(1, 0);

            if ((outVal.Trim().Dims > maxOuts) || double.IsNaN(outVal.Magnitude) || double.IsInfinity(outVal.Magnitude))
                flatRegArgs.Add((Mesh.Region.BORDER, inVals, outVal));
            else
                flatRegArgs.Add((Mesh.Region.UNEXPLORED, inVals, outVal));
            last = flatRegArgs[^1].Item1;
            lastRow = inVals.Length > 1 ? inVals[1] : lastRow;
        } while (!solveSpace.Increment());

        int width = (int)((range[0].Item2 - range[0].Item1) / resolution[0]);
        // Use flood fill to divide the world into regions
        List<Mesh.Region> flatRegions = flatRegArgs.Select(t => t.Item1).ToList();
        Mesh.DivideRegions(flatRegions, width);
        // Paste the data back in
        for (int i = 0; i < flatRegions.Count; i++)
            flatRegArgs[i] = (flatRegions[i], flatRegArgs[i].Item2, flatRegArgs[i].Item3);
        return flatRegArgs;
    }
    public void GenMesh(List<(Mesh.Region, double[], IVal)> flatRegArgs, int width, Func<double[], Color>? colGen = null)
    {
        // Format the data 
        List<Mesh.Region> flatRegions = flatRegArgs.Select(t => t.Item1).ToList();
        (Mesh.Region, double[], IVal)[,] regionsArgs = new (Mesh.Region, double[], IVal)[flatRegions.Count / width, width];

        Mesh.Region[,] regions = new Mesh.Region[flatRegions.Count / width, width];
        for (int i = 0; i < flatRegions.Count / width; i++)
            for (int j = 0; j < width; j++)
            {
                regionsArgs[i, j] = flatRegArgs[i * width + j];
                regions[i, j] = regionsArgs[i, j].Item1;
            }

        List<Mesh> branchMeshes = new();
        List<List<(double[], IVal)>> preMeshes = new();
        List<List<(int, int)>> branchIdcs = new();
        // Create pre-meshes to account for multiple solutions
        for (int i = 0; i < flatRegions.Count / width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // i, j
                
                double[] inVals = regionsArgs[i, j].Item2;
                IVal outVal = regionsArgs[i, j].Item3;
                int solutions = 1;
                if (outVal is Multivalue mv)
                    solutions = mv.Solutions;
                for (int s = 0; s < solutions; s++)
                {
                    if (s == maxSolutions)
                        break;
                    if (preMeshes.Count < s + 1) { preMeshes.Add(new()); }
                    if (branchIdcs.Count < s + 1) { branchIdcs.Add(new()); }
                    preMeshes[s].Add((inVals, outVal is Multivalue mv2 ? new Val(mv2.All[s]) : outVal));
                    branchIdcs[s].Add((i, j));
                }
            }
        }

        int brCounter = 0;
        int ndCounter = 0;
        int c = 0;
        foreach (List<(double[], IVal)> branch in preMeshes)
        {
            //int c = Count;
            foreach ((double[] inVals, IVal outVal) in branch)
            {
                double[] xyz = new double[3];
                xyz[0] = 0;
                xyz[1] = 0;
                xyz[2] = 0;

                for (int j = 0; j < Ins; j++) { xyz[axes[j]] = inVals[j] * inScale; }
                xyz[axes[2]] = outVal.Get() * outScale;
                //xyz[axes[2]] = outVal.Magnitude * outScale;

                // Infer colour data
                double[] hsla = new double[4];
                List<double> colourData = outVal.Values.Skip(1).ToList();
                for (int j = 0; j < Math.Min(4, colourData.Count); j++)
                    hsla[j] = colourData[j];
                // Default colouring
                if (true || colourData.Count == 0)
                {
                    double colX = outVal.Get();
                    double colY = outVal.Dims < 2 ? 0 : outVal.Get(1);

                    //colX /= range[axes[2]].Item2;
                    //colY /= range[axes[2]].Item2;
                    //colX /= Math.Pow(resolution[axes[2]], 1.1);
                    //colY /= Math.Pow(resolution[axes[2]], 1.1);
                    //colY += outScale/1000;
                    double theta = outVal.Trim().Dims == 1 ? colX : outVal.Magnitude;

                    if (double.IsNaN(theta) || !double.IsFinite(theta))
                        theta = outVal.Magnitude;
                    hsla[0] = theta;
                    hsla[1] = 1;
                    hsla[2] = 1;
                    hsla[3] = 255;
                }

                if (Count > ndCounter)
                    this[ndCounter].To(xyz[0], xyz[1], xyz[2]).Colored(new HSLA(hsla[0], hsla[1], hsla[2], hsla[3]));
                else
                {
                    Node n = new(xyz[0], xyz[1], xyz[2], new HSLA(hsla[0], hsla[1], hsla[2], hsla[3]));
                    Add(n);
                }
                ndCounter++;
            }
            branchMeshes.Add(Mesh.Jagged(branchIdcs[brCounter++], regions, c));
            c = branch.Count;
        }

        // Assemble the mesh
        faces = new(branchMeshes.Aggregate(new List<int[]>(), (m, n) => m = m.Concat(n.Faces).ToList()));
        if (faces.Faces.Count == 0)
            faces = null;
    }

    public override void Update()
    {
        Refresh();
        base.Update();
    }

    public IVal Evaluate(params double[] args)
    {
        return relation.Evaluate(args);
    }
    IVal EvalCopy(params double[] args)
    {
        IVal iv = relation.Evaluate(args);
        return iv is Multivalue mv ? new Multivalue(mv.All.Select(v => new Val(v)).ToArray()) : new Val(iv);
    }
}
