namespace Magician.Geo;
using Magician.Core;
using Core.Maps;
using Alg.Symbols;
using Magician.Alg;

public class Implicit : Node, IRelation
{
    // represents x, z, y. This means that by default, the y-axis is the output parameter, while x and z are inputs
    static readonly int[] defaultAxes = new int[] { 0, 2, 1 };
    protected (double, double)[] range;
    protected double[] resolution;
    protected IRelation relation;
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, params (double, double, double)[] rangeResos) : base(x, y, z)
    {
        relation = r;
        if (rangeResos.Length != Ins)
            throw Scribe.Error($"Number of range-resolution pairs did not match length of inputs ({Ins})");
        range = rangeResos.Select(t => (t.Item1, t.Item2)).ToArray();
        resolution = rangeResos.Select(t => t.Item3).ToArray();
        // For easier control over this, you may use a Symbolic
        GenNodesAndMesh(inScale, outScale, 2, -1, defaultAxes);
    }
    public Implicit(IRelation r, double x, double y, double z, double inScale, double outScale, double reso, params (double, double)[] ranges) : this(r, x, y, z, inScale, outScale, ranges.Select(r => (r.Item1, r.Item2, reso)).ToArray()) { }

    public IVar Cache => relation.Cache;

    public int Ins => relation.Ins;

    // maxOuts limits the number of paramaters of the output scalar
    // maxOuts=1 will only generate real solutions
    // maxOuts=2 will include complex solutions
    // maxouts>2 allows for additional non-arithmetic data to be passed to the generator
    public void GenNodesAndMesh(double inScale, double outScale, int maxOuts, int maxSolutions, int[] axes, Func<double[], Color>? colGen = null)
    {
        NDCounter solveSpace = new(range, resolution);
        List<List<(double[], IVal)>> preMeshes = new();
        // Get all the values out of the function. Because the function may be multivalued, we 
        // sort them into separate mesh groups
        do
        {
            double[] inVals = new double[Ins];
            for (int i = 0; i < Ins; i++)
            {
                inVals[i] = solveSpace.Get(i);
            }
            IVal outVal = Evaluate(inVals);

            int solutions = 1;
            if (outVal is Multivalue mv)
            {
                solutions = mv.Solutions;
                //Scribe.Info($"{outVal} is Multivalued, with {solutions} solutions");
            }

            for (int s = 0; s < solutions; s++)
            {
                if (s == maxSolutions)
                    break;
                if (preMeshes.Count < s + 1) { preMeshes.Add(new()); }
                // Restrict the outVal
                if (outVal.Trim().Dims <= maxOuts)
                    preMeshes[s].Add((inVals, outVal is Multivalue mv2 ? new Val(mv2.All[s]) : outVal));
            }

        } while (!solveSpace.Increment());

        List<Mesh> branchMeshes = new();
        
        foreach (List<(double[], IVal)> branch in preMeshes)
        {
            int c = Count;
            int width = (int)((range[0].Item2 - range[0].Item1) / resolution[0]);
            foreach ((double[] inVals, IVal outVal) in branch)
            {
                double[] xyz = new double[3];
                xyz[0] = 0;
                xyz[1] = 0;
                xyz[2] = 0;

                for (int i = 0; i < Ins; i++) { xyz[axes[i]] = inVals[i] * inScale; }
                xyz[axes[2]] = outVal.Get() * outScale;

                // Infer colour data
                double[] hsla = new double[4];
                List<double> colourData = outVal.Values.Skip(1).ToList();
                for (int i = 0; i < Math.Min(4, colourData.Count); i++)
                    hsla[i] = colourData[i];
                // Default colouring
                if (true || colourData.Count == 0)
                {
                    double theta = outVal.Trim().Dims == 1 ? outVal.Get() < 0 ? Math.PI : 0 : Math.Atan2(outVal.Get(1), outVal.Get());
                    theta = outVal.Magnitude;
                    if (double.IsNaN(theta) || !double.IsFinite(theta))
                        theta = outVal.Trim().Dims == 1 ? outVal.Get() < 0 ? Math.PI : 0 : Math.Atan2(outVal.Get(1), outVal.Get());
                    hsla[0] = theta;
                    hsla[1] = 1;
                    hsla[2] = 1;
                    hsla[3] = 255;
                }

                Node n = new(xyz[0], xyz[1], xyz[2], new HSLA(hsla[0], hsla[1], hsla[2], hsla[3]));
                Add(n);
            }
            branchMeshes.Add(Mesh.Square(width, (int)solveSpace.Max, c));
        }
        // Assemble the mesh
        faces = new(branchMeshes.Aggregate(new List<int[]>(), (m, n) => m = m.Concat(n.Faces).ToList()));
    }

    public IVal Evaluate(params double[] args)
    {
        return relation.Evaluate(args);
    }
}

public class Symbolic : Implicit
{
    public Symbolic(Oper o, double x, double y, double z, double inScale, double outScale, params (double, double, double)[] rangeResos) : base(o, x, y, z, inScale, outScale, rangeResos)
    {
        Scribe.Info($"relation is Oper? {relation is Oper}");
    }
    public Symbolic(Oper o, double x, double y, double z, double inScale, double outScale, double reso, params (double, double)[] ranges) : base(o, x, y, z, inScale, outScale, reso, ranges)
    {
        Scribe.Info($"relation is Oper? {relation is Oper} (ctor 2)");
    }
}