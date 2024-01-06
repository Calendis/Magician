using Magician.Symbols;

namespace Magician.Core;

public interface IVal : IDimensional
{
    public double[] All { get; }
    int IDimensional.Dims => All.Length;
    public double Get(int i = 0) => All[i];
    public void Set(params double[] vs);
    public void Set(IVal iv)
    {
        for (int i = 0; i < Dims; i++)
        {
            All[i] = iv.All[i];
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
            if (All[i] != other.All[i])
                return false;
        return true;
    }
    public double Magnitude()
    {
        double total = 0;
        foreach (double x in All)
        {
            total += x * x;
        }
        return Math.Sqrt(total);
    }

    public IVal Delta(params double[] vs)
    {
        Set(this + new Number(vs));
        return this;
    }

    public IVal Trim()
    {
        bool nz = false;
        foreach (double d in All)
            if (d != 0)
            {
                nz = true;
                break;
            }
        if (!nz)
            return new Number(0);
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
        return new Number(All.SkipLast(toTrim).ToArray());
    }

    public static bool operator <(IVal iv0, IVal iv1)
    {
        if (iv0.Dims * iv1.Dims == 1)
            return iv0.Get() < iv1.Get();
        return iv0.Magnitude() < iv1.Magnitude();
    }
    public static bool operator >(IVal iv0, IVal iv1)
    {
        return !(iv0 < iv1);
    }

    public static IVal operator +(IVal i, IVal v)
    {
        return new Number(i.All.Zip(v.All, (a, b) => a + b).ToArray());
    }
    public static IVal operator -(IVal i, IVal v)
    {
        return new Number(i.All.Zip(v.All, (a, b) => a - b).ToArray());
    }
    public static IVal operator +(IVal i, double x)
    {
        double[] newAll = i.All.ToArray();
        newAll[0] += x;
        return new Number(newAll);
    }
    public static IVal operator -(IVal i, double x)
    {
        double[] newAll = i.All.ToArray();
        newAll[0] -= x;
        return new Number(newAll);
    }
    // Multiplication supported for one or two dimensions
    public static IVal operator *(IVal i, double x)
    {
        return new Number(i.All.Select(k => k*x).ToArray());
    }
    public static IVal operator /(IVal i, double x)
    {
        return new Number(i.All.Select(k => k/x).ToArray());
    }
    public static IVal operator *(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new Number(a*c - b*c, a*d + b*c);
    }
    public static IVal operator /(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new Number((a*c + b*d)/(c*c + d*d), (b*c - a*d)/(c*c + d*d));
    }

    public static IVal Exp(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new Number(Math.Pow(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex exponentiation");
    }
    public static IVal Log(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new Number(Math.Log(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex logarithms");
    }

    // TODO: remove this method, as Number is now public
    public static IVal FromLiteral(double x)
    {
        return new Number(x);
    }
}

public class Number : IVal
{
    private double[] vals;
    public Number(params double[] ds)
    {
        vals = ds.ToArray();
    }

    double[] IVal.All => vals;

    void IVal.Set(params double[] vs)
    {
        vals = vs.ToArray();
    }
}