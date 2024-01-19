namespace Magician.Alg.Numeric;

public static class Trig
{
    public static double Sin(double x)
    {
        if (x % Math.PI == 0)
            return 0;
        return Math.Sin(x);
    }
    public static double Cos(double x)
    {
        if ((x+Math.PI/2) % Math.PI == 0)
            return 0;
        return Math.Cos(x);
    }
}

public static class Rand
{
    public static Random RNG = new Random();
    public static double RandX => RNG.NextDouble() * Runes.Globals.winWidth - Runes.Globals.winWidth / 2;
    public static double RandY => RNG.NextDouble() * Runes.Globals.winHeight - Runes.Globals.winHeight / 2;
}