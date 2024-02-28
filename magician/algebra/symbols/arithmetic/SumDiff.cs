namespace Magician.Alg.Symbols;
using Core;

// SumDiff objects represent addition and subtraction operations with any number of arguments
public class SumDiff : Arithmetic, IDifferentiable
{
    protected override int Identity => 0;

    // TODO: expand Notate and drop support for this constructor
    public SumDiff(params Oper[] ops) : base("sumdiff", ops) { }
    public SumDiff(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("sumdiff", a, b) { }
    IVal total = new Val(0);
    public override Variable Sol()
    {
        total.Set(0);
        foreach (Oper o in posArgs)
            if (o is Variable v)
                IVal.Add(v, total, total);
            else
                IVal.Add(o.Sol(), total, total);
        foreach (Oper o in negArgs)
            if (o is Variable v)
                IVal.Subtract(total, v, total);
            else
                IVal.Subtract(total, o.Sol(), total);
        solution.Value.Set(total);
        return solution;
    }

    public override SumDiff New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }

    public override Oper Degree(Oper v)
    {
        if (IsDetermined)
            return new Variable(0);
        if (Like(v))
            return new Variable(1);
        Oper maxD = new Variable(double.MinValue);
        foreach (Oper o in AllArgs)
        {
            Oper d = o.Degree(v);
            maxD = d > maxD ? d : maxD;
        }
        //return new Commonfuncs.Abs(maxD.Minus(minD));
        return maxD;
    }

    protected override Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive)
    {
        Oper ABbar;
        if (!(aPositive ^ bPositive))
            ABbar = A.Divide(AB).Plus(B.Divide(AB));
        else if (aPositive)
            ABbar = A.Divide(AB).Minus(B.Divide(AB));
        else if (bPositive)
            ABbar = B.Divide(AB).Minus(A.Divide(AB));
        else
            throw Scribe.Issue("haggu!");

        ABbar.Reduce(2);
        Oper combined;
        if (A is Variable av && av.Found && av.Value.EqValue(0))
            combined = bPositive ? B : B.Mult(new Variable(-1));
        else if (B is Variable bv && bv.Found && bv.Value.EqValue(0))
            combined = A;
        else
            combined = AB.Mult(ABbar);
        //combined.ReduceOuter();
        return combined;
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