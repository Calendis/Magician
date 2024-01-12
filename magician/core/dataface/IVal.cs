namespace Magician.Core;

public interface IVal : IDimensional<double>
{
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
    public bool EqValue(double d) => EqValue(new Val(d));
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
        Set(this + new Val(vs));
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
            return new Val(0);
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
        return new Val(Values.SkipLast(toTrim).ToArray());
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
        return new Val(newAll);
    }
    public static IVal operator -(IVal i, double x)
    {
        double[] newAll = i.Values.ToArray();
        newAll[0] -= x;
        return new Val(newAll);
    }
    // Multiplication supported for one or two dimensions
    public static IVal operator *(IVal i, double x)
    {
        return new Val(i.Values.Select(k => k * x).ToArray());
    }
    public static IVal operator /(IVal i, double x)
    {
        return new Val(i.Values.Select(k => k / x).ToArray());
    }

    public static IVal operator +(IVal i, IVal v)
    {
        // Pad if dimensions do not match
        if (i.Dims < v.Dims)
        {
            int diff = v.Dims - i.Dims;
            double[] padding = new double[diff];
            i.Set(i.Values.Concat(padding).ToArray());
        }
        else if (v.Dims < i.Dims)
        {
            int diff = i.Dims - v.Dims;
            double[] padding = new double[diff];
            v.Set(i.Values.Concat(padding).ToArray());
        }
        return new Val(i.Values.Zip(v.Values, (a, b) => a + b).ToArray());
    }
    public static IVal operator -(IVal i, IVal v)
    {
        // Pad if dimensions do not match
        if (i.Dims < v.Dims)
        {
            int diff = v.Dims - i.Dims;
            double[] padding = new double[diff];
            i.Set(i.Values.Concat(padding).ToArray());
        }
        else if (v.Dims < i.Dims)
        {
            int diff = i.Dims - v.Dims;
            double[] padding = new double[diff];
            v.Set(i.Values.Concat(padding).ToArray());
        }
        return new Val(i.Values.Zip(v.Values, (a, b) => a - b).ToArray());
    }
    public static IVal operator *(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        double re = a * c - b * d;
        double im = a * d + b * c;
        if (im == 0)
            return new Val(re);
        return new Val(re, im);
    }
    public static IVal operator /(IVal i, IVal v)
    {
        double a = i.Get();
        double b = i.Dims > 1 ? i.Get(1) : 0;
        double c = v.Get();
        double d = v.Dims > 1 ? v.Get(1) : 0;
        double re = (a * c + b * d) / (c * c + d * d);
        double im = (b * c - a * d) / (c * c + d * d);
        if (im == 0)
            return new Val(re);
        return new Val(re, im);
    }

    public static IVar Exp(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
        {
            if (i.Get() < 0 && v.Get() != (int)v.Get())
            {
                double re = Math.Exp(v.Get()*Math.Log(Math.Abs(i.Get())))*Symbols.Numeric.Funcs.Cos(Math.PI*v.Get());
                double im = Math.Exp(v.Get()*Math.Log(Math.Abs(i.Get())))*Symbols.Numeric.Funcs.Sin(Math.PI*v.Get());
                if (re != 0 && im != 0)
                {
                    Scribe.Info($"real is {re}");
                    Scribe.Info($"parts are {Math.Exp(v.Get()*Math.Log(Math.Abs(i.Get())))}, {Symbols.Numeric.Funcs.Cos(Math.PI*v.Get())}");
                }
                return new Var(re, im);
            }
            else
            {
                return new Var(Math.Pow(i.Get(), v.Get()));
            }
        }
            
        int yu, yi; yu = i.Trim().Dims; yi = v.Trim().Dims;
        throw Scribe.Issue($"TODO: Support complex exponentiation");
    }
    public static IVal Log(IVal i, IVal v)
    {
        if (i.Trim().Dims * v.Trim().Dims == 1)
            return new Val(Math.Log(i.Get(), v.Get()));
        throw Scribe.Issue($"TODO: Support complex logarithms");
    }

    // TODO: remove this method, as Number is now public
    public static IVal FromLiteral(double x)
    {
        return new Val(x);
    }
}

public class Val : IVal
{
    private readonly List<double> vals;
    List<double> IDimensional<double>.Values => vals;
    public Val(params double[] ds)
    {
        if (ds.Length == 0)
            throw Scribe.Error("Cannot create empty num");
        vals = ds.ToList();
    }
    public Val(IVal iv)
    {
        vals = iv.Values.ToList();
    }

    void IDimensional<double>.Normalize()
    {
        throw Scribe.Issue("Implement IVal normalize");
    }

    public override string ToString()
    {
        return Scribe.Expand<List<double>, double>(vals);
    }
}

public class Rational : IVal
{
    int num;
    int denom;
    List<double> IDimensional<double>.Values => Crunch.Values;
    public IVal Crunch => new Val((double)num / denom);

    // TODO: arithmetic operatrors for Rational
    public void Normalize()
    {
        throw new NotImplementedException();
    }
}