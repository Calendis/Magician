using System;

namespace Magician.Geo
{
    public class Vec
    {
        Quantity[] vecArgs;
        int Dims => vecArgs.Length;
        public Vec(params double[] vals)
        {
            vecArgs = new Quantity[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                vecArgs[i] = new Quantity(vals[i]);
            }
        }
        public Vec(params Quantity[] qs)
        {
            vecArgs = qs;
        }

        public void AssignVector(params Quantity[] qs)
        {
            if (qs.Length != Dims)
            {
                throw Scribe.Error($"Invalid number of arguments {qs.Length} to {Dims}-vector");
            }
            for (int i = 0; i < Dims; i++)
            {
                vecArgs[i] = qs[i];
            }
        }
        public Quantity x
        {
            get => vecArgs[0];
        }
        public Quantity y
        {
            get => vecArgs[1];
        }
        public Quantity z
        {
            get => vecArgs[2];
        }

        /* Measured phase */
        public virtual double PhaseZ
        {
            get
            {
                double p = Math.Atan2(y.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public virtual double PhaseY
        {
            get
            {
                double p = Math.Atan2(z.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public virtual double PhaseX
        {
            get
            {
                double p = Math.Atan2(z.Evaluate(), y.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        
        public double MagnitudeZ
        {
            get => Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate());
        }
        public double MagnitudeX
        {
            get => Math.Sqrt(z.Evaluate() * z.Evaluate() + y.Evaluate() * y.Evaluate());
        }
        public double MagnitudeY
        {
            get => Math.Sqrt(x.Evaluate() * x.Evaluate() + z.Evaluate() * z.Evaluate());
        }

        public static Vec operator -(Vec v1, Vec v2)
        {
            return new(v1.vecArgs.Select((x, i) => x-v2.vecArgs[i]).ToArray());
        }

        public Vec Cross(Vec v)
        {
            if (Dims != v.Dims)
            {
                throw Scribe.Error("Vectors must be of same length");
            }
            double[] results = new double[Dims];
            for (int i = 0; i < Dims; i++)
            {
                int j = (i + 1) % Dims;
                int k = (i + 2) % Dims;
                results[i] = vecArgs[j].Evaluate() * v.vecArgs[k].Evaluate() - vecArgs[k].Evaluate() * v.vecArgs[j].Evaluate();
            }
            return new Vec(results);
        }

    }
}
