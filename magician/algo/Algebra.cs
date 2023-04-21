namespace Magician.Algo;
using static MathCache;

public static class Algebra
{
    // Create a free variable
    public static Variable Let(string name)
    {
        if (freeVars.ContainsKey(name))
        {
            //return freeVars[name];
        }
        Variable v = new Variable(name);
        //freeVars.Add(name, v);
        return v;
    }

    public static Variable Let(string name, double x)
    {
        Variable v = new Variable(name);
        v.Coefficient = x;
        return v;
    }

    // Denote a number
    public static Variable N(double v)
    {
        return new Variable(v);
    }
}
static class MathCache
{
    public static Dictionary<string, Variable> freeVars = new();
}
