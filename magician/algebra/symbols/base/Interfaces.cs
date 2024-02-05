namespace Magician.Alg.Symbols;
using Core.Maps;

public interface IDifferentiable
{
    public IRelation Derivative();
}

public interface IAnalytic : IDifferentiable
{
    public new IAnalytic Derivative();
}