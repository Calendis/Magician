namespace Magician.Symbols;

// SumDiff objects represent addition and subtraction operations with any number of arguments
public class SumDiff : Oper
{
    protected override int identity { get => 0; }

    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        commutative = true;
        associative = true;
        posUnaryIdentity = true;
    }
    public SumDiff(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("sumdiff", a, b)
    {
        commutative = true;
        associative = true;
        posUnaryIdentity = true;
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

    public override SumDiff New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }
    public static SumDiff StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }

    public override double Degree(Variable v)
    {
        double minD = 0;
        double maxD = double.MinValue;
        foreach (Oper o in AllArgs)
        {
            double d = o.Degree(v);
            minD = d < minD ? d : minD;
            maxD = d > maxD ? d : maxD;
        }
        return Math.Abs(maxD - minD);
    }

    public override void Reduce(Variable axis)
    {
        CombineLikeTerms(this, axis);
    }

    public override string ToString()
    {
        if (AllArgs.Count == 0)
            return "0";
        string sumdiff = "";
        foreach (Oper o in posArgs)
        {
            sumdiff += " + " + o.ToString();
        }
        sumdiff = sumdiff.TrimStart(' ');
        sumdiff = sumdiff.TrimStart('+');
        sumdiff = sumdiff.TrimStart(' ');
        
        foreach (Oper o in negArgs)
        {
            sumdiff += " - " + o.ToString();
        }
        return "(" + sumdiff + ")";
    }
}

// Fraction objects represent multiplication and division operations with any number of arguments
public class Fraction : Oper
{
    protected override int identity { get => 1; }
    public Fraction(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("fraction", a, b)
    {
        commutative = true;
        associative = true;
        posUnaryIdentity = true;
    }
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        commutative = true;
        associative = true;
        posUnaryIdentity = true;
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

    public override Fraction New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new Fraction(a, b);
    }
    public static Fraction StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new Fraction(a, b);
    }

    // TODO: when you introduce power laws, Degree is going to need to be an Oper
    public override double Degree(Variable v)
    {
        return posArgs.Select(o => o.Degree(v)).Sum() - negArgs.Select(o => o.Degree(v)).Sum();
    }

    public override void Reduce(Variable axis)
    {
        CombineFoil();
        //Scribe.Info($"I don't know fractions!");
    }

    //public static Fraction Mult(Oper a, Oper b)
    //{
    //    //
    //}

    public override Fraction Mult(Oper o)
    {
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.posArgs), negArgs.Concat(o.negArgs));
        return (Fraction)base.Mult(o);
    }

    public override Fraction Divide(Oper o)
    {
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.negArgs), negArgs.Concat(o.posArgs));
        return (Fraction)base.Divide(o);
    }

    public override string ToString()
    {
        string numerator = "";
        string denominator = "";

        foreach (Oper o in posArgs)
        {
            if (!(o is Variable v && !v.Found))
                numerator += "*" + o.ToString();
            else
                numerator += o.ToString();
        }
        foreach (Oper o in negArgs)
        {
            denominator += "*" + o.ToString();
        }
        string unbracketed = negArgs.Count == 0 ? numerator.TrimStart('*') : $"{numerator.TrimStart('*')}/{denominator.TrimStart('*')}";
        if (negArgs.Count > 0)
            return $"({unbracketed})";
        return unbracketed;
    }
}