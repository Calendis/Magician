namespace Magician.Symbols;
using static MathCache;

public static class Notate
{
    // Denote an unknown
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
