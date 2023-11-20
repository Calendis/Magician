namespace Magician.Symbols;

// SumDiff objects represent addition and subtraction operations with any number of arguments
public class SumDiff : Arithmetic
{
    protected override int? Identity { get => 0; }

    // TODO: expand Notate and drop support for this constructor
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        commutative = true;
        associative = true;
    }
    public SumDiff(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("sumdiff", a, b)
    {
        commutative = true;
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

    public override SumDiff New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }
    public static SumDiff StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }

    public override Oper Degree(Variable v)
    {
        if (IsDetermined)
            return new Variable(0);
        Oper minD = new Variable(0);
        Oper maxD = new Variable(1);
        foreach (Oper o in AllArgs)
        {
            Oper d = o.Degree(v);
            minD = d < minD ? d : minD;
            maxD = d > maxD ? d : maxD;
        }
        return Form.Canonical(new Funcs.Abs(maxD.Subtract(minD)));
    }

    protected override Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive)
    {
        Oper ABbar;
        if (!(aPositive ^ bPositive))
            ABbar = A.Divide(AB).Add(B.Divide(AB));
        else if (aPositive)
            ABbar = A.Divide(AB).Subtract(B.Divide(AB));
        else if (bPositive)
            ABbar = B.Divide(AB).Subtract(A.Divide(AB));
        else
            throw Scribe.Issue("haggu!");

        Oper combined = AB.Mult(ABbar);
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