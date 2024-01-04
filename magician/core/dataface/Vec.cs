// TODO: don't rely on these
using Magician.Symbols;
using Silk.NET.Maths;

namespace Magician.Geo
{
    public class Vec : IMultival
    {
        protected IVal[] vecArgs;
        IVal[] IMultival.Values => vecArgs;

        // TODO: avoid this dumb-ass pattern
        int Dims => ((IMultival)this).Dims;
        public Vec(params double[] vals)
        {
            
            vecArgs = new ValWrapper[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                vecArgs[i] = new ValWrapper(vals[i]);
            }
        }
        public Vec(params IVal[] qs)
        {
            vecArgs = qs;
        }

        public IVal x
        {
            get => vecArgs[0];
        }
        public IVal y
        {
            get => vecArgs[1];
        }
        public IVal z
        {
            get => vecArgs[2];
        }
        public IVal w
        {
            get => vecArgs[3];
        }

        public double Magnitude
        {
            get
            {
                double m = 0;
                for (int i = 0; i < Dims; i++)
                {
                    m += Math.Pow(vecArgs[i].Get(), 2);
                }
                return Math.Sqrt(m);
            }
            set
            {
                double m = Magnitude;
                Normalize();
                foreach (IVal q in vecArgs)
                {
                    q.Set(q * value);
                }
            }
        }

        public void Normalize()
        {
            double m = Magnitude;
            foreach (IVal q in vecArgs)
            {
               q.Set(q / m);
            }
        }

        public override string ToString()
        {
            string s = "(";
            foreach (IVal q in vecArgs)
            {
                s += $"{q}, ";
            }
            s = String.Concat(s.SkipLast(2));
            return s + ")";
        }

        public Vec3 ToVec3()
        {
            if (Dims != 3)
                throw Scribe.Error($"Could not convert {this} to Vec3");
            if (x.Dims == 3)
                return new(x.Get(0), x.Get(1), x.Get(2));
            return new(x.Get(), y.Get(), z.Get());
        }
    }
}
