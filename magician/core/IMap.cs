using Magician.Renderer;
using static Magician.Geo.Create;

namespace Magician;
public interface IMap
{
    public abstract bool IsAbs { get; set; }
    public abstract double Evaluate(double x = 0);

    public virtual IMap AsAbsolute()
    {
        IsAbs = true;
        return this;
    }
    //public static IMap Identity = new CustomMap(x => x);

    // IMap operators
    public virtual IMap Add(IMap o)
    {
        throw new NotImplementedException($"Method Add not supported on {this.GetType().Name}");
    }
    public virtual IMap Mult(IMap o)
    {
        throw new NotImplementedException($"Method Mult not supported on {this.GetType().Name}");
    }
    public virtual IMap Derivative()
    {
        throw new NotImplementedException($"Method Derivative not supported on {this.GetType().Name}");
    }
    public virtual IMap Integral()
    {
        throw new NotImplementedException($"Method Integral not supported on {this.GetType().Name}");
    }
    public virtual IMap Concat()
    {
        throw new NotImplementedException($"Method Concat not supported on {this.GetType().Name}");
    }
    // Compose two IMaps :)
    // TODO: is there a better method of doing this?
    public IMap Compose(IMap imap)
    {
        return new CustomMap(x => Evaluate(imap.Evaluate(x)));
    }

    // Place Multis along an IMap according to some truth function
    public Multi MultisAlong(double lb, double ub, double dx, Multi tmp, double xOffset = 0, double yOffset = 0, Func<double, double>? truth = null, double threshold = 0)
    {
        if (truth is null)
        {
            truth = x => 1;
        }
        Multi m = new Multi(xOffset, yOffset);
        for (double i = lb; i < ub; i += dx)
        {
            if (truth.Invoke(i) >= threshold)
            {
                tmp.Parented(m);
                double p = Evaluate(i);
                m.Add(tmp.Copy().Positioned(i + tmp.X, p + tmp.Y));
            }
        }
        m.Parented(Geo.Ref.Origin);
        return m.WithFlags(DrawMode.INVISIBLE);
    }
    // Place characters text, rendered char-by-char along an IMap according to some truth function
    public Multi TextAlong(double lb, double ub, double dx, string msg, Color? c = null, int? size = null, double xOffset = 0, double yOffset = 0, Func<double, double>? truth = null, double threshold = 0)
    {
        if (truth is null)
        {
            truth = x => 1;
        }
        c = c ?? Data.Col.UIDefault.FG;
        size = size ?? Data.Globals.fontSize;

        Multi m = new Multi(xOffset, yOffset);
        int j = 0;
        for (double i = lb; i < ub; i += dx)
        {
            // Do not create more multis than characters in the string
            if (j >= msg.Length)
            {
                break;
            }
            if (truth.Invoke(i) <= threshold)
            {
                continue;
            }

            Text tx = new Text(msg.Substring(j, 1), c, (int)size);
            _SDLTexture txr = tx.Render();

            Multi tmp = new Multi().Textured(txr);
            if (truth.Invoke(i) >= threshold)
            {
                tmp.Parented(m);
                double p = Evaluate(i);
                m.Add(
                    tmp.Positioned(i, p)
                );
            }
            j++;
            tx.Dispose();
        }
        m.Parented(Geo.Ref.Origin);
        return m.WithFlags(DrawMode.INVISIBLE);
    }

    // Render an IMap to a Multi
    public Multi Plot(double x, double y, double start, double end, double dx, Color c)
    {
        List<Multi> points = new List<Multi>();
        for (double t = start; t < end; t += dx)
        {
            Multi[] ps = Interpolate(t, t + dx);
            ps[0].Colored(c);
            ps[1].Colored(c);
            points.Add(ps[0].WithFlags(DrawMode.INVISIBLE));

        }
        Multi m = new Multi(x, y, c, DrawMode.PLOT, points.ToArray());
        return m;
    }

    Multi[] Interpolate(double t0, double t1)
    {
        double y0 = Evaluate(t0);
        double y1 = Evaluate(t1);

        Multi p0 = Point(t0, y0);
        Multi p1 = Point(t1, y1);
        return new Multi[] { p0, p1 };
    }
}

public class CustomMap : IMap
{
    Func<double, double> f;
    bool isAbs;
    public bool IsAbs
    {
        get => isAbs;
        set => isAbs = value;
    }
    public CustomMap()
    {
        f = x => x;
        IsAbs = false;
    }
    public CustomMap(Func<double, double> f)
    {
        this.f = f;
        IsAbs = false;
    }

    public double Evaluate(double x)
    {
        return f.Invoke(x);
    }
}

// IMap with an arbitrary amount of input and output dimensions
// TODO: Maybe IMaps should implement IOMap, not the other way around
public class ParametricMap : IMap
{
    // Functionality
    List<IMap> imaps = new List<IMap>();
    int[][]? pairs;
    public bool IsAbs { get; set; }

    // Dimensionality
    public int Outs { get; set; }

    public ParametricMap(params IMap[] outs)
    {
        imaps.AddRange(outs);
        Outs = imaps.Count;
    }
    public ParametricMap(params Func<double, double>[] outs)
    {
        foreach (Func<double, double> f in outs)
        {
            imaps.Add(new CustomMap(f));
        }
        Outs = imaps.Count;
    }

    // Parametric 2D Multisalong
    public Multi MultisAlong(double lb, double ub, double dx, Multi tmp, double xOffset = 0, double yOffset = 0, double zOffset = 0, Func<double, double>? truth = null, double threshold = 0)
    {
        if (truth is null)
        {
            truth = x => 1;
        }
        Multi m = new Multi(xOffset, yOffset);
        for (double i = lb; i < ub; i += dx)
        {
            if (truth.Invoke(i) <= threshold)
            {
                continue;
            }
            tmp.Parented(m);

            double[] out0 = new double[3];
            out0[0] = imaps[0].Evaluate(i);
            out0[1] = imaps[1].Evaluate(i);
            out0[2] = 0;
            if (imaps.Count >= 3)
            {
                out0[2] = imaps[2].Evaluate(i);
            }

            m.Add(
                tmp.Copy().Positioned(out0[0] + tmp.X + xOffset, out0[1] + tmp.Y + xOffset, out0[2] + tmp.Z + zOffset)
            );

        }
        m.Parented(Geo.Ref.Origin);
        return m.WithFlags(DrawMode.INVISIBLE);
    }
    public Multi TextAlong(double lb, double ub, double dx, string msg, Color? c = null, int? size = null, double xOffset = 0, double yOffset = 0, Func<double, double>? truth = null, double threshold = 0)
    {
        if (truth is null)
        {
            truth = x => 1;
        }
        c = c ?? Data.Col.UIDefault.FG;
        size = size ?? Data.Globals.fontSize;

        Multi m = new Multi(xOffset, yOffset);
        int j = 0;
        for (double i = lb; i < ub; i += dx)
        {
            // Do not create more multis than characters in the string
            if (j >= msg.Length)
            {
                break;
            }
            if (truth.Invoke(i) <= threshold)
            {
                continue;
            }
            Text tx = new Text(msg.Substring(j, 1), c, (int)size);
            _SDLTexture txr = tx.Render();

            Multi tmp = new Multi().Textured(txr).Tagged(msg.Substring(j, 1));
            //tmp.parent=m;
            double[] out0 = new double[2];
            out0[0] = imaps[0].Evaluate(i);
            out0[1] = imaps[1].Evaluate(i);
            m.Add(
                tmp.Positioned(out0[0] + xOffset, out0[1] + yOffset)
            );
            tx.Dispose();
            j++;
        }
        return m.WithFlags(DrawMode.INVISIBLE);
    }

    public ParametricMap Paired(int[][] pairs)
    {
        this.pairs = pairs;
        return this;
    }

    public double[] Evaluate()
    {
        return Evaluate(new double[] { });
    }
    public double Evaluate(double x)
    {
        if (Outs != 1)
        {
            throw Scribe.Issue("This should never occur");
        }
        return imaps[0].Evaluate(x);
    }

    // General Evaluate for any number of ins/outs
    public double[] Evaluate(double[] args)
    {
        int noArgs = args.Length;
        double[] output = new double[Outs];

        if (1 < Outs)
        {
            for (int i = 0; i < Outs; i++)
            {
                output[i] = imaps[i].Evaluate(args[0]);
            }
        }
        else if (1 == Outs)
        {
            /* Inputs and out are equal, map 1-1 */
            int counter = 0;
            foreach (double x in args)
            {
                output[counter] = (imaps[counter++].Evaluate(x));
            }
        }

        return output;
    }

    public Multi Plot(double x, double y, double start, double end, double dt, Color c)
    {
        switch (Outs)
        {
            // Normal plot
            case 1:
                return imaps[0].Plot(x, y, start, end, dt, c);

            // Parametric
            case 2:
                Multi parametricPlot = new Multi().WithFlags(DrawMode.PLOT);
                for (double t = start; t < end; t += dt)
                {
                    double[] out0 = new double[2];
                    out0[0] = imaps[0].Evaluate(t);
                    out0[1] = imaps[1].Evaluate(t);

                    double[] out1 = new double[2];
                    out1[0] = imaps[0].Evaluate(t + dt);
                    out1[1] = imaps[1].Evaluate(t + dt);

                    parametricPlot.Add(
                        Point(out0[0], out0[1]).Colored(c),
                        Point(out1[0], out1[1]).Colored(c)
                    );

                }
                return parametricPlot.Positioned(x, y);

            // Parametric with hue
            case 3:
                break;

            default:
                break;
        }
        throw new NotImplementedException($"Multiplex plotting for outs: {Outs} not working yet. File an issue at https://github.com/Calendis");
    }
}