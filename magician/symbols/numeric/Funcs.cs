namespace Magician.Symbols.Numeric;

public static class Funcs
{
    public static double Sin(double x)
    {
        return Math.Sin(x);
    }
    public static double Cos(double x)
    {
        if (x % (Math.PI/2) == 0)
            return 0;
        return Math.Cos(x);
    }
}