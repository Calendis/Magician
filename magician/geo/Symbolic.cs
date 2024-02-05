namespace Magician.Geo;
using Core.Maps;
using Alg.Symbols;

public class Symbolic : Implicit
{
    public Symbolic(Oper o, double x, double y, double z, double inScale, double outScale, params (double, double, double)[] rangeResos) : base(o, x, y, z, inScale, outScale, rangeResos)
    {
        Scribe.Info($"relation is Oper? {relation is Oper}");
    }

    //public static Mesh MeshFromOper(Oper o)
    //{
    //    //
    //}
}