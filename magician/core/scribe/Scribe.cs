namespace Magician;
/* Scribe is the logger */
public static class Scribe
{
    public static void Info(string? s)
    {
        Console.Write($"INFO: {s}\n");
    }
    public static void Info(object? o)
    {
        if (o == null)
        {
            Info("NULL");
            return;
        }
        Info(o.ToString());
        return;
    }

    public static void Warn(string s)
    {
        Console.Write($"WARNING: {s}\n");
    }
    public static void Warn(object? o)
    {
        if (o == null)
        {
            Warn("NULL");
            return;
        }
        Warn(o.ToString()!);
        return;
    }
    public static void WarnIf(bool condition, string s)
    {
        if (condition)
            Warn(s);
    }
    public static void List<T>(List<T> l)
    {
        if (l == null)
        {
            Scribe.Warn($"Could not list null");
            l = new List<T>();
        }
        int c = 0;
        foreach (T o in l)
        {
            Console.WriteLine($"..{c++}: {o}");
        }
    }

    public static void List<T>(params T[] l)
    {
        List(l.ToList());
    }

    /// <summary><exception>
    /// bruhException
    /// </exception></summary>
    public static MagicianError Error(string s)
    {
        Console.Write($"ERROR: {s}\n");
        return new MagicianError(s);
    }
    public static MagicianError Issue(string s)
    {
        Console.WriteLine($"ERROR: {s}\nPlease file an issue at https://github.com/Calendis");
        return new MagicianError(s);
    }

    public class MagicianError : Exception
    {
        public MagicianError(string s) : base(s) { }
    }
}