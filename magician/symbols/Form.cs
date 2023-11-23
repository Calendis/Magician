
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
    public static Oper Canonical(Oper o)
    {
        Oper p = o.Copy();
        p.SimplifyFullAll();
        p.Reduce();
        p.Commute();
        return Shed(p);
    }

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

public class Form : Oper
{
    public Form(Oper o) : base("typeform_placeholder_name", o.posArgs, o.negArgs)
    {
        //
    }

    public override Oper Degree(Variable v)
    {
        throw new NotImplementedException();
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        throw new NotImplementedException();
    }

    public override void ReduceOuter()
    {
        throw new NotImplementedException();
    }

    public override Variable Solution()
    {
        throw new NotImplementedException();
    }
}