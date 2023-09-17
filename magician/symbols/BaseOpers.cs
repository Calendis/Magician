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
        foreach (Oper o in AllArgs)
        {
            double d = o.Degree(v);
            minD = d < minD ? d : minD;
            maxD = d > maxD ? d : maxD;
        }
        return maxD - minD;
    }

    public override string ToString()
    {
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
        return "(" + sumdiff + ")";
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

    // TODO: when you introduce power laws, Degree is going to need to be an Oper
    public override double Degree(Variable v)
    {
        return posArgs.Select(o => o.Degree(v)).Sum() - negArgs.Select(o => o.Degree(v)).Sum();
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