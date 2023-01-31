using System.Collections;

namespace Magician
{
    public class Seq : IMap, ICollection<double>
    {
        // If a generator is specified, the sequence can be shifted/extended when necessary
        // If no generator is specifed, the sequence may lose information when shifting/extending
        IMap? generator;
        protected double[] s;
        public double Offset{get; set;}
        
        // Seq from literals
        public Seq(params double[] s)
        {
            this.s = new double[s.Length];
            s.CopyTo(this.s, 0);
        }
        // Seq from 1D generator
        public Seq(IMap g, double start, double end, double dn)
        {
            generator = g;
            int range = (int)(end - start);
            int steps = (int)(range / dn);
            s = new double[range];
            for (double i = start; i < end; i += dn)
            {
                s.Append(g.Evaluate(i));
            }
        }

        public virtual double Evaluate(double x)
        {
            double d;
            try
            {
                d = s[(int)x];
            }
            catch (IndexOutOfRangeException)
            {
                d = 0;
            }

            return d;
        }
        public virtual double[] Evaluate(double[] offsets)
        {
            double[] outputs = new double[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                outputs[i] = Evaluate(offsets[i]) + outputs[i];
            }
            return outputs;
        }

        // ICollection properties/methods
        public int Count
        {
            get => s.Length;
        }
        public void Add(double x)
        {
            double[] newS = new double[s.Length + 1];
            s.CopyTo(newS, 0);
            newS[s.Length] = x;
            s = newS;
        }

        // TODO: test this
        public bool Remove(double x)
        {
            int done = 0;
            double[] newS = new double[s.Length-1];
            
            for (int i = 0; i < s.Length; i++)
            {
                double d = s[i];
                if (d == x && done == 0)
                {
                    done = 1;
                    continue;
                }
                newS[i] = s[i-done];
            }
            return done == 1;
        }

        public void Clear()
        {
            s = new double[] {};
        }

        public bool Contains(double d)
        {
            return s.Contains(d);
        }

        public void CopyTo(double[] ds, int i)
        {
            s.CopyTo(ds, i);
        }

        public bool IsReadOnly
        {
            get => false;
        }

        public IEnumerator<double> GetEnumerator()
        {
            return (IEnumerator<double>)(s.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return s.GetEnumerator();
        }
    }
    
    public class Polynomial : Seq
    {
        public Polynomial(params double[] s) : base(s) {}

        public override double Evaluate(double x)
        {
            double y = 0;
            for (int i = 0; i < Count; i++)
            {
                y += Math.Pow(x, i);
            }
            return y;
        }
    }

    public class Taylor : Seq
    {
        public Taylor(params double[] s) : base(s) {}

        public override double Evaluate(double x)
        {
            double y = 0;
            for (int i = 0; i < s.Length; i++)
            {
                throw new NotImplementedException("Taylor not supported");
                //y+= Math.Pow(x, i) / Math.Factorial(i);
            }
            return y;
        }
    }
}
