namespace Magician.Symbols;

// TODO: write this once you make Opers generic
public class Form : Oper
{
    public Form(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega) : base(name, posa, nega)
    {
    }

    protected override int identity => throw new NotImplementedException();

    public override double Degree(Variable v)
    {
        throw new NotImplementedException();
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        throw new NotImplementedException();
    }

    public override Variable Solution()
    {
        throw new NotImplementedException();
    }

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
}