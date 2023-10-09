namespace Magician.Maps;

using Magician.Symbols;
using static Magician.Geo.Create;

public interface IRelation
{
    public double[] Evaluate(params double[] args);
    public int Ins { get; protected set; }
    public int Outs { get; protected set; }
}
public interface IFunction : IRelation
{
    int IRelation.Outs { get { return 1; } set { } }
    public new double Evaluate(params double[] args);
    double[] IRelation.Evaluate(params double[] args)
    {
        return new[] { Evaluate(args) };
    }
}
public interface IParametric : IRelation
{
    int IRelation.Ins { get { return 1; } set { } }
    public double[] Evaluate(double x);
    double[] IRelation.Evaluate(params double[] args)
    {
        return new[] { Evaluate(args[0]) }[0];
    }
}
public interface IMap : IFunction, IParametric
{
    public new double Evaluate(double x);
    double[] IParametric.Evaluate(double x)
    {
        return new[] { Evaluate(x) };
    }
    double IFunction.Evaluate(params double[] args)
    {
        return Evaluate(args[0]);
    }
    double[] IRelation.Evaluate(params double[] args)
    {
        return new[] { Evaluate(args[0]) };
    }
}

// Multiple inputs, multiple outputs
// Plottable when the number of inputs and outputs are specified
public class RelationalMap : IRelation
{
    protected Func<double[], double[]>? map;
    public int Ins { get; set; }
    public int Outs { get; set; }
    public RelationalMap(int ins, int outs, Func<double[], double[]>? m = null)
    {
        map = m;
        Ins = ins;
        Outs = outs;
    }

    public double[] Evaluate(double[] xs)
    {
        if (map is null)
            throw Scribe.Issue("Null relational map");
        return map.Invoke(xs);
    }
    //public virtual Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    public virtual Multi Plot(params Symbols.PlotOptions[] options)
    {
        throw Scribe.Issue("RelationalMap plotting not implemented");
    }

    double[] IRelation.Evaluate(params double[] args)
    {
        throw new NotImplementedException();
    }
}

// Multiple inputs, one output
// Plottable when the number of inputs is specified
public class InverseParamMap : IFunction
{
    Func<double[], double> map;
    public int Ins { get; set; }
    //public InverseParamMap(Func<double[], double>? f = null, int inputs = -1) : base(f is null ? null : xs => new double[] { f.Invoke(xs) })
    public InverseParamMap(Func<double[], double> f, int inputs)
    {
        Ins = inputs;
        map = f;
    }

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
            double outVal;
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
                : (int)outAxis] = outVal;

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
    public static Func<double[], double[]> MapFromIPFunc(Func<double[], double> f)
    {
        return xs => new double[] { f.Invoke(xs) };
    }

    public double Evaluate(params double[] args)
    {
        return map.Invoke(args);
    }

    double[] IRelation.Evaluate(params double[] args)
    {
        return new double[] { map.Invoke(args) };
    }
}

// Parametric equation. One or fewer input, multiple outputs
// Always plottable, as the number of outputs is always known
public class ParamMap : IParametric
{
    //public int Params { get; set; }
    public int Outs { get; set; }
    public Func<double, double>[] Maps;
    //public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray())
    public ParamMap(params Func<double, double>[] fs)
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
    //public ParamMap(params DirectMap[] fs) : base(xs => fs.Select(m => m.Evaluate(xs[0])).ToArray())
    public ParamMap(params IMap[] fs) : this(fs.Select<IMap, Func<double, double>>(im => im.Evaluate).ToArray()) { }
    public double[] Evaluate(double x = 0)
    {
        return Maps.Select(f => f.Invoke(x)).ToArray();
    }

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
            double[] out0 = Evaluate(t);
            double[] out1 = Evaluate(t + dt);
            double[] pos0 = { 0, 0, 0 };
            double[] pos1 = { 0, 0, 0 };
            double[][] ous = new[] { out0, out1 };
            double[][] poss = new[] { pos0, pos1 };
            int counter = 0;
            int innerCounter;
            foreach (double[] ou in ous)
            {
                innerCounter = 0;
                foreach (double d in ou)
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

        return plot.Positioned(x, y, z);
    }

}

// One or fewer input, one output
// Always plottable
public class DirectMap : IMap
{
    Func<double, double> map;
    public DirectMap(Func<double, double> f)
    {
        map = f;
    }
    public double Evaluate(double x = 0)
    {
        return map.Invoke(x);
    }
}

public static class Maps
{
    public static Random rng = new();
}