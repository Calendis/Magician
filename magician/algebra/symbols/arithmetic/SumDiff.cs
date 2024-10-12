namespace Magician.Alg.Symbols;
using Core;

// SumDiff objects represent addition and subtraction operations with any number of arguments
public class SumDiff : Arithmetic
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

    public override void CombineOuter(Variable? axis = null)
    {
        (Oper, Oper, int idx1, int idx2)[] handshakes = new (Oper, Oper, int, int)[AllArgs.Count * (AllArgs.Count - 1)];
        int c = 0;
        for (int i = 0; i < AllArgs.Count-1; i++)
            for (int j = i; j < AllArgs.Count-1; j++)
                handshakes[c++] = (AllArgs[i], AllArgs[j], i < posArgs.Count ? i : posArgs.Count-1-i, j < posArgs.Count ? j : posArgs.Count-1-j);
                //handshakes[c++] = (AllArgs[i], AllArgs[j], !(i < AllArgs.Count ^ j < AllArgs.Count));

        foreach ((Oper o, Oper p, int oIdx, int pIdx) handshake in handshakes)
        {
            // Find common determined factors
            //(List<Oper> oFacs, List<Oper> pFacs) = ([], []);
            //(List<Oper>, List<Oper>) commonDeterminedFactors = (handshake.o.Factors().Item1.Intersect(handshake.p.Factors().Item1).Where(a => a.IsDetermined).ToList(), handshake.o.Factors().Item2.Intersect(handshake.p.Factors().Item2).Where(a => a.IsDetermined).ToList());
            (List<Oper>, List<Oper>) commonFactors = (handshake.o.Factors().Item1.Intersect(handshake.p.Factors().Item1).ToList(), handshake.o.Factors().Item2.Intersect(handshake.p.Factors().Item2).ToList());
            if (commonFactors.Item1.Count == 0 && commonFactors.Item1.Count == 0)
            {
                continue;
            }
            Fraction ffacs = new Fraction(commonFactors.Item1, commonFactors.Item2);
            Oper oCommon = ffacs.Divide(handshake.o);
            Oper pCommon = ffacs.Divide(handshake.p);
            oCommon.Reduce(1);
            pCommon.Reduce(1);
            // When simplifying a SumDiff, we only factor out constants
            // TODO: unless there is a defined axis, then we may factor it
            if (oCommon.IsDetermined && pCommon.IsDetermined)
            {
                Oper summedCommonFactors = oCommon.Plus(pCommon);
                // TODO: we need to include the polarity of both terms so we know which args list to remove from
                // we could just check. It will lead to odd behaviour if an Oper contains copies but that shouldn't happen
                if (handshake.oIdx < 0)
                {
                    negArgs.RemoveAt(1-handshake.oIdx);
                }
                else
                {
                    posArgs.RemoveAt(handshake.oIdx);
                }
                if (handshake.pIdx < 0)
                {
                    negArgs.RemoveAt(1-handshake.pIdx);
                }
                else
                {
                    posArgs.RemoveAt(handshake.pIdx);
                }
                
                bool positive = !(handshake.oIdx >= 0 ^ handshake.pIdx >= 0);
                (bool oiPos, bool piPos) = (handshake.oIdx >= 0, handshake.pIdx >= 0);
                (int posOi, int posPi) = (oiPos ? handshake.oIdx : 1 - handshake.oIdx, piPos ? handshake.pIdx : 1 - handshake.pIdx);
                if (positive)
                {
                    posArgs.Insert(Math.Min(posOi, posPi), summedCommonFactors);
                }
                else
                {
                    negArgs.Insert(Math.Min(posOi, posPi), summedCommonFactors);
                }
                
            }
            // TODO: account for axis
            else
            {
                //
            }

        }
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