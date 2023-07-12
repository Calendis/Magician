// TODO: don't rely on these
using Silk.NET.Maths;

namespace Magician.Geo
{
    public class Vec
    {
        protected Quantity[] vecArgs;
        public int Dims => vecArgs.Length;
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
        public Quantity w
        {
            get => vecArgs[3];
        }

        public static Vec operator +(Vec v1, Vec v2)
        {
            return new(v1.vecArgs.Select((x, i) => x + v2.vecArgs[i]).ToArray());
        }
        public static Vec operator -(Vec v1, Vec v2)
        {
            return new(v1.vecArgs.Select((x, i) => x - v2.vecArgs[i]).ToArray());
        }
        // Scalar multiplication
        public static Vec operator *(Vec v1, double x)
        {
            return new(v1.vecArgs.Select(va => va.Evaluate() * x).ToArray());
        }

        public double Magnitude()
        {
            double m = 0;
            for (int i = 0; i < Dims; i++)
            {
                m += Math.Pow(vecArgs[i].Evaluate(), 2);
            }
            return Math.Sqrt(m);
        }

        public Vec Normalized()
        {
            double m = Magnitude();
            List<double> news = new();
            foreach (Quantity q in vecArgs)
            {
                news.Add(q.Evaluate() / m);
            }
            return new(news.ToArray());
        }

        public override string ToString()
        {
            string s = "(";
            foreach (Quantity q in vecArgs)
            {
                s += $"{q.Evaluate()}, ";
            }
            s = String.Concat(s.SkipLast(2));
            return s + ")";
        }
    }
}
