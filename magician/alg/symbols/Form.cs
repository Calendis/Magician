
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
        //Scribe.Info($"  Canonicalizing {o}");
        Oper p;
        if (makeCopy)
            p = o.Copy();
        else
            p = o;
        //Scribe.Info($"  ...partial {p}");

        for (int i = 0; i < p.posArgs.Count; i++)
        {
            p.posArgs[i] = Canonical(p.posArgs[i], false);
        }
        for (int i = 0; i < p.negArgs.Count; i++)
        {
            p.negArgs[i] = Canonical(p.negArgs[i], false);
        }
        p.SimplifyAll();
        p.Commute();
        //Scribe.Info($"  ...to-shed {p}");
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

public class Form : Oper
{
    public Form(Oper o) : base("typeform_placeholder_name", o.posArgs, o.negArgs)
    {
        //
    }

    public override Oper Degree(Oper v)
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