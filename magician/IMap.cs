using Magician.Renderer;
using static Magician.Geo.Create;

namespace Magician
{
    public interface IMap
    {
        public abstract double Evaluate(double x);

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
        public IMap Compose(IMap imap)
        {
            return new DirectMap(x => Evaluate(imap.Evaluate(x)));
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
                    tmp.parent = m;
                    double p = Evaluate(i);
                    // Parametric Multi placement
                    // TODO: re-implement parametric plotting
                    /*
                    if (p.Length > 1)
                    {
                        m.Add(tmp.Copy().Positioned(p[0] + tmp.X.Evaluate(), p[1] + tmp.Y.Evaluate()));
                    }
                    */
                    m.Add(tmp.Copy().Positioned(i + tmp.X.Evaluate(), p + tmp.Y.Evaluate()));
                }
            }
            m.parent = Geo.Ref.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }
        // Place characters text, rendered char-by-char along an IMap according to some truth function
        public Multi TextAlong(double lb, double ub, double dx, string msg, Color? c = null, double xOffset = 0, double yOffset = 0, Func<double, double>? truth = null, double threshold = 0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            if (c is null)
            {
                c = Globals.UIDefault.FG;
            }

            Multi m = new Multi(xOffset, yOffset);
            int j = 0;
            for (double i = lb; i < ub; i += dx)
            {
                // Do not create more multis than characters in the string
                if (j >= msg.Length)
                {
                    break;
                }
                Text tx = new Text(msg.Substring(j, 1), c);
                Texture txr = tx.Render();
                Multi tmp = new Multi().Textured(txr);
                if (truth.Invoke(i) >= threshold)
                {
                    tmp.parent = m;
                    double p = Evaluate(i);
                    m.Add(tmp.Copy().Positioned(i + tmp.X.Evaluate(), p + tmp.Y.Evaluate()));
                }
                j++;
                tx.Dispose();
            }
            m.parent = Geo.Ref.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }

        // Render an IMap to a Multi
        public Multi Plot(double x, double y, double start, double end, double dx, Color c)
        {
            List<Multi> points = new List<Multi>();
            for (double t = start; t < end; t += dx)
            {
                Multi[] ps = Interpolate(t, t + dx);
                //ps[0].Col = c;
                //ps[1].Col = c;
                // TODO: test plotting after cleaning Multi and refactoring IMap/Driver
                ps[0].Colored(c);
                ps[1].Colored(c);
                points.Add(ps[0].DrawFlags(DrawMode.INVISIBLE));

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

    public class DirectMap : IMap
    {
        Func<double, double> f;
        public DirectMap(Func<double, double> f)
        {
            this.f = f;
        }

        public double Evaluate(double x)
        {
            return f.Invoke(x);
        }
    }

    // IMap with an arbitrary amount of input and output dimensions
    public class Multimap : IMap
    {
        // Functionality
        List<IMap> imaps = new List<IMap>();
        int[][]? pairs;

        // Dimensionality
        public int Ins { get; set; }
        public int Outs { get; set; }

        public Multimap(int ins, params IMap[] outs)
        {
            imaps.AddRange(outs);
            Outs = imaps.Count;
            Ins = ins;
        }
        public Multimap(int ins, params Func<double, double>[] outs)
        {
            foreach (Func<double, double> f in outs)
            {
                imaps.Add(new DirectMap(f));
            }
            Ins = ins;
            Outs = imaps.Count;
        }

        public Multimap Paired(int[][] pairs)
        {
            this.pairs = pairs;
            return this;
        }
        // Evaluate function for Multimaps with only one input and one output
        public double Evaluate(double x)
        {
            if (Ins != 1 || Outs != 1)
            {
                throw new InvalidDataException("doesn't work like that, buddy");
            }
            return imaps[0].Evaluate(x);
        }

        // General Evaluate for any number of ins/outs
        public double[] Evaluate(params double[] args)
        {
            int noArgs = args.Length;
            double[] output = new double[Outs];
            if (noArgs != Ins)
            {
                throw new InvalidDataException($"Number of provided arguments ({args.Length}) does not match input dimensionality ({Ins})");
            }

            if (Ins != Outs)
            {
                /* With one input, resolution is trivial. Simply pass the input to each output */
                if (Ins == 1)
                {
                    for (int i = 0; i < Outs; i++)
                    {
                        output.Append(imaps[i].Evaluate(args[0]));
                    }
                }

                /* In all other cases, we need to resolve the multiplex using a pair-mapping */
                else
                {
                    if (pairs == null)
                    {
                        throw new InvalidDataException("Since this Multimap has more outputs than inputs, a pairs must be specified");
                    }
                    return Resolved(pairs).Evaluate(args);
                }
            }

            /* Inputs and out are equal, map 1-1 */
            int counter = 0;
            foreach (double x in args)
            {
                output.Append(imaps[counter++].Evaluate(x));
            }
            return output;
        }

        /*
        // Interpolate method for Multimaps with multiple inputs
        Multi[] Interpolate(double[] ts0, double[] ts1)
        {
            double[] p0 = Evaluate(ts0);
            double[] p1 = Evaluate(ts1);

            Multi m0 = Point(p0[0], p0[1]);
            Multi m1 = Point(p1[0], p1[1]);
            return new Multi[] { m0, m1 };
        }
        // Interpolate method for Multimaps with only one input
        Multi[] Interpolate(double t0, double t1)
        {
            Multi[] ndPs = new Multi[Outs];
            for (int i = 0; i < Outs; i++)
            {
                double[] p0 = Evaluate(t0);
                double[] p1 = Evaluate(t1);

                Multi m0 = Point(p0[0], p0[1])
                Multi m1 = new Multi();

                ndPs.Append(new Multi());
            }
        }
        */

        // Square plot
        public Multi Plot(double x, double y, double start, double end, double dt, Color c)
        {
            // One-input parametric plots
            if (Ins == 1)
            {
                switch (Outs)
                {
                    // Normal plot
                    case 1:
                        return imaps[0].Plot(x, y, start, end, dt, c);

                    // Parametric
                    case 2:
                        Console.WriteLine("two outputs, baby");
                        Multi parametricPlot = new Multi().DrawFlags(DrawMode.PLOT);
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
                        return parametricPlot;

                    // Parametric with hue
                    case 3:
                        break;

                    default:
                        break;
                }
            }
            else
            
            // Square value-plots
            if (Ins == 2)
            {
                switch (Outs)
                {
                    // Rectangular plot via hue
                    case 1:
                        break;

                    // Rectangular plor via value and hue
                    case 2:
                        break;

                    default:
                        break;
                }
            }
            throw new NotImplementedException($"Multiplex plotting for ins: {Ins} not working yet. File an issue at https://github.com/Calendis");
        }

        public Multimap Resolved(int[][] pairs)
        {
            IMap[] resolvedIns = new IMap[Outs];
            int combines = Ins - Outs;

            if (pairs.Length != Math.Abs(combines))
            {
                throw new InvalidDataException("Number of pairs must be equal to the difference of inputs and outputs");
            }

            // More inputs than outputs, so we reduce the number of inputs by composition
            if (combines > 0)
            {
                for (int i = 0; i < combines; i++)
                {
                    IMap in0 = imaps[pairs[i][0]];
                    IMap in1 = imaps[pairs[i][1]];
                    // TODO: allow for arbitrary relations rather than just composition
                    resolvedIns.Append(in0.Compose(in1));
                }

                return new Multimap(Outs, resolvedIns);
            }
            else
            // Fewer inputs than outputs. This is evil
            if (combines < 0)
            {
                throw new InvalidDataException("Cannot resolve!");
            }

            Console.WriteLine("WARNING: IOResolver: nothing to resolve");
            return this;
        }
    }
}