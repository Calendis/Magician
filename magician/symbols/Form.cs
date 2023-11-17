namespace Magician.Symbols;
public class Form
{
    public static bool Term(Oper o)
    {
        if (o is Variable)
            return true;
        if (o is SumDiff)
            return false;
        bool term = true;
        o.AllArgs.ForEach(a => term &= Term(a));
        return term;
    }

    public static Oper Canonical(Oper o)
    {
        o.Associate();
        o.Reduce();
        o.Commute();
        return o.Reduced();
    }
}