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
        set
        {
            Normalize();
            for (int i = 0; i < Values.Count; i++)
            {
                Values[i] *= value;
            }
        }
    }
    void IDimensional<double>.Normalize()
    {
        double m = Magnitude;
        for (int i = 0; i < Values.Count; i++)
        {
            Values[i] = Values[i] / m;
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

    public static IVar Exp(IVal z, IVal w)
    {
        // if i and v are both real numbers...
        if (z.Trim().Dims * w.Trim().Dims == 1)
        {
            // if i is negative and v is not an integer, the answer is complex
            if (z.Get() < 0 && w.Get() != (int)w.Get())
            {
                double re = Math.Exp(w.Get() * Math.Log(Math.Abs(z.Get()))) * Symbols.Numeric.Funcs.Cos(Math.PI * w.Get());
                double im = Math.Exp(w.Get() * Math.Log(Math.Abs(z.Get()))) * Symbols.Numeric.Funcs.Sin(Math.PI * w.Get());
                return new Var(re, im);
            }
            else
            {
                // if i is positive or if v is an integer, the answer is real
                // when v is not an integer, there are multiple solutions
                // these are found by exponentiating a non-integer Symbols.Rational
                return new Var(Math.Pow(z.Get(), w.Get()));
            }
        }
        // real base, complex exponent
        else if (z.Trim().Dims == 1)
        {
            double a = w.Get();
            double b = w.Get(1);
            double arg = z.Get() >= 0 ? 0 : Math.PI;
            double coefficient = Math.Exp(a*Math.Log(Math.Abs(z.Get()))-b*arg);
            double re = Symbols.Numeric.Funcs.Cos(b*Math.Log(z.Magnitude) + a*arg) * coefficient;
            double im = Symbols.Numeric.Funcs.Sin(b*Math.Log(z.Magnitude) + a*arg) * coefficient;
            return new Var(re, im);
        }
        // complex base, any exponent
        else
        {
            if (z.Trim().Dims == 1 && z.Get() == 1)
                return new Var(1);
            return Exp(new Val(Math.E), w * Ln(z));
        }
    }

    public static IVal ExpI(double x) => new Val(Symbols.Numeric.Funcs.Cos(x), Symbols.Numeric.Funcs.Sin(x));
    public static IVal Log(IVal z, IVal logBase)
    {
        if (z.EqValue(logBase))
            return new Val(1);
        if (z.Trim().Dims * logBase.Trim().Dims == 1)
        {
            if (logBase.Get() < 0)
                return Ln(z) / Ln(logBase);
            if (z.Get() < 0)
                return Log(new Val(Math.Abs(z.Get())), logBase) + new Val(0, Math.PI);
            return new Val(Math.Log(z.Get(), logBase.Get()));
        }
        double a = z.Get();
        double b = z.Trim().Dims == 1 ? 0 : z.Get(1);
        return new Val(Math.Log(z.Magnitude), Math.Atan2(b, a));
    }
    public static IVal Ln(IVal v) => Log(v, new Val(Math.E));

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

    // this pattern where I implement the base interface twice is annoying, but it does work
    public double Magnitude
    {
        get
        {
            return ((IDimensional<double>)this).Magnitude;
        }
        set
        {
            ((IDimensional<double>)this).Magnitude = value;
        }
    }
    public void Normalize()
    {
        ((IDimensional<double>)this).Normalize();
    }

    double IDimensional<double>.Magnitude
    {
        get
        {
            double total = 0;
            foreach (double x in vals)
            {
                total += x * x;
            }
            return Math.Sqrt(total);
        }
        set
        {
            Normalize();
            for (int i = 0; i < vals.Count; i++)
            {
                vals[i] *= value;
            }
        }
    }
    void IDimensional<double>.Normalize()
    {
        double m = Magnitude;
        for (int i = 0; i < vals.Count; i++)
        {
            vals[i] = vals[i] / m;
        }
    }

    public override string ToString()
    {
        return Scribe.Expand<List<double>, double>(vals);
    }
}