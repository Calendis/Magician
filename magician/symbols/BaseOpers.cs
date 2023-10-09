namespace Magician.Symbols;

// Alternating sum to reprsent addition and subtraction
public class SumDiff : Oper, ISum
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
        CombineConstantTerms(this);
        CombineLikeTerms(this, axis);
    }

    public override SumDiff Add(params Oper[] os)
    {
        return New(posArgs.Concat(os), negArgs);
    }
    public override SumDiff Subtract(params Oper[] os)
    {
        return New(posArgs, negArgs.Concat(os));
    }

    public override string ToString()
    {
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

// Alternating reciprocal
public class Fraction : Oper, IFrac
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

    public override Fraction Mult(params Oper[] osa)
    {
        if (osa.Length == 1 && osa[0] is Fraction)
            if (osa[0].negArgs.Count == 0)
                return Mult(osa[0].posArgs.ToArray());
            else if (osa[0].posArgs.Count == 0)
                return Divide(osa[0].posArgs.ToArray());

        OperLike oc = new();
        List<Oper> os = osa.ToList();
        List<Oper> final = new(negArgs);
        for (int i = 0; i < os.Count; i++)
        {
            Oper o = os[i];
            if (posArgs.Contains(o, oc))
            {
                if (o is Variable)
                    final.Remove(o);
                else
                    final.Remove(o.posArgs[0]);
                os.Remove(o);
            }
        }
        return New(posArgs.Concat(os), final);
    }
    public override Fraction Divide(params Oper[] osa)
    {
        if (osa.Length == 1 && osa[0] is Fraction)
            if (osa[0].negArgs.Count == 0)
                return Divide(osa[0].posArgs.ToArray());
            else if (osa[0].posArgs.Count == 0)
                return Mult(osa[0].posArgs.ToArray());

        OperLike oc = new();
        List<Oper> os = osa.ToList();
        List<Oper> pos = new(posArgs);
        for (int i = 0; i < os.Count; i++)
        {
            Oper o = os[i];
            if (posArgs.Contains(o, oc))
            {
                if (o is Variable)
                    pos.Remove(o);
                else
                    pos.Remove(o.posArgs[0]);
                os[i].cancelled = true;
            }
        }
        return New(pos, negArgs.Concat(os.Where(o => !o.cancelled)));
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
        return negArgs.Count == 0 ? numerator.TrimStart('*') : $"{numerator.TrimStart('*')}/{denominator.TrimStart('*')}";
    }
}