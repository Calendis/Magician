namespace Magician.Alg.Symbols;
// TODO: make this an interface

public abstract class Invertable : Oper
{
    protected Invertable(string name, params Oper[] cstArgs) : base(name, cstArgs)
    {
    }

    protected Invertable(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega) : base(name, posa, nega)
    {
    }

    public abstract Oper Inverse(Oper axis, Oper? opposite);
}