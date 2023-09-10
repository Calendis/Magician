namespace Magician.Symbols;

// Alternating sum to reprsent addition and subtraction
public class SumDiff : Oper
{
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        identity = 0;
    }
    public SumDiff(List<Oper> a, List<Oper> b) : base("sumdiff", a, b)
    {
        identity = 0;
    }
    public override Variable Solution()
    {
        double total = 0;
        foreach (Oper o in posArgs)
            if (o is Variable v)
                total += v.Val;
            else
                total += o.Solution().Val;
        foreach (Oper o in negArgs)
            if (o is Variable v)
                total -= v.Val;
            else
                total -= o.Solution().Val;
        return new Variable(total);
    }

    public override SumDiff New(List<Oper> a, List<Oper> b)
    {
        return new SumDiff(a, b);
    }

    public override double Degree(Variable v)
    {
        double minD = double.MaxValue;
        double maxD = double.MinValue;
        foreach (Oper o in _oldArgs)
        {
            double d = o.Degree(v);
            minD = d < minD ? d : minD;
            maxD = d > maxD ? d : maxD;
        }
        return Math.Abs(minD) + maxD;
    }

    public override void Simplify()
    {
        //ArgsOLD = Associate<SumDiff>();

        // Combine constants
        List<(Oper, bool)> flaggedArgs = new();
        double total = 0;
        int switcher = 0;
        foreach (Oper o in _oldArgs)
        {
            if (o is Variable v && v.Found)
            {
                total += v.Val * switcher == 0 ? 1 : -1;  // TODO: use this total
            }
            else
            {
                flaggedArgs.Add((o, switcher == 1));
            }

            switcher = 1 - switcher;
        }
        //args = AssembleArgs(flaggedArgs, identity);
        //numArgs = args.Length;

        // TODO: Combine like variables
        //

        // Modify
        //ArgsOLD = AssembleArgs(flaggedArgs, identity);
    }

    public override string ToString()
    {
        //string argsSigns = $"{string.Join("", _oldArgs.Select((a, i) => i % 2 == 0 ? $"+{a.ToString()}" : $"-{a.ToString()}"))}";
        //return "(" + string.Join("", argsSigns.Skip(1)) + ")";
        string sumdiff = "";
        if (posArgs.Count > 0)
        {
            sumdiff = posArgs[0].ToString();
        }
        foreach (Oper o in posArgs.Skip(1))
        {
            sumdiff += "+" + o.ToString();
        }
        foreach (Oper o in negArgs)
        {
            sumdiff += "-" + o.ToString();
        }
        return sumdiff + ")";
    }
}

// Alternating reciprocal
public class Fraction : Oper
{
    public Fraction(List<Oper> a, List<Oper> b) : base("fraction", a, b)
    {
        identity = 1;
    }
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        identity = 1;
    }

    public override Variable Solution()
    {
        double quo = 1;
        foreach (Oper o in posArgs)
            if (o is Variable v)
                quo *= v.Val;
            else
                quo *= o.Solution().Val;
        foreach (Oper o in negArgs)
            if (o is Variable v)
                quo /= v.Val;
            else
                quo /= o.Solution().Val;
        return new Variable(quo);
    }

    public override Oper New(List<Oper> a, List<Oper> b)
    {
        return new Fraction(a, b);
    }

    public override double Degree(Variable v)
    {
        return _oldArgs.Select<Oper, double>((o, i) =>
        {
            if (i % 2 == 0)
                return o.Degree(v);
            else
                return -o.Degree(v);
        }).Sum();
    }

    public override string ToString()
    {
        string numerator = "";
        string denominator = "";

        foreach (Oper o in posArgs)
        {
            numerator += "*" + o.ToString();
        }
        foreach (Oper o in negArgs)
        {
            denominator += "*" + o.ToString();
        }
        return $"({numerator.TrimStart('*')} / {denominator.TrimStart('*')})";
    }
}