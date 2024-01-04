namespace Magician.Symbols;

// Fraction objects represent multiplication and division operations with any number of arguments
public class Fraction : Arithmetic
{
    protected override int? Identity { get => 1; }
    public Fraction(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("fraction", a, b) { }
    public Fraction(params Oper[] ops) : base("fraction", ops) { }

    public List<Oper> Numerator => posArgs;
    public List<Oper> Denominator => negArgs;

    public override Variable Sol()
    {
        IVal quo = new ValWrapper(1);
        foreach (Oper o in posArgs)
            if (o is Variable v)
                quo *= v.Sol();
            else
                quo *= o.Sol();
        foreach (Oper o in negArgs)
            if (o is Variable v)
                quo /= v.Sol();
            else
                quo /= o.Sol();
        return new Variable(quo);
    }

    public override Fraction New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new Fraction(a, b);
    }

    public override Oper Degree(Oper v)
    {
        if (IsDetermined)
            return new Variable(0);
        if (Like(v))
            return new Variable(1);
        return new SumDiff(posArgs.Select(a => a.Degree(v)), negArgs.Select(a => a.Degree(v)));
    }
    public override Fraction Factors()
    {
        (Dictionary<string, Oper> ordToOper, Dictionary<string, int> ordToCount) = ArgBalance();
        Fraction factors = new();
        foreach (string ord in ordToOper.Keys)
        {
            Oper fac = ordToOper[ord];
            Variable deg = new(ordToCount[ord]);
            //Scribe.Info($"\t\tfac, deg: {fac}, {deg}");
            if (((IVal)deg) > new Variable(0))
                factors.Numerator.Add(fac.Pow(deg));
            else if (((IVal)deg) < new Variable(0))
                factors.Denominator.Add(fac.Pow(deg));
        }
        return factors;
    }

    protected override Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive)
    {
        if (AB.IsDetermined && AB.Sol().Value.Get() == 1)
        {
            if (aPositive)
                if (bPositive)
                    return New(new List<Oper> { A, B }, new List<Oper> { });
                else
                    return New(new List<Oper> { A }, new List<Oper> { B });
            else if (bPositive)
                return New(new List<Oper> { B }, new List<Oper> { A });
            else
                return New(new List<Oper> { }, new List<Oper> { A, B });
        }
        Oper ABbar;
        if (!(aPositive ^ bPositive))
            ABbar = A.Degree(AB).Add(B.Degree(AB));
        else if (aPositive)
            ABbar = A.Degree(AB).Subtract(B.Degree(AB));
        else if (bPositive)
            ABbar = B.Degree(AB).Subtract(A.Degree(AB));
        else
            throw Scribe.Issue("haggu!");
        
        ABbar.Reduce(2);
        Oper combined;
        if (A is Variable av && av.Found && av.Value.Get() == 1)
        {
            //Scribe.Info($"\t\t\tCase B: {B}");
            combined = B;
        }
        else if (B is Variable bv && bv.Found && bv.Value.Get() == 1)
        {
            //Scribe.Info($"\t\t\tCase A: {A}");
            combined = A;
        }
        else if (AB.IsUnary && AB.posArgs[0] is Variable abv && abv.Found && abv.Value.Get() == 1)
        {
            //Scribe.Info($"\t\t\tCase ABbar: {ABbar}");
            combined = ABbar;
        }
        else
        {
            //Scribe.Info($"\t\t\tCase Pow: {AB}.Pow{ABbar}");
            //combined = LegacyForm.Shed(AB).Pow(LegacyForm.Shed(ABbar));
            combined = AB.Pow(ABbar);
        }

        return combined;
    }

    public override Oper Mult(Oper o)
    {
        if (IsUnary)
            return posArgs[0].Mult(o);
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.posArgs), negArgs.Concat(o.negArgs));
        return (Fraction)base.Mult(o);
    }

    public override Oper Divide(Oper o)
    {
        if (IsUnary)
            return posArgs[0].Divide(o);
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
        return $"({(negArgs.Count == 0 ? numerator.TrimStart('*') : $"{numerator.TrimStart('*')}/{denominator.TrimStart('*')}")})";
    }
}