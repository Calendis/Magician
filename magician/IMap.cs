namespace Magician
{
    public interface IMap
    {
        public abstract double[] Evaluate(params double[] x);
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

        // Place Multis along an IMap according to some truth function
        public Multi MultisAlong(double lb, double ub, double dx, Multi tmp, double xOffset=0, double yOffset=0, Func<double, double>? truth=null, double threshold=0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            Multi m = new Multi(xOffset, yOffset);
            for (double i = lb; i < ub; i+=dx)
            {
                if (truth.Invoke(i) >= threshold)
                {
                    tmp.parent = m;
                    double[] p = Evaluate(new double[]{i});
                    if (p.Length > 1)
                    {
                        m.Add(tmp.Copy().Positioned(p[0]+tmp.X.Evaluate(), p[1]+tmp.Y.Evaluate()));
                    }
                    else
                    {
                        m.Add(tmp.Copy().Positioned(i+tmp.X.Evaluate(), p[0]+tmp.Y.Evaluate()));
                    }
                }
            }
            m.parent = Geo.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }

        // Render an IMap to a Multi
        public Multi Plot(double x, double y, double start, double end, double dx, Color c)
        {
            List<Multi> points = new List<Multi>();
            for (double t = start; t < end; t+=dx)
            {
                Multi[] ps = interpolate(t, t+dx);
                ps[0].Col = c;
                ps[1].Col = c;
                points.Add(ps[0]);

            }
            Multi m = new Multi(x, y, c, DrawMode.PLOT, points.ToArray());
            return m;
        }

        private Multi[] interpolate(double t0, double t1)
        {            
            double[] p0 = Evaluate(new double[] {t0});
            double[] p1 = Evaluate(new double[] {t1});
            
            if (p0.Count() < 2)
            {
                p0 = new double[]{t0, p0[0]};
            }
            if (p1.Count() < 2)
            {
                p1 = new double[]{t1, p1[0]};
            }


            Multi mp0 = Geo.Point(p0[0], p0[1]);
            Multi mp1 = Geo.Point(p1[0], p1[1]);
            return new Multi[] {mp0, mp1};
        }
    }
}