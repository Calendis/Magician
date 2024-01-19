using Magician.Core;
using Magician.Core.Maps;

namespace Magician.Algebra;
public class Seq : DirectMap
{
    // If a generator is specified, the sequence can be shifted/extended when necessary
    // If no generator is specifed, the sequence may lose information when shifting/extending
    DirectMap? generator;
    //protected double[]? seq;
    public int Length;
    public double Offset { get; set; }

    // Seq from literals
    public Seq(params double[] s) : base(x => 
    {
        //IVal d = new Val(0);
        try
        {
            //d.Set(s[(int)x]);
            s[(int)x].GetType();
        }
        catch (IndexOutOfRangeException)
        {
            //d.Set(0);
            return 0;
        }
        //return d.Get();
        return s[(int)x];
    })
    {
        Length = s.Length;
        //seq = new double[s.Length];
        //s.CopyTo(seq, 0);
    }
    // Lazy seq from 1D generator
    public Seq(DirectMap g) : base(x => g.Evaluate(x).Get())
    {
        generator = g;
    }

    // ICollection properties/methods
    //public int Count
    //{
    //    get => seq.Length;
    //}
    // TODO: make Seqs immutable

    //public void Add(double x)
    //{
    //    double[] newS = new double[seq.Length + 1];
    //    seq.CopyTo(newS, 0);
    //    newS[seq.Length] = x;
    //    seq = newS;
    //}
//
    //// TODO: test this
    //public bool Remove(double x)
    //{
    //    int done = 0;
    //    double[] newS = new double[seq.Length - 1];
//
    //    for (int i = 0; i < seq.Length; i++)
    //    {
    //        double d = seq[i];
    //        if (d == x && done == 0)
    //        {
    //            done = 1;
    //            continue;
    //        }
    //        newS[i] = seq[i - done];
    //    }
    //    return done == 1;
    //}
//
    //public void Clear()
    //{
    //    seq = new double[] { };
    //}
//
    //public bool Contains(double d)
    //{
    //    return seq.Contains(d);
    //}
//
    //public void CopyTo(double[] ds, int i)
    //{
    //    seq.CopyTo(ds, i);
    //}
//
    //public bool IsReadOnly
    //{
    //    get => false;
    //}
//
    //public IEnumerator<double> GetEnumerator()
    //{
    //    return (IEnumerator<double>)(seq.GetEnumerator());
    //}
//
    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //    return seq.GetEnumerator();
    //}
}

public class Polynomial : Seq
{
    public Polynomial(params double[] s) : base(s) { }

    public new double Evaluate(double x)
    {
        double y = 0;
        for (int i = 0; i < Length; i++)
        {
            y += Math.Pow(x, i);
        }
        return y;
    }
}

public class Taylor : Seq
{
    public Taylor(params double[] s) : base(s) { }

    public new double Evaluate(double x)
    {
        double y = 0;
        for (int i = 0; i < Length; i++)
        {
            throw new NotImplementedException("Taylor not supported");
            //y+= Math.Pow(x, i) / Math.Factorial(i);
        }
        return y;
    }
}