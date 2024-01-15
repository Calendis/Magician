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

    public static IVal Exp(IVal z, IVal w)
    {
        // if z and w are both real numbers...
        if (z.Trim().Dims * w.Trim().Dims == 1)
        {
            // if z is negative and w is not an integer, the answer is complex
            if (z.Get() < 0 && w.Get() != (int)w.Get())
            {
                double re = Math.Exp(w.Get() * Math.Log(Math.Abs(z.Get()))) * Symbols.Numeric.Funcs.Cos(Math.PI * w.Get());
                double im = Math.Exp(w.Get() * Math.Log(Math.Abs(z.Get()))) * Symbols.Numeric.Funcs.Sin(Math.PI * w.Get());
                return new Var(re, im);
            }
            else
            {
                // otherwise, the answer is real
                return new Val(Math.Pow(z.Get(), w.Get()));
            }
        }
        // real base, complex exponent
        else if (z.Trim().Dims == 1)
        {
            double a = w.Get();
            double b = w.Get(1);
            double x = z.Get();
            double coef = Math.Pow(x, a);

            return new Val(coef * Symbols.Numeric.Funcs.Cos(b * Math.Log(x)), coef * Symbols.Numeric.Funcs.Sin(b * Math.Log(x)));
        }
        // complex base, positive integer exponent
        else if (z.Trim().Dims != 1 && w.Trim().Dims == 1 && (int)w.Get() == w.Get() && w.Get() >= 1)
        {
            return new Val(z * Exp(z, w - new Val(1)));
        }
        // TODO: complex base, Gaussian integer exponent
        // complex base, other exponent
        else
        {
            if (z.Trim().Dims == 1 && z.Get() == 1)
                return new Val(1);
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
        if (vals.Count == 1)
            return $"{vals[0]}";
        if (vals.Count == 2)
        {
            if (vals[0] == 0)
                if (vals[1] == 1)
                    return "i";
                else if (vals[1] == -1)
                    return "-i";
                else
                    return $"{vals[1]}i";
            if (vals[1] == 1)
                return $"{vals[0]} + i";
            else
            {
                if (vals[1] < 0)
                    return $"{vals[0]} - {Math.Abs(vals[1])}i";
                return $"{vals[0]} + {vals[1]}i";
            }
        }
        return Scribe.Expand<List<double>, double>(vals);
    }
}