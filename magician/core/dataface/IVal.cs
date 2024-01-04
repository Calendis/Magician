using Magician.Symbols;

namespace Magician;

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
            if (Magnitude() == other.Magnitude() && Trim().Dims == other.Trim().Dims)
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

    // TODO: this SHOULD work, but keep an eye on it
    public IVal Delta(IVal iv)
    {
        Set(this + iv);
        return this;
    }
    public IVal Delta(params double[] vs)
    {
        Set(vs);
        return this;
    }

    public IVal Trim()
    {
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
        return new ValWrapper(All.SkipLast(toTrim).ToArray());
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
        return new ValWrapper(i.All.Zip(v.All, (a, b) => a + b).ToArray());
    }
    public static IVal operator -(IVal i, IVal v)
    {
        return new ValWrapper(i.All.Zip(v.All, (a, b) => a - b).ToArray());
    }
    public static IVal operator +(IVal i, double x)
    {
        double[] newAll = i.All.ToArray();
        newAll[0] += x;
        return new ValWrapper(newAll);
    }
    public static IVal operator -(IVal i, double x)
    {
        double[] newAll = i.All.ToArray();
        newAll[0] -= x;
        return new ValWrapper(newAll);
    }
    // Multiplication supported for one or two dimensions
    public static IVal operator *(IVal i, double x)
    {
        return new ValWrapper(i.All.Select(k => k*x).ToArray());
    }
    public static IVal operator /(IVal i, double x)
    {
        return new ValWrapper(i.All.Select(k => k/x).ToArray());
    }
    public static IVal operator *(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new ValWrapper(a*c - b*c, a*d + b*c);
    }
    public static IVal operator /(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        return new ValWrapper((a*c + b*d)/(c*c + d*d), (b*c - a*d)/(c*c + d*d));
    }

    public static IVal Exp(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new ValWrapper(Math.Pow(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex exponentiation");
    }
    public static IVal Log(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new ValWrapper(Math.Log(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex logarithms");
    }

    public static IVal FromLiteral(double x)
    {
        return new ValWrapper(x);
    }
}

internal class ValWrapper : IVal
{
    private double[] vals;
    public ValWrapper(params double[] ds)
    {
        vals = ds.ToArray();
    }

    double[] IVal.All => vals;

    void IVal.Set(params double[] vs)
    {
        vals = vs.ToArray();
    }
}