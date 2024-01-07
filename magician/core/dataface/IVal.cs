namespace Magician.Core;

public interface IVal : IDimensional<double>
{
    abstract List<double> IDimensional<double>.Values {get;}
    int IDimensional<double>.Dims => Values.Count;
    public double Get(int i = 0) => Values[i];
    public void Set(params double[] vs);
    public void Set(IVal iv)
    {
        for (int i = 0; i < Dims; i++)
        {
            Values[i] = iv.Values[i];
        }
    }
    public bool EqValue(IVal other)
    {
        if (Dims != other.Dims)
            if (Trim().Get() == other.Trim().Get() && Trim().Dims == other.Trim().Dims)
                return true;
            else
                return false;
        for (int i = 0; i < Dims; i++)
            if (Values[i] != other.Values[i])
                return false;
        return true;
    }
    double IDimensional<double>.Magnitude
    {
        get
        {
            double total = 0;
            foreach (double x in Values)
            {
                total += x * x;
            }
            return Math.Sqrt(total);
        }
    }

    public IVal Delta(params double[] vs)
    {
        Set(this + new Num(vs));
        return this;
    }

    public IVal Trim()
    {
        if (Values is null)
            return this;
        bool nz = false;
        foreach (double d in Values)
            if (d != 0)
            {
                nz = true;
                break;
            }
        if (!nz)
            return new Num(0);
        if (Dims < 2)
            return this;
        int toTrim = 0;
        for (int i = 0; i < Dims; i++)
        {
            int j = Dims - i - 1;
            if (Get(j) == 0)
                toTrim++;
            else
                break;
        }
        return new Num(Values.SkipLast(toTrim).ToArray());
    }

    public static bool operator <(IVal iv0, IVal iv1)
    {
        if (iv0.Dims * iv1.Dims == 1)
            return iv0.Get() < iv1.Get();
        return iv0.Magnitude < iv1.Magnitude;
    }
    public static bool operator >(IVal iv0, IVal iv1)
    {
        return !(iv0 < iv1);
    }

    public static IVal operator +(IVal i, double x)
    {
        double[] newAll = i.Values.ToArray();
        newAll[0] += x;
        return new Num(newAll);
    }
    public static IVal operator -(IVal i, double x)
    {
        double[] newAll = i.Values.ToArray();
        newAll[0] -= x;
        return new Num(newAll);
    }
    // Multiplication supported for one or two dimensions
    public static IVal operator *(IVal i, double x)
    {
        return new Num(i.Values.Select(k => k * x).ToArray());
    }
    public static IVal operator /(IVal i, double x)
    {
        return new Num(i.Values.Select(k => k / x).ToArray());
    }
    
    public static IVal operator +(IVal i, IVal v)
    {
        return new Num(i.Values.Zip(v.Values, (a, b) => a + b).ToArray());
    }
    public static IVal operator -(IVal i, IVal v)
    {
        return new Num(i.Values.Zip(v.Values, (a, b) => a - b).ToArray());
    }
    public static IVal operator *(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new Num(a * c - b * c, a * d + b * c);
    }
    public static IVal operator /(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new Num((a * c + b * d) / (c * c + d * d), (b * c - a * d) / (c * c + d * d));
    }

    public static IVec Exp(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new Vec(Math.Pow(i.Get(), v.Get()));
        int yu, yi; yu = i.Trim().Dims; yi = v.Trim().Dims;
        throw Scribe.Issue($"TODO: Support complex exponentiation");
    }
    public static IVal Log(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new Num(Math.Log(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex logarithms");
    }

    // TODO: remove this method, as Number is now public
    public static IVal FromLiteral(double x)
    {
        return new Num(x);
    }
}

public class Num : IVal
{
    private List<double> vals;
    List<double> IDimensional<double>.Values => vals;
    public Num(params double[] ds)
    {
        vals = ds.ToList();
    }
    public Num(IVal iv)
    {
        vals = iv.Values.ToList();
    }

    void IVal.Set(params double[] vs)
    {
        vals = vs.ToList();
    }

    void IDimensional<double>.Normalize()
    {
        throw Scribe.Issue("Implement IVal normalize");
    }
}