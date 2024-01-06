
namespace Magician.Symbols;
// TODO: move this stuff to Oper
public class LegacyForm
{
    public static bool IsTerm(Oper o)
    {
        if (o is Variable)
            return true;
        if (o is SumDiff)
            return false;
        bool term = true;
        o.AllArgs.ForEach(a => term &= IsTerm(a));
        return term;
    }

    // TODO: write tests to make sure canonical and SimplifyFull actually simplify all the way!
    public static Oper Canonical(Oper o, bool makeCopy=true)
    {
        Oper p = o.Copy();
        p.Reduce();
        p.Commute();
        p.SimplifyMax();
        return Shed(p);
    }

    // TODO: you can possibly make this a Form type like Fraction(n->PTRL)
    //public static Oper Term(Oper o)
    //{
    //    if (o is Variable || o is Fraction)
    //        return o;
    //    return new Fraction(o);
    //}

    // TODO: move this back to Oper.Trim
    public static Oper Shed(Oper o)
    {
        if (o.IsTrivial)
            return Shed(o.posArgs[0]);
        return o;
    }
}