namespace Magician;
using static Magician.Geo.Create;

// General equation with multiple branches
public class RelationalMap
{
    Func<double[], double[]> map;
    public RelationalMap(Func<double[], double[]> m)
    {
        map = m;
    }
    public double[] Evaluate(double[] xs)
    {
        return map.Invoke(xs);
    }
    public virtual Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    {
        throw Scribe.Error("Not implemented");
    }
}

// General equation
public class InverseParamMap : RelationalMap
{
    int ins;
    int outs;
    public InverseParamMap(Func<double[], double> f) : base(xs => new double[] { f.Invoke(xs) }) { }

    public new double Evaluate(double[] xs)
    {
        return base.Evaluate(xs)[0];
    }

    public override Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    {
        // TODO: move plotting code here
        throw Scribe.Error("not implemented. Use an Equation instead");
    }
}

// Parametric equation
public class ParamMap : RelationalMap
{
    public int Params {get; set;}
    public Func<double, double>[] Maps;
    public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray())
    {
        Params = fs.Length;
        Func<double, double>[] fs2 = new Func<double, double>[fs.Length];
        int c = 0;
        foreach (Func<double, double> f in fs)
        {
            fs2[c++] = f.Invoke;
        }
        Maps = fs2;
    }
    public ParamMap(params DirectMap[] fs) : base(xs => fs.Select(m => m.Evaluate(xs[0])).ToArray())
    {
        Params = fs.Length;
        Func<double, double>[] fs2 = new Func<double, double>[fs.Length];
        int c = 0;
        foreach (DirectMap dm in fs)
        {
            fs2[c++] = dm.Evaluate;
        }
        Maps = fs2;
    }

    public double[] Evaluate(double x=0)
    {
        return base.Evaluate(new double[] { x });
    }
    public override Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    {
        Multi plot = new Multi().WithFlags(DrawMode.PLOT);
        for (double t = start; t < end; t += dt)
        {
            double[] out0 = Evaluate(t);
            double[] out1 = Evaluate(t + dt);
            double[] pos0 = { 0, 0, 0 };
            double[] pos1 = { 0, 0, 0 };
            double[][] outs = new[] { out0, out1 };
            double[][] poss = new[] { pos0, pos1 };
            int counter = 0;
            int innerCounter;
            foreach (double[] ou in outs)
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

public class DirectMap : ParamMap
{
    public DirectMap(Func<double, double> f) : base(f) { }
    public new double Evaluate(double x=0)
    {
        return base.Evaluate(x)[0];
    }
}