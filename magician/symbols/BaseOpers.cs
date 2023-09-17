namespace Magician.Symbols;

// Alternating sum to reprsent addition and subtraction
public class SumDiff : Oper
{
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        identity = 0;
        associative = true;
    }
    public SumDiff(List<Oper> a, List<Oper> b) : base("sumdiff", a, b)
    {
        identity = 0;
        associative = true;
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

    public override void Reduce(Variable axis)
    {
        /* Combine constant integer terms */
        for (int i = 0; i < posArgs.Count; i++)
        {
            Oper o = posArgs[i];
            if (o is Fraction f && f.posArgs.Count == 1 && f.negArgs.Count == 0)
            {
                posArgs.Remove(o);
                posArgs.AddRange(f.posArgs);
                Reduce(axis);
            }
        }
        IEnumerable<Oper> plus = posArgs.Where(o => o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != identity);
        IEnumerable<Oper> minus = negArgs.Where(o => o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != identity);
        List<Oper> posChaff = posArgs.Where(o => !(o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != identity)).ToList();
        List<Oper> negChaff = negArgs.Where(o => !(o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != identity)).ToList();
        int newPlus = plus.Aggregate(0, (total, o) => total + (int)((Variable)o).Val);
        int newMinus = minus.Aggregate(0, (total, o) => total + (int)((Variable)o).Val);
        int reducedConstant = newPlus - newMinus;
        if (reducedConstant < 0)
            negChaff.Add(new Variable(-reducedConstant));
        else if (reducedConstant > 0)
            posChaff.Add(new Variable(reducedConstant));
        posArgs.Clear(); posArgs.AddRange(posChaff);
        negArgs.Clear(); negArgs.AddRange(negChaff);

        /* Combine like terms */
        Scribe.Warn("Combining like terms...");
        List<Fraction> matchingTerms = new();
        List<Fraction> chaffTerms = new();
        foreach (Oper o in AllArgs)
        {
            if (o.Contains(axis))
                matchingTerms.Add(o.ByCoefficient());
            else
                chaffTerms.Add(o.ByCoefficient());
        }

        // AB AC AD BC BD CD
        Dictionary<(int, int), Oper> matchingIntersections = new();
        Dictionary<(int, int), Oper> matchingSummedSetDifferences = new();
        for (int i = 0; i < matchingTerms.Count - 1; i++)
        {
            for (int j = 0; j < matchingTerms.Count - i; j++)
            {
                // j + i + 1
                Fraction termA = matchingTerms[i];
                Fraction termB = matchingTerms[i + j + 1];
                Fraction intersectFrac = new(
                    termA.posArgs.Intersect(termB.posArgs, new OperCompare()).ToList(),
                    termA.negArgs.Intersect(termB.negArgs, new OperCompare()).ToList()
                );
                matchingIntersections.Add((i, j + i + 1), intersectFrac);
            }
        }
        //
        Dictionary<(int, int), Oper> immatchingIntersections = new();
        for (int i = 0; i < chaffTerms.Count - 1; i++)
        {
            for (int j = 0; j < chaffTerms.Count - 1 - i; j++)
            {
                // j + i + 1
                Fraction termA = chaffTerms[i];
                Fraction termB = chaffTerms[i + j + 1];
                Fraction intersectFrac = new();
                matchingIntersections.Add((i, j + i + 1), intersectFrac);
            }
        }
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
        associative = true;
    }
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        identity = 1;
        associative = true;
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