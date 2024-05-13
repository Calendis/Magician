/* namespace Magician.Alg;
using Magician.Alg.Symbols;

public static class InstantiationAssociator
{
    static Oper? key;
    static List<Variable> associates = new();

    public static bool TryLock(Oper o)
    {
        if (key is null)
        {
            key = o;
            return true;
        }
        return false;
    }
    public static bool TryUnlock(Oper o)
    {
        if (key == o)
        {
            key = null;
            associates.Clear();
            return true;
        }
        return false;
    }
} */