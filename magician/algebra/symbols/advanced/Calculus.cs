namespace Magician.Alg.Symbols;

using System.Collections.Generic;

public class Derivative : Oper
{
    public Derivative(params Oper[] ops) : base("derivative", ops) { }
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

    public override Variable Sol()
    {
        throw new NotImplementedException();
    }
}