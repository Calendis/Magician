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
            m.parent = Multi.Origin;
            return m.DrawFlags(DrawMode.INVISIBLE);
        }
    }
}