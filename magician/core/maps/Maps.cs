namespace Magician.Core.Maps;

using Magician.Geo;
using Magician.Alg;
using static Magician.Geo.Create;

public interface IRelation
{
    IVar Cache { get; }
    public IVal Evaluate(params double[] args);
    public IVal Evaluate(IVal arg) => Evaluate(arg.Values.ToArray());
    public double[] Evaluate(List<double> args) => Evaluate(args.ToArray()).Values.ToArray();
    public int Ins { get; }
    // TODO: change this
    public int Outs { get { return 0; } }
}

public interface IParametric : IRelation
{
    int IRelation.Ins => 1;
    public IVal Evaluate(double x) => Evaluate(new double[] {x});
    IVal IRelation.Evaluate(params double[] args) => Evaluate(args[0]);
    IVal IRelation.Evaluate(IVal args) => Evaluate(args.Get());
}
public interface IMap : IRelation
{
    int IRelation.Ins => 1;
    //public IVal Evaluate(double x);
    public double Evaluate(double x) => ((IRelation)this).Evaluate(x).Get();
    //IVal IFunction.Evaluate(params double[] args) => Evaluate(args[0]);
    //IVal IFunction.Evaluate(IVal args) => Evaluate(args.Get());
}


// Multiple inputs, one output
// Plottable when the number of inputs is specified
public class Relational : IRelation
{
    private readonly Var vCache = new(0);
    public IVar Cache => vCache;
    Func<double[], double[]> map;
    public int Ins { get; set; }
    public Relational(Func<double[], double[]> f, int inputs)
    {
        Ins = inputs;
        map = f;
    }

    //public double[] Evaluate(List<double> args)
    //{
    //    return map.Invoke(args.ToArray());
    //}
    public IVal Evaluate(params double[] args)
    {
        Cache.Set(map.Invoke(args));
        return Cache;
    }

    public Node Plot(params Alg.PlotOptions[] options)
    {
        return Plot(null, options);
    }
    public Node Plot(Alg.AxisSpecifier? outAxis = null, params Alg.PlotOptions[] options)
    {
        if (Ins > 2)
        {
            throw Scribe.Error("Too many input variables");
        }

        NDCounter solveSpace = new(options.Select(o => o.Range).ToArray());
        List<AxisSpecifier> axes = options.Select(o => o.Axis).ToList();
        bool threeD = solveSpace.Dims >= 2;
        //List<int[]> faces = new();
        Node plot = new Node().Flagged(DrawMode.PLOT);
        do
        {
            // Get arguments from the counter
            IVal outVal;
            double[] inVals = new double[Ins];
            for (int i = 0; i < Ins; i++)
            {
                inVals[i] = solveSpace.Get(i);
            }
            outVal = Evaluate(inVals);

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
            hue = Math.Abs(y) / 1000;
            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                hue = 0;
                y = 0;
            }

            //Multi point = new(argsByAxis[0], argsByAxis[1], argsByAxis[2]);
            Node point = new(x, y, z);
            //Scribe.Tick();
            //point.Colored(new HSLA(hue, sat, ligh, 255));
            point.Colored(new HSLA(hue, sat, ligh, 255));
            plot.Add(point);
        }
        while (!solveSpace.Increment());

        // 3D plot format
        if (threeD)
        {
            Node plot3d = new(plot, Mesh.Rect((int)solveSpace.AxisLen(0), (int)solveSpace.Max));
            //Scribe.Tick();
            //plot3d.SetFaces(faces);
            return plot3d.Flagged(DrawMode.INNER);
        }
        // 2D plot
        return plot;
    }
}

// Parametric equation. One input, multiple outputs
public class Parametric : IParametric
{
    //public int Params { get; set; }
    private readonly Var vCache = new(0);
    public IVar Cache => vCache;
    public int Outs { get; set; }
    Func<double, double[]> map;
    //public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray())
    public Parametric(Func<double, double[]> m)
    {
        // TODO: change the outs
        Outs = 0;
        map = m;
    }
    public Parametric(params Func<double, double>[] ns) : this(x => ns.Select(f => f.Invoke(x)).ToArray()) { }
    public Parametric(params IMap[] fs) : this(fs.Select<IMap, Func<double, double>>(f => f.Evaluate).ToArray()) { }
    public IVal Evaluate(double x = 0)
    {
        Cache.Set(map.Invoke(x));
        return Cache;
    }

    public Node Plot(Alg.Range paramRange, Color c, double x = 0, double y = 0, double z = 0)
    {
        if (Outs > 3)
            throw Scribe.Error($"Cannot plot ParamMap with {Outs} outputs");
        Node plot = new Node().Flagged(DrawMode.PLOT);
        double start = paramRange.Min;
        double end = paramRange.Max;
        double dt = paramRange.Res;
        for (double t = start; t < end; t += dt)
        {
            IVal out0 = ((IParametric)this).Evaluate(t);
            IVal out1 = ((IParametric)this).Evaluate(t + dt);
            double[] pos0 = { 0, 0, 0 };
            double[] pos1 = { 0, 0, 0 };
            IVal[] ous = new[] { out0, out1 };
            double[][] poss = new[] { pos0, pos1 };
            int counter = 0;

            int innerCounter;
            foreach (Var ou in ous)
            {
                innerCounter = 0;
                foreach (double d in ((IVal)ou).Values)
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
public class Direct : IMap
{
    public readonly static Direct Dummy = new(x => 0);
    private readonly Var vCache = new(0);
    public IVar Cache => vCache;
    Func<double, double> map;
    public Direct(Func<double, double> f)
    {
        map = f;
    }

    public IVal Evaluate(params double[] args)
    {
        Cache.Set(map.Invoke(args[0]));
        return Cache;
    }
    public IVal Evaluate(double a=0)
    {
        Cache.Set(map.Invoke(a));
        return Cache;
    }
}