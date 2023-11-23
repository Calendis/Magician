namespace Magician.Symbols;

// Fraction objects represent multiplication and division operations with any number of arguments
public class Fraction : Arithmetic
{
    protected override int? Identity { get => 1; }
    public Fraction(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("fraction", a, b)
    {
        // TODO: move these up to Arithmetic
        commutative = true;
        associative = true;
    }
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        commutative = true;
        associative = true;
    }

    // temporary override for debugging
    public override void SimplifyOuter(Variable? axis = null)
    {
        Reduce();
        Associate();
        //Scribe.Info($"\tSimplifying {this}...!\n\t\t===========================");
        //base.Simplify(axis);
        //Scribe.Info($"\tGot {this}");
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

    public override Oper Degree(Variable v)
    {
        if (IsDetermined)
            return new Variable(0);
        return new SumDiff(posArgs.Select(a => a.Degree(v)), negArgs.Select(a => a.Degree(v)));
    }

    protected override Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive)
    {
        Scribe.Info($"    A, B: {A}, {B}");
        Oper ABbar;
        if (!(aPositive ^ bPositive))
            ABbar = A.Divide(AB).Add(B.Divide(AB));
        else if (aPositive)
            ABbar = A.Divide(AB).Subtract(B.Divide(AB));
        else if (bPositive)
            ABbar = B.Divide(AB).Subtract(A.Divide(AB));
        else
            throw Scribe.Issue("haggu!");
        Scribe.Info($"\t  AB, ABbar: {AB}, {ABbar}");

        Oper combined = AB.Pow(ABbar);
        if (A is Variable av && av.Found && av.Val == 1)
            combined = B;
        else if (B is Variable bv && bv.Found && bv.Val == 1)
            combined = A;

        return combined;
    }

    //public override Oper Add(Oper o)
    //{
    //    if (posArgs.Count == 2)
    //    {
    //        if (posArgs[0].IsDetermined)
    //        {
    //            if (posArgs[1].Like(o))
    //            {
    //                posArgs[0] = new Variable(posArgs[0].Solution().Val + 1);
    //                return this;
    //            }
    //        }
    //        if (posArgs[1].IsDetermined)
    //        {
    //            if (posArgs[0].Like(o))
    //            {
    //                posArgs[1] = new Variable(posArgs[0].Solution().Val + 1);
    //                return this;
    //            }
    //        }
    //    }
    //    return base.Add(o);
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
        return $"({(negArgs.Count == 0 ? numerator.TrimStart('*') : $"{numerator.TrimStart('*')}/{denominator.TrimStart('*')}")})";
    }

    public override Fraction Factors()
    {
        return (Fraction)Copy();
    }
}