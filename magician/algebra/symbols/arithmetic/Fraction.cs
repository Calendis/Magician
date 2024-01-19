namespace Magician.Algebra.Symbols;
using Core;

// Fraction objects represent multiplication and division operations with any number of arguments
public class Fraction : Arithmetic
{
    protected override int Identity => 1;
    public Fraction(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("fraction", a, b) { }
    public Fraction(params Oper[] ops) : base("fraction", ops) { }

    public List<Oper> Numerator => posArgs;
    public List<Oper> Denominator => negArgs;
    IVal num = new Val(1);
    IVal denom = new Val(1);
    private readonly Rational rationalSolution = new(0, 0);

    public override Variable Sol()
    {
        //IVar quo = new Var(1);
        //IVar num = new Var(1);
        //IVar denom = new Var(1);
        num.Set(1);
        denom.Set(1);
        foreach (Oper o in posArgs)
            if (o is Variable v)
                num = IVal.Multiply(v, num, num);
            else
                num = IVal.Multiply(o.Sol(), num, num);
        foreach (Oper o in negArgs)
            if (o is Variable v)
                denom = IVal.Multiply(v, denom, denom);
            else
                denom = IVal.Multiply(o.Sol(), denom, denom);

        if (num.Get() == (int)num.Get() && denom.Get() == (int)denom.Get() && denom.Get() != 0 && !(denom.Get() == 1 && num.Get() == 1))
        {
            rationalSolution.Set((int)num.Get(), (int)denom.Get());
            return rationalSolution;
        }
        solution.Value.Set(IVal.Divide(num, denom));
        return solution;
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
        SumDiff sd = new(posArgs.Select(a => a.Degree(v)), negArgs.Select(a => a.Degree(v)));
        if (sd.IsDetermined)
            return sd.Sol();
        sd.ReduceOuter();
        return LegacyForm.Shed(sd);
    }
    public override FactorMap Factors()
    {
        return new(posArgs.Concat(negArgs.Select(na => na.Pow(new Variable(-1)))).ToArray());
    }

    protected override Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive)
    {
        Oper ABbar;
        if (!(aPositive ^ bPositive))
            ABbar = A.Degree(AB).Plus(B.Degree(AB));
        else if (aPositive)
            ABbar = A.Degree(AB).Minus(B.Degree(AB));
        else if (bPositive)
            ABbar = B.Degree(AB).Minus(A.Degree(AB));
        else
            throw Scribe.Issue("haggu!");
        ABbar.Reduce(2);

        Oper combined;
        if (A is Variable av && av.Found && av.Value.EqValue(1))
            combined = B;
        else if (B is Variable bv && bv.Found && bv.Value.EqValue(1))
            combined = A;
        else if (
            (AB.IsUnary && AB.posArgs[0] is Variable abv && abv.Found && abv.Value.Trim().Dims == 1 && abv.Value.EqValue(1)) ||
            (AB.IsDetermined && AB.Sol().Value.Trim().Dims == 1 && AB.Sol().Value.EqValue(1))
        )
        {
            if (aPositive && bPositive)
                combined = A.Mult(B);
            else if (aPositive)
                combined = A.Divide(B);
            else if (bPositive)
                combined = B.Divide(A);
            else
                combined = new Variable(1).Divide(A.Mult(B));
        }

        else
            combined = AB.Pow(ABbar);

        combined.ReduceOuter();
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