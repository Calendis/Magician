//namespace Magician.Geo;
//using Alg;
//using Alg.Symbols;
//using Core;
//using Core.Maps;
//
//public class Implicit : Node
//{
//    IRelation relation;
//    Oper derivative;
//    IRelation[] parameterizations;
//
//    static readonly int[] defaultAxes = new int[] { 0, 2, 1 };
//    protected (double min, double max)[] range;
//    protected double[] resolution;
//
//    int maxOuts;
//    double inScale;
//    double outScale;
//    int maxSolutions;
//    int[] axes;
//    //Variable output;
//
//    public Implicit(Equation eq, double x, double y, double z, double inScale, double outScale, int domain, int[] axes, params (double, double, double)[] rangeResos) : this(eq.RHS.Minus(eq.LHS), x, y, z, inScale, outScale, domain, axes, rangeResos) { }
//    public Implicit(Oper o, double x, double y, double z, double inScale, double outScale, int domain, int[]? axes = null, params (double, double, double)[] rangeResos) : base(x, y, z)
//    {
//        axes ??= defaultAxes;
//        relation = new Equation(o.Copy(), Fulcrum.EQUALS, new Variable(0)).Solved(o.AssociatedVars[axes[^1]]);
//        //output = o.AssociatedVars[axes[^1]];
//        Variable parameter = new("parameter");
//        derivative = new Derivative(o, parameter, DerivativeKind.IMPLICIT);
//        parameterizations = new IRelation[o.AssociatedVars.Count];
//        for (int i = 0; i < o.AssociatedVars.Count; i++)
//        {
//            Scribe.Info($"Creating implicit parameterization {i}");
//            Oper parameterized = derivative.Copy().SimplifiedAll();
//            Variable implicitDiffParameter = new($"d{o.AssociatedVars[i]}[{i}]");
//            Scribe.Info($"Before: {parameterized}");
//            Scribe.Info($"Replacing {new Derivative(o.AssociatedVars[i], parameter, DerivativeKind.IMPLICIT)} with {implicitDiffParameter}...");
//            parameterized.Substitute(new Derivative(o.AssociatedVars[i], parameter, DerivativeKind.IMPLICIT), implicitDiffParameter);
//            Scribe.Info($"After:  {parameterized}");
//            parameterizations[i] = new Equation(parameterized.Copy(), Fulcrum.EQUALS, new Variable(0)).Solved(implicitDiffParameter);
//        }
//        this.inScale = inScale;
//        this.outScale = outScale;
//        this.axes = axes;
//        range = rangeResos.Select(t => (t.Item1, t.Item2)).ToArray();
//        resolution = rangeResos.Select(t => t.Item3).ToArray();
//        maxOuts = domain;
//        GenMesh();
//        Scribe.Info($"We are {this}");
//    }
//
//    public void GenMesh()
//    {
//        // Start with a point on the surface, specified by ranges, and use the parameterized implicit derivates to...
//        // ... "bump" the point as we go, meshing it perfectly for some resolution
//        //relation.Evaluate()
//        double x = range[0].min, z = range[1].min;
//        //IVal y = relation.Evaluate(x, z);
//        IVal y = relation.Evaluate(x);
//        //Add(new Node(x, y.Get(), z));
//
//        for (double u = range[0].min; u < range[0].max; u += resolution[0])
//        {
//            for (double v = range[1].min; u < range[1].max; u += resolution[1])
//            {
//                // TODO: this is a debug hack
//                //IVal dx = parameterizations[axes[0]].Evaluate(x, y.Get(), z);
//                //IVal dz = parameterizations[axes[2]].Evaluate(x, y.Get(), z);
//                IVal dx = parameterizations[axes[0]].Evaluate(x, y.Get());
//                IVal dz = parameterizations[axes[2]].Evaluate(x, y.Get());
//                x += dx.Get();
//                z += dz.Get();
//                y = relation.Evaluate(x, z);
//                Add(new Node(x, y.Get(), z));
//            }
//        }
//    }
//}