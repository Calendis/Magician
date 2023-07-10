namespace Magician;
using static Magician.Geo.Create;

// General equation with multiple branches
public class NeoMap
{
    Func<double[], double[]> map;
    public NeoMap(Func<double[], double[]> m)
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

// General quation
public class InverseParamMap : NeoMap
{
    int ins;
    int outs;
    public InverseParamMap(Func<double[], double> f) : base(xs => new double[]{f.Invoke(xs)}) {}

    public new double Evaluate(double[] xs)
    {
        return base.Evaluate(xs)[0];
    }

    public override Multi Plot(double x, double y, double z, double start, double end, double dt, Color c)
    {
        throw Scribe.Error("not implemented. Use an Equation instead");
    }
}

// Parametric equation
public class ParamMap : NeoMap
{
    public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray()) { }

    public double[] Evaluate(double x)
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
            double[] pos0 = {0, 0, 0};
            double[] pos1 = {0, 0, 0};
            double[][] outs = new[]{out0, out1};
            double[][] poss = new[]{pos0, pos1};
            int counter = 0;
            int innerCounter;
            foreach (double[] ou in outs)
            {
                innerCounter = 0;
                foreach(double d in ou)
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
    public new double Evaluate(double x)
    {
        return base.Evaluate(x)[0];
    }
}