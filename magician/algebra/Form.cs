
namespace Magician.Alg.Symbols;
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
}