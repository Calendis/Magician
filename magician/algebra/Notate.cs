namespace Magician.Alg;
using Symbols;

using static MathCache;

public static class Notate
{
    // Denote and access variables with a string
    public static Variable Var(string name)
    {
        if (freeVars.ContainsKey(name))
        {
            return freeVars[name];
        }
        Variable v = new Variable(name);
        freeVars.Add(name, v);
        return freeVars[name];
    }

    // Denote a number
    public static Variable Val(double v)
    {
        return new Variable(v);
    }
}
static class MathCache
{
    public static Dictionary<string, Variable> freeVars = new();
    public static void Clear()
    {
        freeVars.Clear();
    }
}
