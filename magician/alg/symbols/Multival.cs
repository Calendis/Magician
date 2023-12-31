namespace Magician.Symbols;

public class Multival : Variable, Maps.IRelation
{
    public Multival(int divisions, params double[] vs) : base(vs)
    {
        //
    }
}