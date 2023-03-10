using System;

namespace Magician.Geo
{
    public class Vec : IOMap
    {
        int dims;
        Quantity[] v;
        enum VecMode
        {
            XY,
            PM
        }
        public Vec(int d, params double[] vals) : base(1)  // TODO: test if 0 ins works
        {
            if (d != vals.Length)
            {
                throw Scribe.Error($"Cannot store {vals.Length} values in {dims}-dimensional vector");
            }
            dims = d;
            v = new Quantity[dims];
            for (int i = 0; i < dims; i++)
            {
                v[i] = new Quantity(vals[i]);
            }
        }

        public Quantity x
        {
            get => v[0];
            set => v[0].From(value);
        }
        public Quantity y
        {
            get => v[1];
            set => v[1].From(value);
        }
        public Quantity z
        {
            get => v[2];
            set => v[2].From(value);
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

    }
}
