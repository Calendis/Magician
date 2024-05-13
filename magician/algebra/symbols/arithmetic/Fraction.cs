namespace Magician.Alg.Symbols;
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

    public override void ReduceOuter()
    {
        // Detect 0s in the numerator
        foreach (Oper a in posArgs)
        {
            if (a.IsDetermined && a.Sol().Value.EqValue(0))
            {
                posArgs.Clear();
                negArgs.Clear();
                posArgs.Add(new Variable(0));
                AssociatedVars.Clear();
                break;
            }
        }
        if (IsDetermined)
        {
            IVal sv = Sol().Value;
            bool isInt = true;
            foreach (double svd in sv.Values)
                if (svd != (int)svd)
                    isInt = false;
            if (isInt)
            {
                posArgs.Clear();
                negArgs.Clear();
                posArgs.Add(new Variable(sv));
                return;
            }
        }
        base.ReduceOuter();
    }
    internal override void CombineOuter(Variable? axis = null)
    {
        base.CombineOuter(axis);
        //BalanceDegree(axis);
    }

    //void BalanceDegree(Variable? axis=null)
    //{
    //    List<Oper> toSwaptoNeg = new();
    //    List<int> toRemFromPos = new();
    //    List<Oper> toSwaptoPos = new();
    //    List<int> toRemFromNeg = new();
    //    for (int i = 0; i < posArgs.Count; i++)
    //    {
    //        Oper deg = posArgs[i].Degree();
    //        if (deg.IsDetermined && deg < new Variable(0))
    //        {
    //            toSwaptoNeg.Add(posArgs[i]);
    //        }
    //    }
    //    for (int i = 0; i < negArgs.Count; i++)
    //    {
    //        Oper deg = negArgs[i].Degree();
    //        if (deg.IsDetermined && deg < new Variable(0))
    //        {
    //            toSwaptoPos.Add(negArgs[i]);
    //        }
    //    }
    //    foreach (Oper o in toSwaptoNeg)
    //    {
    //        posArgs.Remove(o);
    //        negArgs.Add()
    //    }
    //}

    //public void BalanceDegree(Variable? axis = null)
    //{
    //    Scribe.Info($"BalanceDegree: axis is {axis}");
    //    // If axis is null, balance degree for all variables
    //    if (axis == null)
    //    {
    //        AssociatedVars.ForEach(v =>
    //        {
    //            if (posArgs.Count > 1)
    //                BalDeg(posArgs, v);
    //            if (negArgs.Count > 1)
    //                BalDeg(negArgs, v);
    //        });
    //    }
    //    else
    //    {
    //        if (posArgs.Count > 1)
    //            BalDeg(posArgs, axis);
    //        if (negArgs.Count > 1)
    //            BalDeg(negArgs, axis);
    //    }
    //}
    //static void BalDeg(List<Oper> args, Variable axis)
    //{
    //    for (int i = 0; i < args.Count; i++)
    //    {
    //        Oper arg = args[i];
    //        if (!arg.IsDetermined && arg.Degree(axis) < new Variable(1))
    //        {
    //            int idx = FindIdx(args, i, axis);
    //            if (idx != -1)
    //            {
    //                if (args[i] is SumDiff sd)
    //                {
    //                    args[i] = Mult(args[idx], sd);
    //                    args[i].Combine(null, 2);
    //                    args[i].Reduce(2);
    //                }
    //                else
    //                {
    //                    args[i] = arg.Mult(args[idx]);
    //                    args[i].ReduceOuter();
    //                }
    //                //args[i].ReduceOuter();
    //                args.RemoveAt(idx);
    //                if (idx < i)
    //                    i--;
    //            }
    //        }
    //    }
    //    static int FindIdx(List<Oper> args, int idx, Variable axis)
    //    {
    //        Oper arg = args[idx];
    //        for (int i = 0; i < args.Count; i++)
    //        {
    //            if (i != idx)
    //            {
    //                Oper other = args[i];
    //                if (arg.Degree(axis).Plus(other.Degree(axis)).IsDetermined && arg.Degree(axis).Plus(other.Degree(axis)) > new Variable(0))
    //                {
    //                    return i;
    //                }
    //            }
    //        }
    //        return -1;
    //    }
    //}

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
        return sd.Trim();
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
            {
                //combined = A.Mult(B);
                combined = new Variable(1).Divide(A.Mult(B));
            }
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
            return new Fraction(posArgs.Concat(o.posArgs).ToList(), negArgs.Concat(o.negArgs).ToList());
        return base.Mult(o);
    }

    public override Oper Divide(Oper o)
    {
        if (IsUnary)
            return posArgs[0].Divide(o);
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.negArgs).ToList(), negArgs.Concat(o.posArgs).ToList());
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