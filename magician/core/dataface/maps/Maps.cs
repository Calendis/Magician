namespace Magician.Core.Maps;

using Magician.Geo;
using Magician.Symbols;
using static Magician.Geo.Create;

public interface IRelation : IDimensional
{
    public IMultival Evaluate(params double[] args);
    public IMultival Evaluate(IVal args) => Evaluate(args.All);
    public int Ins { get; protected set; }
    public int Outs {get {return 0;}}
    int IDimensional.Dims => Ins + Outs;
}
public interface IFunction : IRelation
{
    public new IVal Evaluate(params double[] args);
    public new IVal Evaluate(IVal args) => Evaluate(args.All);
    IMultival IRelation.Evaluate(params double[] args) => new Vec(Evaluate(args).All);
    IMultival IRelation.Evaluate(IVal args) => new Vec(Evaluate(args.All));
    int IRelation.Outs { get { return 1; } }
}
public interface IParametric : IRelation
{
    int IRelation.Ins { get { return 1; } set { } }
    public IMultival Evaluate(double x);
    IMultival IRelation.Evaluate(params double[] args) => Evaluate(args[0]);
    IMultival IRelation.Evaluate(IVal args) => Evaluate(args.Get());
}
public interface IMap : IFunction
{
    int IRelation.Ins { get { return 1; } set { } }
    public IVal Evaluate(double x);
    IVal IFunction.Evaluate(params double[] args) => Evaluate(args[0]);
    IVal IFunction.Evaluate(IVal args) => Evaluate(args.Get());
}


// Multiple inputs, one output
// Plottable when the number of inputs is specified
public class InverseParamMap : IFunction
{
    Func<double[], IVal> map;
    public int Ins { get; set; }
    //public InverseParamMap(Func<double[], double>? f = null, int inputs = -1) : base(f is null ? null : xs => new double[] { f.Invoke(xs) })
    public InverseParamMap(Func<double[], IVal> f, int inputs)
    {
        Ins = inputs;
        map = f;
    }
    // TODO: is there a non-hack way to do this?? I don't want to just wrap the output like this, but oh well
    public InverseParamMap(Func<double[], double> nonwrapped, int inputs) : this(xs => IVal.FromLiteral(nonwrapped.Invoke(xs)), inputs) { }

    public Multi Plot(params Symbols.PlotOptions[] options)
    {
        return Plot(null, options);
    }
    public Multi Plot(Symbols.AxisSpecifier? outAxis = null, params Symbols.PlotOptions[] options)
    {
        if (Ins > 2)
        {
            throw Scribe.Error("Too many input variables");
        }

        NDCounter solveSpace = new(options.Select(o => o.Range).ToArray());
        List<AxisSpecifier> axes = options.Select(o => o.Axis).ToList();
        bool threeD = solveSpace.Dims >= 2;
        List<int[]> faces = new();
        Multi plot = new Multi().Flagged(DrawMode.PLOT);
        do
        {
            // Get arguments from the counter
            double[] inVals = new double[Ins];
            IVal outVal;
            for (int i = 0; i < Ins; i++)
            {
                inVals[i] = solveSpace.Get(i);
            }
            outVal = Evaluate(inVals);
            //Scribe.Info($"{inVals[0]}, {inVals[1]} => {outVal}");

            // Determine faces
            bool edgeRow = false;
            bool edgeCol = false;
            if (threeD)
            {
                double w = solveSpace.AxisLen(0);
                double h = solveSpace.AxisLen(1);
                int n = solveSpace.Val;
                if (solveSpace.Positional[0] >= Math.Ceiling(solveSpace.AxisLen(0) - 1))
                {
                    edgeCol = true;
                }
                if (n >= solveSpace.Max - w)
                {
                    edgeRow = true;
                }
                if (!edgeCol && !edgeRow && n + Math.Ceiling(w) + 1 < solveSpace.Max)
                {
                    faces.Add(new int[] { (int)Math.Ceiling(w) + n + 1, (int)Math.Ceiling(w) + n, n, n + 1 });
                }
            }

            // Put the x, y, and z values in an array in that order
            double[] argsByAxis = new double[3];
            // Replace with inVals based on the axis specifiers
            for (int i = 0; i < inVals.Length; i++)
            {
                argsByAxis[(int)axes[i]] = inVals[i];
            }
            argsByAxis[outAxis is null ?
                (int)new List<AxisSpecifier> { AxisSpecifier.X, AxisSpecifier.Y, AxisSpecifier.Z }.Except(options.Select(o => o.Axis)).ToList()[0]
                : (int)outAxis] = outVal.Get();

            // Plot colouring code can go here
            // TODO: provide an API for this
            double x = argsByAxis[0];
            double y = argsByAxis[1];
            double z = argsByAxis[2];
            //Scribe.Info($"xyz: {x}, {y}, {z}");
            //y += 80*Math.Sin(x/60 - z/60);
            double hue;
            double sat = 1;
            double ligh = 1;

            //hue = 4 * solveSpace.Positional[1] / solveSpace.AxisLen(1) - solveSpace.Positional[0] / solveSpace.AxisLen(0);
            hue = Math.Abs(y) / 100;
            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                hue = 0;
                y = 0;
            }

            //Multi point = new(argsByAxis[0], argsByAxis[1], argsByAxis[2]);
            Multi point = new(x, y, z);
            point.Colored(new HSLA(hue, sat, ligh, 255));
            plot.Add(point);
        }
        while (!solveSpace.Increment());

        // 3D plot format
        if (threeD)
        {
            Multi3D plot3d = new(plot);
            plot3d.SetFaces(faces);
            return plot3d.Flagged(DrawMode.INNER);
        }
        // 2D plot
        return plot;
    }
    //public static Func<double[], double[]> MapFromIPFunc(Func<double[], double> f)
    //{
    //    return xs => new double[] { f.Invoke(xs) };
    //}

    public IVal Evaluate(params double[] args)
    {
        return map.Invoke(args);
    }
}

// Parametric equation. One or fewer input, multiple outputs
// Always plottable, as the number of outputs is always known
public class ParamMap : IParametric
{
    //public int Params { get; set; }
    public int Outs { get; set; }
    public Func<double, IVal>[] Maps;
    //public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray())
    public ParamMap(params Func<double, IVal>[] fs)
    {
        Outs = fs.Length;
        Maps = fs.ToArray();
        //Func<double, double>[] fs2 = new Func<double, double>[fs.Length];
        //int c = 0;
        //foreach (Func<double, double> f in fs)
        //{
        //    fs2[c++] = f.Invoke;
        //}
        //Maps = fs2;
    }
    public ParamMap(params Func<double, double>[] ns) : this(FromFuncs(ns)) {}
    public static Func<double, IVal>[] FromFuncs(params Func<double, double>[] ns)
    {
        List<Func<double, IVal>> wrapped = new();
        foreach (Func<double, double> nw in ns)
        {
            wrapped.Add(x => IVal.FromLiteral(nw.Invoke(x)));
        }
        return wrapped.ToArray();
    }
    //public ParamMap(params DirectMap[] fs) : base(xs => fs.Select(m => m.Evaluate(xs[0])).ToArray())
    public ParamMap(params IMap[] fs) : this(fs.Select<IMap, Func<double, IVal>>(im => im.Evaluate).ToArray()) { }
    public IMultival Evaluate(double x = 0) => new Vec(Maps.Select(f => f.Invoke(x)).ToArray());

    //public override Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    public Multi Plot(Symbols.Range paramRange, Color c, double x = 0, double y = 0, double z = 0)
    {
        if (Outs > 3)
            throw Scribe.Error($"Cannot plot ParamMap with {Outs} outputs");
        Multi plot = new Multi().Flagged(DrawMode.PLOT);
        double start = paramRange.Min;
        double end = paramRange.Max;
        double dt = paramRange.Res;
        for (double t = start; t < end; t += dt)
        {
            IMultival out0 = ((IParametric)this).Evaluate(t);
            IMultival out1 = ((IParametric)this).Evaluate(t + dt);
            double[] pos0 = { 0, 0, 0 };
            double[] pos1 = { 0, 0, 0 };
            IMultival[] ous = new[] { out0, out1 };
            double[][] poss = new[] { pos0, pos1 };
            int counter = 0;
            int innerCounter;
            foreach (Vec ou in ous)
            {
                innerCounter = 0;
                foreach (double d in ((IVal)ou).All)
                {
                    poss[counter][innerCounter] = d;
                    innerCounter++;
                }
                counter++;
            }
            plot.Add(
                Point(pos0[0], pos0[1], pos0[2]).Colored(c),
                Point(pos1[0], pos1[1], pos1[2]).Colored(c)
            );
        }

        return plot.To(x, y, z);
    }
}

// One or fewer input, one output
// Always plottable
public class DirectMap : IMap
{
    Func<double, IVal> map;
    public DirectMap(Func<double, IVal> f)
    {
        map = f;
    }
    public DirectMap(Func<double, double> f) : this(x => IVal.FromLiteral(f.Invoke(x))) {}
    public IVal Evaluate(double x = 0)
    {
        return map.Invoke(x);
    }
}