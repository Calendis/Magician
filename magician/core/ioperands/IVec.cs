namespace Magician.Core;

public interface IVec : IDimensional<IVal>
{
    //abstract List<IVal> IDimensional<IVal>.Values { get; }
    //int IDimensional<IVal>.Dims { get => Values.Select(v => v.Dims).Sum(); }

    double IDimensional<IVal>.Magnitude
    {
        get
        {
            double m = 0;
            for (int i = 0; i < ((IVec)this).Dims; i++)
            {
                m += Math.Pow(Values[i].Magnitude, 2);
            }
            return Math.Sqrt(m);
        }
        set
        {
            Normalize();
            foreach (IVal q in Values)
            {
                q.Set(q * value);
            }
        }
    }

    void IDimensional<IVal>.Normalize()
    {
        double m = Magnitude;
        foreach (IVal q in Values)
        {
            q.Set(q / m);
        }
    }

    public Algebra.Symbols.Variable ToVariable()
    {
        List<double> vs = new();
        foreach (IVal iv in Values)
        {
            vs.AddRange(iv.Values);
        }
        return new Algebra.Symbols.Variable(vs.ToArray());
    }

    public static IVec operator +(IVec i, IVec v)
    {
        return new Vec(i.Values.Zip(v.Values, (a, b) => a + b).ToArray());
    }
    public static IVec operator -(IVec i, IVec v)
    {
        return new Vec(i.Values.Zip(v.Values, (a, b) => a - b).ToArray());
    }
    public static IVec operator *(IVec i, IVal x)
    {
        return new Vec(i.Values.Select(k => k * x).ToArray());
    }
    public static IVec operator /(IVec i, IVal x)
    {
        return new Vec(i.Values.Select(k => k / x).ToArray());
    }

    public static IVec operator *(IVec i, double x)
    {
        return new Vec(i.Values.Select(k => k * x).ToArray());
    }
    public static IVec operator /(IVec i, double x)
    {
        return new Vec(i.Values.Select(k => k / x).ToArray());
    }

    public static IVec operator +(IVec i, IVal x)
    {
        return new Vec(i.Values.Select(k => k + x).ToArray());
    }
    public static IVec operator -(IVec i, IVal x)
    {
        return new Vec(i.Values.Select(k => k - x).ToArray());
    }
}

public class Vec : IVec
{
    protected List<IVal> vecArgs = new();
    List<IVal> IDimensional<IVal>.Values => vecArgs;
    //public IVec V => this;

    public Vec(params double[] vals)
    {
        if (vals.Length == 0)
            throw Scribe.Error("Cannot create empty Vec");
        for (int i = 0; i < vals.Length; i++)
        {
            vecArgs.Add(new Val(vals[i]));
        }
    }
    public Vec(params IVal[] qs)
    {
        if (qs.Length == 0)
            throw Scribe.Error("Cannot create empty Vec");
        vecArgs = qs.ToList();
    }
    public Vec(IVec v)
    {
        vecArgs = v.Values.ToList();
    }

    public void Normalize()
    {
        ((IDimensional<IVal>)this).Normalize();
    }
    public double Magnitude
    {
        get
        {
            return ((IDimensional<IVal>)this).Magnitude;
        }
        set
        {
            ((IDimensional<IVal>)this).Magnitude = value;
        }
    }
    double IDimensional<IVal>.Magnitude
    {
        get
        {
            double m = 0;
            for (int i = 0; i < ((IVec)this).Dims; i++)
            {
                m += Math.Pow(vecArgs[i].Magnitude, 2);
            }
            return Math.Sqrt(m);
        }
        set
        {
            Normalize();
            foreach (IVal q in vecArgs)
            {
                q.Set(q * value);
            }
        }
    }

    void IDimensional<IVal>.Normalize()
    {
        double m = Magnitude;
        foreach (IVal q in vecArgs)
        {
            q.Set(q / m);
        }
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

    // TODO: clarify this
    public Geo.Vec3 ToVec3()
    {
        if (((IVec)this).Dims != 3)
            throw Scribe.Error($"Could not convert {this} to Vec3");
        if (x.Dims == 3)
            return new(x.Get(0), x.Get(1), x.Get(2));
        return new(x.Get(), y.Get(), z.Get());
    }
}

